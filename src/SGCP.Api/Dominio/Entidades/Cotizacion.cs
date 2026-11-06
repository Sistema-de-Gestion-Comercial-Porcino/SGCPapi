using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class Cotizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public DateTime Fecha { get; set; }
    public DateTime VigenteHasta { get; set; }
    public CanalSolicitud Canal { get; set; }
    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.BORRADOR;
    public string MotivoRespuesta { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid? CotizacionOrigenId { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Cotizacion? CotizacionOrigen { get; set; }
    public Pedido? Pedido { get; set; }
    public ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
}
