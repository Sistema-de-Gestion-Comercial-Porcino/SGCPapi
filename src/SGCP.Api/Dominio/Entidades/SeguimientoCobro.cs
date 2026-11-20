using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class SeguimientoCobro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CondicionPagoId
    {
        get; set;
    }
    public DateTime? FechaGestion
    {
        get; set;
    }
    public string Resultado { get; set; } = string.Empty;
    public DateTime ProximaFecha
    {
        get; set;
    }
    public EstadoSeguimiento Estado
    {
        get; set;
    }
}
