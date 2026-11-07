using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class OrdenProduccion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public TipoOrdenProduccion Tipo
    {
        get; set;
    }
    public EstadoOrdenProduccion Estado
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
    public DateTime FechaCreacion
    {
        get; set;
    }
    public DateTime InicioPrevisto
    {
        get; set;
    }
    public DateTime FinPrevisto
    {
        get; set;
    }
    public DateTime? InicioReal
    {
        get; set;
    }
    public DateTime? FinReal
    {
        get; set;
    }
    public string Observacion { get; set; } = string.Empty;
}
