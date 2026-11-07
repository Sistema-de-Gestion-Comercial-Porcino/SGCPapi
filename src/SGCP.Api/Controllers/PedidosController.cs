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

[ApiController, Route("api/v1/pedidos"), Authorize(Policy = "pedidos.consultar")]
public class PedidosController(SgcpContext db, ServicioConfirmarPedido servicio, ActorActual actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(string? busqueda,
            EstadoComercial? estadoComercial,
            Guid? clienteId,
            int pagina = 1,
            int tamanoPagina = 20)
    {
        var q = db.Pedidos.AsNoTracking();
        if (actor.Comprador)
            q = q.Where(x => x.ClienteId == actor.ClienteId);
        if (clienteId is not null)
            q = q.Where(x => x.ClienteId == clienteId);
        if (estadoComercial is not null)
            q = q.Where(x => x.EstadoComercial == estadoComercial);
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Codigo.Contains(busqueda) || x.Cliente.Nombre.Contains(busqueda));
        return Ok(await q.OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Select(x => new
        {
            x.Id,
            x.Codigo,
            x.Fecha,
            x.FechaComprometida,
            x.EstadoComercial,
            x.EstadoPreparacion,
            x.ClienteId,
            Cliente = x.Cliente.Nombre
        }).Paginar(pagina, tamanoPagina));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Ver(Guid id)
    {
        var p = await db.Pedidos.AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Cotizacion)
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO", "Pedido no encontrado.", 404);
        actor.VerCliente(p.ClienteId);
        var costos = actor.Tiene("costos.consultar") && !actor.Comprador;
        return Ok(new
        {
            p.Id,
            p.Codigo,
            p.Fecha,
            p.FechaComprometida,
            p.EstadoComercial,
            p.EstadoPreparacion,
            p.CondicionPago,
            p.ClienteId,
            Cliente = p.Cliente.Nombre,
            p.CotizacionId,
            Cotizacion = p.Cotizacion.Codigo,
            p.MotivoCancelacion,
            CostoEstimadoConfirmado = costos ? (decimal?)p.CostoEstimadoConfirmado : null,
            MargenEstimadoConfirmado = costos ? (decimal?)p.MargenEstimadoConfirmado : null,
            Total = Consultas.Dinero(p.Detalles.Sum(x => x.CantidadConfirmada * x.PrecioAcordado)),
            Detalles = p.Detalles.Select(x => new
            {
                x.Id,
                x.ProductoId,
                Producto = x.Producto.Nombre,
                x.CantidadSolicitada,
                x.CantidadConfirmada,
                x.Unidad,
                x.PrecioAcordado,
                CostoReferenciaAcordado = costos ? (decimal?)x.CostoReferenciaAcordado : null,
                x.Estado,
                Subtotal = Consultas.Dinero(x.CantidadConfirmada * x.PrecioAcordado)
            })
        });
    }
    [HttpPost,
            Authorize(Policy = "pedidos.administrar")]
    public async Task<IActionResult> Confirmar(PedidoPeticion p) => StatusCode(201,
            new
            {
                Id = await servicio.ConfirmarAsync(p)
            });
    [HttpPost("{id:guid}/cancelar"),
            Authorize(Policy = "pedidos.administrar")]
    public async Task<IActionResult> Cancelar(Guid id,
            MotivoPeticion p)
    {
        await servicio.Cancelar(id, p.Motivo);
        return NoContent();
    }
}
