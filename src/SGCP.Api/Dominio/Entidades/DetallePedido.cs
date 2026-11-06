using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class DetallePedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal CantidadSolicitada { get; set; }
    public decimal CantidadConfirmada { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal PrecioAcordado { get; set; }
    public decimal CostoReferenciaAcordado { get; set; }
    public EstadoDetalle Estado { get; set; } = EstadoDetalle.PENDIENTE;
    public Guid PedidoId { get; set; }
    public Guid ProductoId { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
