using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Contratos.Respuestas;
namespace SGCP.Api.Seguridad;

public class GeneradorToken(IConfiguration config)
{
    public static UsuarioResumen Resumen(Usuario usuario) => new(usuario.Id,
            usuario.Nombre,
            usuario.Login,
            usuario.Roles.Where(r => r.Activo).Select(r => r.Nombre).ToArray(),
            PermisosEfectivos(usuario),
            usuario.ClienteId);
    public static string[] PermisosEfectivos(Usuario usuario)
    {
        var roles = usuario.Roles.Where(r => r.Activo).ToArray();
        var restringir = roles.Any(r => r.Nombre == "ADMINISTRADOR") &&
            !roles.Any(r => r.Nombre is "SUPERADMIN" or "PROPIETARIO" or "FINANZAS");
        return roles.SelectMany(r => r.Permisos).Select(p => p.Codigo)
            .Where(p => !restringir || !RolesSistema.Financieros.Contains(p)).Distinct().ToArray();
    }
    public static List<Claim> Claims(Usuario usuario, Guid sesion)
    {
        var datos = Resumen(usuario);
        List<Claim> claims = [new("sub", usuario.Id.ToString()),
                new("sid", sesion.ToString()),
                new("name", usuario.Nombre),
                new("login", usuario.Login)];
        claims.AddRange(datos.Roles.Select(x => new Claim("rol", x)));
        claims.AddRange(datos.Permisos.Select(x => new Claim("permiso", x)));
        if (usuario.ClienteId is not null)
            claims.Add(new("clienteId", usuario.ClienteId.ToString()!));
        return claims;
    }
    public AccesoRespuesta Crear(Usuario usuario, Sesion sesion)
    {
        var token = new JwtSecurityToken(config["Jwt:Emisor"],
                config["Jwt:Audiencia"],
                Claims(usuario, sesion.Id),
                DateTime.UtcNow,
                sesion.Expira,
                new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Clave"]!)), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), sesion.Expira, Resumen(usuario));
    }
}
