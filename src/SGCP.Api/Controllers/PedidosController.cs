using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Contratos.Respuestas;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api.Controllers;

[ApiController, Route("api/v1/pedidos"), Authorize(Policy = "pedidos.consultar")]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status403Forbidden)]
public class PedidosController(SgcpContext db, ServicioConfirmarPedido servicio, ActorActual actor) : ControllerBase
{
    /// <summary>Lista los pedidos visibles para la sesión con filtros y paginación.</summary>
    /// <param name="busqueda">Código de pedido o nombre del cliente.</param>
    /// <param name="estadoComercial">Estado comercial del pedido.</param>
    /// <param name="clienteId">Identificador del cliente.</param>
    /// <param name="pagina">Número de página desde 1.</param>
    /// <param name="tamanoPagina">Cantidad por página. Admite 10, 20, 50 o 100.</param>
    /// <response code="400">Un filtro o la paginación no tiene un valor válido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
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
    /// <summary>Consulta el pedido, sus productos y su total. Los costos requieren permiso adicional.</summary>
    /// <param name="id">Identificador del pedido.</param>
    /// <response code="403">Falta permiso o el pedido pertenece a otro comprador.</response>
    /// <response code="404">El pedido no existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
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
    /// <summary>Confirma un pedido a partir de una cotización aceptada y vigente.</summary>
    /// <param name="p">Cotización, fecha comprometida y condición de pago.</param>
    /// <response code="201">Devuelve el identificador del pedido creado.</response>
    /// <response code="400">Datos inválidos, fecha o detalles incompletos.</response>
    /// <response code="404">La cotización no existe.</response>
    /// <response code="409">La cotización, el cliente o los productos no cumplen las condiciones de confirmación.</response>
    [HttpPost,
            Authorize(Policy = "pedidos.administrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirmar(PedidoPeticion p) => StatusCode(201,
            new
            {
                Id = await servicio.ConfirmarAsync(p)
            });
    /// <summary>Cancela un pedido confirmado y registra el motivo.</summary>
    /// <param name="id">Identificador del pedido.</param>
    /// <param name="p">Motivo de la cancelación.</param>
    /// <response code="400">Falta el motivo o no es válido.</response>
    /// <response code="404">El pedido no existe.</response>
    /// <response code="409">El estado, el despacho o una seña pendiente de acuerdo impiden cancelar el pedido.</response>
    [HttpPost("{id:guid}/cancelar"),
            Authorize(Policy = "pedidos.administrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(Guid id,
            MotivoPeticion p)
    {
        await servicio.Cancelar(id, p.Motivo);
        return NoContent();
    }
}
