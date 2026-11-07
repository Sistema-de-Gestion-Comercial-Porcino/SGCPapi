using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Incidencia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? LoteId
    {
        get; set;
    }
    public Guid? AnimalId
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public string Motivo { get; set; } = string.Empty;
    public DateTime Fecha
    {
        get; set;
    }
    public bool Resuelta
    {
        get; set;
    }
    public string Resolucion { get; set; } = string.Empty;
    public DateTime? ResueltaEn
    {
        get; set;
    }
}
