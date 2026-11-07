using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class ResultadoProduccion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdenProduccionId
    {
        get; set;
    }
    public Guid ProductoId
    {
        get; set;
    }
    public Guid LoteId
    {
        get; set;
    }
    public decimal? CantidadReal
    {
        get; set;
    }
    public EstadoResultado Estado
    {
        get; set;
    }
    public DateTime FechaRegistro
    {
        get; set;
    }
}
