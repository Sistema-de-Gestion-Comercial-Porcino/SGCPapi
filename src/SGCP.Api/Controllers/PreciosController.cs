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

[ApiController, Authorize(Policy = "costos.consultar")]
public class PreciosController(SgcpContext db, ServicioPrecios servicio) : ControllerBase
{
    [HttpGet("api/v1/productos/{id:guid}/precios")]
    public async Task<IActionResult> Listar(Guid id) => Ok(await db.PreciosProducto.Where(x => x.ProductoId == id).OrderByDescending(x => x.VigenteDesde).Select(x => new
    {
        x.Id,
        x.ProductoId,
        x.CostoReferencia,
        x.PrecioVenta,
        x.VigenteDesde,
        x.VigenteHasta,
        Margen = x.PrecioVenta - x.CostoReferencia
    }).ToListAsync());
    [HttpPost("api/v1/productos/{id:guid}/precios"),
            Authorize(Policy = "costos.administrar")]
    public async Task<IActionResult> Abrir(Guid id,
            PrecioPeticion p) => StatusCode(201,
            await servicio.Abrir(id, p));
    [HttpPut("api/v1/precios/{id:guid}"),
            Authorize(Policy = "costos.administrar")]
    public async Task<IActionResult> Editar(Guid id,
            PrecioPeticion p) => Ok(await servicio.Editar(id, p));
    [HttpPost("api/v1/precios/{id:guid}/finalizar"),
            Authorize(Policy = "costos.administrar")]
    public async Task<IActionResult> Finalizar(Guid id,
            FinalizarPrecioPeticion p)
    { await servicio.Finalizar(id, p.VigenteHasta); return NoContent(); }
}
