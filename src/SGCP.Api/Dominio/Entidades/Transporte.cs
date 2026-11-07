using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Transporte
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public TipoTransporte Modalidad
    {
        get; set;
    }
    public string Transportista { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string DescripcionVehiculo { get; set; } = string.Empty;
    public string Conservacion { get; set; } = string.Empty;
    public bool Activo
    {
        get; set;
    }
}
