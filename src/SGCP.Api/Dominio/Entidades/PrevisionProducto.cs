using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class PrevisionProducto
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
    public decimal CantidadEstimada
    {
        get; set;
    }
    public string Unidad { get; set; } = string.Empty;
}
