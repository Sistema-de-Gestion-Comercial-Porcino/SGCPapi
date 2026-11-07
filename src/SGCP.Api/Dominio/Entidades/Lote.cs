using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Lote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public Guid ProductoId
    {
        get; set;
    }
    public Guid? OrdenProduccionId
    {
        get; set;
    }
    public DateTime FechaProduccion
    {
        get; set;
    }
    public DateTime Vencimiento
    {
        get; set;
    }
    public string Unidad { get; set; } = string.Empty;
    public string Procedencia { get; set; } = string.Empty;
    public string ReferenciaExterna { get; set; } = string.Empty;
    public EstadoLote Estado
    {
        get; set;
    }
}
