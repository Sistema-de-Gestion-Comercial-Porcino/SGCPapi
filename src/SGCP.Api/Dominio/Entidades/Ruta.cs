using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Ruta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public Guid TransporteId
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
    public string Zona { get; set; } = string.Empty;
    public EstadoRuta Estado
    {
        get; set;
    }
    public DateTime? SalidaReal
    {
        get; set;
    }
    public DateTime? RetornoReal
    {
        get; set;
    }
}
