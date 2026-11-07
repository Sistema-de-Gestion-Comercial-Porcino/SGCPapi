using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Recepcion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EntregaId
    {
        get; set;
    }
    public Guid ClaveUnica
    {
        get; set;
    }
    public int VersionBase
    {
        get; set;
    }
    public Guid AutorId
    {
        get; set;
    }
    public DateTime FechaReal
    {
        get; set;
    }
    public DateTime FechaSincronizacion
    {
        get; set;
    }
    public decimal PesoRecibido
    {
        get; set;
    }
    public string Unidad { get; set; } = string.Empty;
    public decimal? SaldoPendienteAlConfirmar
    {
        get; set;
    }
    public bool Conforme
    {
        get; set;
    }
    public string Receptor { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
    public EstadoRecepcion Estado
    {
        get; set;
    }
    public string HuellaContenido { get; set; } = string.Empty;
    public string MotivoConflicto { get; set; } = string.Empty;
    public Guid? RevisadaPorId
    {
        get; set;
    }
    public DateTime? RevisadaEn
    {
        get; set;
    }
    public string Resolucion { get; set; } = string.Empty;
}
