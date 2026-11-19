using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api.Controllers;

[ApiController, Route("api/v1/usuarios"), Authorize(Policy = "usuarios.consultar")]
public class UsuariosController(ServicioUsuarios servicio) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(string? busqueda, EstadoUsuario? estado, Guid? rol, int pagina = 1, int tamanoPagina = 20)
    {
        var q = servicio.Consulta().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Nombre.Contains(busqueda) || x.Login.Contains(busqueda));
        if (estado is not null)
            q = q.Where(x => x.Estado == estado);
        if (rol is not null)
            q = q.Where(x => x.Roles.Any(r => r.Id == rol));
        var r = await q.OrderBy(x => x.Nombre).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina);
        return Ok(new
        {
            Elementos = r.Elementos.Select(ServicioUsuarios.Vista),
            r.Pagina,
            r.TamanoPagina,
            r.TotalRegistros,
            r.TotalPaginas
        });
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Ver(Guid id) => Ok(ServicioUsuarios.Vista(await servicio.Consulta().SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO",
            "Usuario no encontrado.",
            404)));
    [HttpPost,
            Authorize(Policy = "usuarios.administrar")]
    public async Task<IActionResult> Crear(UsuarioPeticion p) => StatusCode(201,
            await servicio.Guardar(null, p));
    [HttpPut("{id:guid}"),
            Authorize(Policy = "usuarios.administrar")]
    public async Task<IActionResult> Editar(Guid id,
            UsuarioPeticion p) => Ok(await servicio.Guardar(id, p));
    [HttpPost("{id:guid}/estado"),
            Authorize(Policy = "usuarios.administrar")]
    public async Task<IActionResult> Estado(Guid id,
            EstadoUsuarioPeticion p)
    { await servicio.Estado(id, p.Estado); return NoContent(); }
}
[ApiController, Authorize(Policy = "usuarios.consultar")]
public class RolesController(SgcpContext db, ActorActual actor) : ControllerBase
{
    [HttpGet("api/v1/roles")]
    public async Task<IActionResult> Roles(bool asignables = false) => Ok((await db.Roles.Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => new
    {
        x.Id,
        x.Nombre,
        x.Descripcion,
        x.Activo,
        Permisos = x.Permisos.Select(p => p.Codigo).ToArray()
    }).ToListAsync()).Where(x => !asignables || RolesSistema.Asignable(actor.Principal, x.Nombre)));
    [HttpGet("api/v1/permisos")]
    public async Task<IActionResult> Permisos() => Ok(await db.Permisos.OrderBy(x => x.Codigo).Select(x => new
    {
        x.Id,
        x.Codigo,
        x.Descripcion,
        x.Modulo,
        x.Accion
    }).ToListAsync());
    [HttpGet("api/v1/auditoria"), Authorize(Policy = "auditoria.consultar")]
    public async Task<IActionResult> Auditoria(int pagina = 1, int tamanoPagina = 20) => Ok(await db.RegistrosAuditoria.OrderByDescending(x => x.Fecha).Select(x => new
    {
        x.Id,
        x.Fecha,
        x.Operacion,
        x.Entidad,
        x.EntidadId,
        Usuario = x.Usuario.Login
    }).Paginar(pagina, tamanoPagina));
}
