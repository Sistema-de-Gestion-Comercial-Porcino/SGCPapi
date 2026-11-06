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

[ApiController, Route("api/v1/clasificaciones"), Authorize(Policy = "catalogo.consultar")]
public class ClasificacionesController(SgcpContext db, ServicioCatalogo servicio) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(TipoProducto? familia, bool? activo, string? busqueda, int pagina = 1, int tamanoPagina = 20)
    {
        var q = db.ClasificacionesProducto.AsNoTracking();
        if (familia is not null)
            q = q.Where(x => x.Familia == familia);
        if (activo is not null)
            q = q.Where(x => x.Activo == activo);
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Nombre.Contains(busqueda) || x.Codigo.Contains(busqueda));
        return Ok(await q.OrderBy(x => x.Nombre).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Ver(Guid id) => Ok(await db.ClasificacionesProducto.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO",
            "Clasificación no encontrada.",
            404));
    [HttpPost,
            Authorize(Policy = "catalogo.administrar")]
    public async Task<IActionResult> Crear(ClasificacionPeticion p) => StatusCode(201,
            await servicio.Clasificacion(null, p));
    [HttpPut("{id:guid}"),
            Authorize(Policy = "catalogo.administrar")]
    public async Task<IActionResult> Editar(Guid id,
            ClasificacionPeticion p) => Ok(await servicio.Clasificacion(id, p));
    [HttpPost("{id:guid}/estado"),
            Authorize(Policy = "catalogo.administrar")]
    public async Task<IActionResult> Estado(Guid id,
            EstadoPeticion p)
    { await servicio.EstadoClasificacion(id, p.Activo); return NoContent(); }
}
