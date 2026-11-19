using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
namespace SGCP.Api.Servicios;

public class ServicioUsuarios(SgcpContext db, ActorActual actor, UnidadTrabajo unidad, ServicioAuditoria auditoria)
{
    public IQueryable<Usuario> Consulta() => db.Usuarios.Include(x => x.Roles);
    public bool PuedeGestionar(Usuario u) => actor.Principal.IsInRole("SUPERADMIN") ||
        (u.Id != actor.Id && (actor.Principal.IsInRole("PROPIETARIO") || !u.Roles.Any(r => r.Activo && r.Nombre == "FINANZAS")) &&
         RolesSistema.Nivel(actor.Principal) > u.Roles.Where(r => r.Activo).Select(r => RolesSistema.Nivel(r.Nombre)).DefaultIfEmpty(0).Max());
    private void VerificarGestion(Usuario u)
    {
        if (!PuedeGestionar(u))
            throw new ReglaException("JERARQUIA_ROLES", "No puedes modificar esta cuenta ni tus propios privilegios.", 403);
    }
    public static object Vista(Usuario u) => new
    {
        u.Id,
        u.Nombre,
        u.Login,
        u.Correo,
        u.Telefono,
        u.Estado,
        u.UltimoAcceso,
        u.ClienteId,
        Roles = u.Roles.Select(r => new { r.Id, r.Nombre }).ToArray()
    };
    public Task<object> Guardar(Guid? id, UsuarioPeticion p) => unidad.Ejecutar<object>(async () =>
    {
        var u = id is null ? new Usuario { CreadoEn = DateTime.UtcNow } : await Consulta()
            .SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO", "Usuario no encontrado.", 404);
        var roles = await db.Roles.Include(x => x.Permisos).Where(x => p.Roles.Contains(x.Id) && x.Activo).ToListAsync();
        if (id is not null) VerificarGestion(u);
        if (roles.Any(r => !RolesSistema.Asignable(actor.Principal, r.Nombre)) && u.ClienteId is null)
            throw new ReglaException("JERARQUIA_ROLES", "No puedes asignar este nivel de acceso.", 403);
        if (roles.Count != p.Roles.Distinct().Count() || roles.Count == 0)
            throw new ReglaException("ROLES_INVALIDOS", "Selecciona responsabilidades habilitadas.", 400);
        if (roles.Any(x => x.Nombre == "COMPRADOR") && (id is null || u.ClienteId is null || roles.Count != 1))
            throw new ReglaException("ROL_COMPRADOR",
                    "La cuenta del comprador se crea mediante registro público y no recibe responsabilidades internas.",
                    400);
        if (u.ClienteId is not null && roles.Any(x => x.Nombre != "COMPRADOR"))
            throw new ReglaException("CUENTA_COMPRADOR", "La cuenta comercial del comprador no puede convertirse en cuenta interna.");
        if (!actor.Tiene("usuarios.administrar") && roles.SelectMany(x => x.Permisos).Any(x => !actor.Tiene(x.Codigo)))
            throw new ReglaException("SIN_PERMISO", "No puedes asignar permisos que no posees.", 403);
        if (id is not null
            && u.Estado == EstadoUsuario.ACTIVO
            && u.Roles.Any(x => x.Nombre == "SUPERADMIN")
            && !roles.Any(x => x.Nombre == "SUPERADMIN"))
            await ProtegerAdministrador(u.Id);
        u.Nombre = p.Nombre.Trim();
        u.Login = p.Login.Trim().ToLowerInvariant();
        u.Correo = p.Correo.Trim().ToLowerInvariant();
        u.Telefono = p.Telefono?.Trim();
        u.Roles = roles;
        if (await db.Usuarios.AnyAsync(x => x.Id != u.Id && (x.Login == u.Login || x.Correo == u.Correo)))
            throw new ReglaException("USUARIO_DUPLICADO", "Login o correo ya registrados.");
        if (id is null && string.IsNullOrWhiteSpace(p.Clave))
            throw new ReglaException("CLAVE_REQUERIDA", "Define una contraseña inicial.", 400);
        if (!string.IsNullOrEmpty(p.Clave))
        {
            if (p.Clave.Length < 10)
                throw new ReglaException("CLAVE_CORTA",
                "La contraseña requiere 10 caracteres.",
                400);
            u.HashClave = new PasswordHasher<Usuario>().HashPassword(u,
                p.Clave);
        }
        if (id is null)
            db.Usuarios.Add(u);
        else
            foreach (var s in await db.Sesiones.Where(x => x.UsuarioId == u.Id).ToListAsync())
                s.Revocada = true;
        auditoria.Registrar(id is null ? "CREAR" : "EDITAR_PERMISOS", "Usuario", u.Id);
        return Vista(u);
    });
    public Task<bool> Estado(Guid id, EstadoUsuario estado) => unidad.Ejecutar(async () =>
    {
        var u = await Consulta()
            .SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO", "Usuario no encontrado.", 404);
        VerificarGestion(u);
        if (u.Estado == EstadoUsuario.ACTIVO && estado != EstadoUsuario.ACTIVO && u.Roles.Any(x => x.Nombre == "SUPERADMIN"))
            await ProtegerAdministrador(id);
        u.Estado = estado;
        if (estado == EstadoUsuario.ACTIVO)
            u.IntentosFallidos = 0;
        foreach (var s in await db.Sesiones.Where(x => x.UsuarioId == id).ToListAsync())
            s.Revocada = true;
        auditoria.Registrar("ESTADO_" + estado, "Usuario", id);
        return true;
    });
    private async Task ProtegerAdministrador(Guid id)
    {
        if (!await db.Usuarios.AnyAsync(x => x.Id != id
            && x.Estado == EstadoUsuario.ACTIVO
            && x.Roles.Any(r => r.Nombre == "SUPERADMIN" && r.Activo)))
            throw new ReglaException("ULTIMO_ADMINISTRADOR", "Debe permanecer al menos un superadmin habilitado.");
    }
}
