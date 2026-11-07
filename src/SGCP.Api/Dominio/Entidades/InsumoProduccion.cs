using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class InsumoProduccion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReferenciaUnica
    {
        get; set;
    }
    public Guid OrdenProduccionId
    {
        get; set;
    }
    public Guid ExistenciaId
    {
        get; set;
    }
    public decimal Cantidad
    {
        get; set;
    }
    public string Unidad { get; set; } = string.Empty;
    public Guid ResponsableId
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
}
