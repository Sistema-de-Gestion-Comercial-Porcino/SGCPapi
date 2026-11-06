using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Contratos.Respuestas;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
namespace SGCP.Api.Servicios;

public class ServicioAutenticacion(SgcpContext db,
        GeneradorToken tokens,
        IConfiguration config,
        ServicioAuditoria auditoria,
        UnidadTrabajo unidad,
        EntregaRecuperacion entrega)
{
    private readonly PasswordHasher<Usuario> hasher = new();
    public async Task<AccesoRespuesta> Acceder(AccesoPeticion p)
    {
        var login = p.Login.Trim().ToLowerInvariant();
        var u = await db.Usuarios.Include(x => x.Roles).ThenInclude(x => x.Permisos).SingleOrDefaultAsync(x => x.Login == login);
        if (u is null)
        {
            hasher.VerifyHashedPassword(new Usuario(),
                HashDummy,
                p.Clave);
            throw new ReglaException("CREDENCIALES_INVALIDAS",
                "Usuario o contraseña incorrectos.",
                401);
        }
        if (u.Estado != EstadoUsuario.ACTIVO)
            throw new ReglaException("CUENTA_NO_HABILITADA", "Tu cuenta requiere habilitación de administración.", 403);
        if (hasher.VerifyHashedPassword(u, u.HashClave, p.Clave) == PasswordVerificationResult.Failed)
        {
            u.IntentosFallidos++;
            if (u.IntentosFallidos >= Math.Max(1, config.GetValue<int?>("Seguridad:MaximoIntentos") ?? 5))
            { u.Estado = EstadoUsuario.BLOQUEADO; foreach (var s in await db.Sesiones.Where(x => x.UsuarioId == u.Id).ToListAsync()) s.Revocada = true; }
            auditoria.Registrar("ACCESO_FALLIDO", "Usuario", u.Id, u.Id);
            await db.SaveChangesAsync();
            throw new ReglaException("CREDENCIALES_INVALIDAS", "Usuario o contraseña incorrectos.", 401);
        }
        u.IntentosFallidos = 0;
        u.UltimoAcceso = DateTime.UtcNow;
        var sesion = new Sesion { UsuarioId = u.Id, Inicio = DateTime.UtcNow, Expira = DateTime.UtcNow.AddHours(8) };
        db.Sesiones.Add(sesion);
        auditoria.Registrar("INICIAR_SESION", "Sesion", sesion.Id, u.Id);
        await db.SaveChangesAsync();
        return tokens.Crear(u, sesion);
    }
    private static readonly string HashDummy = new PasswordHasher<Usuario>()
        .HashPassword(new Usuario(), Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
    public Task<object> Registrar(RegistroCompradorPeticion p) => unidad.Ejecutar<object>(async () =>
    {
        var login = p.Login.Trim().ToLowerInvariant();
        var correo = p.Correo.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.Login == login || x.Correo == correo))
            throw new ReglaException("CUENTA_DUPLICADA", "El usuario o correo ya está registrado.");
        var cliente = new Cliente
        {
            Nombre = p.Nombre.Trim(),
            Tipo = "Consumidor final",
            Telefono = p.Telefono?.Trim() ?? "",
            FechaAlta = DateTime.UtcNow
        };
        var u = new Usuario
        {
            Nombre = p.Nombre.Trim(),
            Login = login,
            Correo = correo,
            Telefono = p.Telefono,
            Cliente = cliente,
            CreadoEn = DateTime.UtcNow,
            Roles = [await db.Roles.SingleAsync(x => x.Nombre == "COMPRADOR" && x.Activo)]
        };
        u.HashClave = hasher.HashPassword(u, p.Clave);
        db.Clientes.Add(cliente);
        db.Usuarios.Add(u);
        auditoria.Registrar("REGISTRO_COMPRADOR", "Usuario", u.Id, u.Id);
        auditoria.Registrar("CREAR", "Cliente", cliente.Id, u.Id);
        return new { u.Id, Mensaje = "Cuenta creada. Ya puedes iniciar sesión." };
    });
    public async Task Recuperar(string correo)
    {
        var normal = correo.Trim().ToLowerInvariant();
        var u = await db.Usuarios.SingleOrDefaultAsync(x => x.Correo == normal && x.Estado == EstadoUsuario.ACTIVO);
        if (u is null)
            return;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await unidad.Ejecutar(async () =>
        {
            foreach (var anterior in await db.RecuperacionesAcceso.Where(x => x.UsuarioId == u.Id
                && !x.Invalidado
                && x.UtilizadoEn == null).ToListAsync())
                anterior.Invalidado = true;
            db.RecuperacionesAcceso.Add(new RecuperacionAcceso
            {
                UsuarioId = u.Id,
                TokenHash = Hash(token),
                SolicitadoEn = DateTime.UtcNow,
                ExpiraEn = DateTime.UtcNow.AddMinutes(30)
            });
            auditoria.Registrar("SOLICITAR_RECUPERACION", "Usuario", u.Id, u.Id);
            return true;
        });
        await entrega.Enviar(u.Correo, token);
    }
    public Task<bool> Restablecer(RestablecerPeticion p) => unidad.Ejecutar(async () =>
    {
        var hash = Hash(p.Token);
        var r = await db.RecuperacionesAcceso.Include(x => x.Usuario).SingleOrDefaultAsync(x => x.TokenHash == hash);
        if (r is null
            || r.Invalidado
            || r.UtilizadoEn is not null
            || r.ExpiraEn <= DateTime.UtcNow
            || r.Usuario.Estado != EstadoUsuario.ACTIVO)
            throw new ReglaException("TOKEN_NO_VALIDO", "El enlace venció o ya fue utilizado.", 400);
        r.UtilizadoEn = DateTime.UtcNow;
        r.Usuario.HashClave = hasher.HashPassword(r.Usuario, p.Clave);
        r.Usuario.IntentosFallidos = 0;
        foreach (var s in await db.Sesiones.Where(x => x.UsuarioId == r.UsuarioId).ToListAsync())
            s.Revocada = true;
        foreach (var otro in await db.RecuperacionesAcceso.Where(x => x.UsuarioId == r.UsuarioId && x.Id != r.Id).ToListAsync())
            otro.Invalidado = true;
        auditoria.Registrar("RESTABLECER_ACCESO", "Usuario", r.UsuarioId, r.UsuarioId);
        return true;
    });
    public static string Hash(string texto) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
}
