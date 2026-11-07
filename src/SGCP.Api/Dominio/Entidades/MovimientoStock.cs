using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class MovimientoStock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExistenciaId
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public Guid ReferenciaId
    {
        get; set;
    }
    public string Tipo { get; set; } = string.Empty;
    public decimal Cantidad
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
    public string Motivo { get; set; } = string.Empty;
}
