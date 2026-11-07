using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Existencia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoteId
    {
        get; set;
    }
    public Guid ProductoId
    {
        get; set;
    }
    public string Ubicacion { get; set; } = string.Empty;
    public decimal CantidadFisica
    {
        get; set;
    }
}
