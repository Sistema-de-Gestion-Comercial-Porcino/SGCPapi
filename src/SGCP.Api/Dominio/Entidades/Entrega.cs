using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Entrega
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId
    {
        get; set;
    }
    public Guid? RutaId
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public ModalidadEntrega Modalidad
    {
        get; set;
    }
    public EstadoEntrega Estado
    {
        get; set;
    }
    public int Version
    {
        get; set;
    }
    public DateTime FechaPrevista
    {
        get; set;
    }
    public DateTime? FechaReal
    {
        get; set;
    }
    public DateTime? FechaConfirmacion
    {
        get; set;
    }
    public string Receptor { get; set; } = string.Empty;
    public string ObservacionRecepcion { get; set; } = string.Empty;
    public int OrdenEnRuta
    {
        get; set;
    }
}
