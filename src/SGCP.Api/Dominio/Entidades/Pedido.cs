using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class Pedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public DateTime FechaComprometida { get; set; }
    public EstadoComercial EstadoComercial { get; set; } = EstadoComercial.CONFIRMADO;
    public EstadoPreparacion EstadoPreparacion { get; set; } = EstadoPreparacion.PENDIENTE;
    public FormaPago CondicionPago { get; set; }
    public decimal CostoEstimadoConfirmado { get; set; }
    public decimal MargenEstimadoConfirmado { get; set; }
    public string MotivoCancelacion { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid CotizacionId { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Cotizacion Cotizacion { get; set; } = null!;
    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}
