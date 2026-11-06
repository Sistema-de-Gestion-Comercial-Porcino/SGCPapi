using SGCP.Api.Comun;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Patrones.Comercial;

public class PedidoBuilder
{
    private readonly Pedido pedido = new();
    private bool construido;
    public PedidoBuilder ParaCliente(Cliente c) { pedido.ClienteId = c.Id; return this; }
    public PedidoBuilder DesdeCotizacion(Cotizacion c) { pedido.CotizacionId = c.Id; return this; }
    public PedidoBuilder ConCodigo(string codigo) { pedido.Codigo = codigo; return this; }
    public PedidoBuilder ConFechaComprometida(DateTime fecha) { pedido.FechaComprometida = fecha; return this; }
    public PedidoBuilder ConCondicionPago(FormaPago pago) { pedido.CondicionPago = pago; return this; }
    public PedidoBuilder AgregarDetalle(Guid productoId,
            decimal cantidad,
            string unidad,
            decimal precioAcordado,
            decimal costoReferenciaAcordado)
    {
        if (construido || cantidad <= 0 || precioAcordado <= 0 || costoReferenciaAcordado <= 0 || string.IsNullOrWhiteSpace(unidad))
            throw new ReglaException("DETALLE_INVALIDO", "El detalle requiere cantidad, unidad, precio y costo válidos.", 400);
        pedido.Detalles.Add(new DetallePedido
        {
            ProductoId = productoId,
            CantidadSolicitada = cantidad,
            CantidadConfirmada = cantidad,
            Unidad = unidad,
            PrecioAcordado = precioAcordado,
            CostoReferenciaAcordado = costoReferenciaAcordado
        });
        return this;
    }
    public Pedido Construir()
    {
        if (construido
            || pedido.Detalles.Count == 0
            || pedido.ClienteId == Guid.Empty
            || pedido.CotizacionId == Guid.Empty
            || string.IsNullOrWhiteSpace(pedido.Codigo)
            || pedido.FechaComprometida <= DateTime.UtcNow)
            throw new ReglaException("PEDIDO_INCOMPLETO", "Revisa el cliente, cotización, fecha y detalles.", 400);
        construido = true;
        pedido.Fecha = DateTime.UtcNow;
        pedido.EstadoComercial = EstadoComercial.CONFIRMADO;
        pedido.EstadoPreparacion = EstadoPreparacion.PENDIENTE;
        pedido.CostoEstimadoConfirmado = Consultas.Dinero(pedido.Detalles.Sum(d => d.CantidadConfirmada * d.CostoReferenciaAcordado));
        pedido.MargenEstimadoConfirmado = Consultas.Dinero(pedido.Detalles.Sum(d => d.CantidadConfirmada * (d.PrecioAcordado - d.CostoReferenciaAcordado)));
        return pedido;
    }
}
