using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class ControlFrio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? EntregaId
    {
        get; set;
    }
    public Guid? LoteId
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public string Unidad { get; set; } = "°C";
    public decimal Temperatura
    {
        get; set;
    }
    public string Ubicacion { get; set; } = string.Empty;
    public EvaluacionFrio Evaluacion
    {
        get; set;
    }
    public string Observacion { get; set; } = string.Empty;
    public DateTime Fecha
    {
        get; set;
    }
    /// <summary>Desde cuándo la cámara o el lote estuvo fuera de temperatura, según la investigación del responsable.
    /// Solo los consumos y movimientos posteriores a esta hora quedan comprometidos.</summary>
    public DateTime? InicioFalla
    {
        get; set;
    }
}
