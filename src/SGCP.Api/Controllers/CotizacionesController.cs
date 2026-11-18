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

[ApiController, Route("api/v1/cotizaciones"), Authorize(Policy = "cotizaciones.consultar")]
public class CotizacionesController(ServicioCotizaciones servicio, SgcpContext db, ActorActual actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(string? busqueda,
            EstadoCotizacion? estado,
            Guid? clienteId,
            int pagina = 1,
            int tamanoPagina = 20)
    {
        await servicio.Vencer();
        var q = servicio.Consulta().AsNoTracking();
        if (actor.Comprador)
            q = q.Where(x => x.ClienteId == actor.ClienteId);
        if (clienteId is not null)
            q = q.Where(x => x.ClienteId == clienteId);
        if (estado is not null)
            q = q.Where(x => x.Estado == estado);
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Codigo.Contains(busqueda) || x.Cliente.Nombre.Contains(busqueda));
        return Ok(await q.OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Select(x => new
        {
            x.Id,
            x.Codigo,
            x.Version,
            x.Estado,
            x.Fecha,
            x.VigenteHasta,
            x.ClienteId,
            Cliente = x.Cliente.Nombre
        }).Paginar(pagina, tamanoPagina));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Ver(Guid id)
    {
        await servicio.Vencer();
        var c = await servicio.Obtener(id);
        var posterior = await db.Cotizaciones.Where(x => x.CotizacionOrigenId == id)
            .Select(x => new { x.Id, x.Codigo, x.Version })
            .SingleOrDefaultAsync();
        return Ok(new
        {
            c.Id,
            c.Codigo,
            c.Version,
            c.Fecha,
            c.VigenteHasta,
            c.Canal,
            c.Estado,
            c.MotivoRespuesta,
            c.ClienteId,
            Cliente = c.Cliente.Nombre,
            c.CotizacionOrigenId,
            VersionPosterior = posterior,
            PedidoId = c.Pedido?.Id,
            Total = c.Detalles.Sum(x => x.Subtotal),
            Detalles = c.Detalles.Select(x => new
            {
                x.Id,
                x.ProductoId,
                Producto = x.Producto.Nombre,
                x.Cantidad,
                x.Unidad,
                x.PrecioUnitario,
                x.Subtotal
            })
        });
    }
    [HttpPost,
            Authorize(Policy = "cotizaciones.administrar")]
    public async Task<IActionResult> Crear(CotizacionPeticion p)
    {
        var id = await servicio.Guardar(null, p);
        // 201 con Location: GET cotizaciones/{id}.
        return CreatedAtAction(nameof(Ver), new { id }, new { Id = id });
    }
    [HttpPut("{id:guid}"),
            Authorize(Policy = "cotizaciones.administrar")]
    public async Task<IActionResult> Editar(Guid id,
            CotizacionPeticion p) => Ok(new { Id = await servicio.Guardar(id, p) });
    [HttpPost("{id:guid}/presentar"), Authorize(Policy = "cotizaciones.administrar")] public async Task<IActionResult> Presentar(Guid id) { await servicio.Cambiar(id, EstadoCotizacion.PRESENTADA); return NoContent(); }
    [HttpPost("{id:guid}/aceptar"), Authorize(Policy = "cotizaciones.responder")] public async Task<IActionResult> Aceptar(Guid id) { await servicio.Cambiar(id, EstadoCotizacion.ACEPTADA); return NoContent(); }
    [HttpPost("{id:guid}/rechazar"),
            Authorize(Policy = "cotizaciones.responder")]
    public async Task<IActionResult> Rechazar(Guid id,
            MotivoPeticion p)
    { await servicio.Cambiar(id, EstadoCotizacion.RECHAZADA, p.Motivo); return NoContent(); }
    [HttpPost("{id:guid}/nueva-version"),
            Authorize(Policy = "cotizaciones.administrar")]
    public async Task<IActionResult> Version(Guid id,
            NuevaVersionPeticion p) => StatusCode(201,
            new { Id = await servicio.NuevaVersion(id, p.VigenteHasta) });
}
