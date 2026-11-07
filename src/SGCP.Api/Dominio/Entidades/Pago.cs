using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Pago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClienteId
    {
        get; set;
    }
    public string ReferenciaUnica { get; set; } = string.Empty;
    public decimal Importe
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
    public string Medio { get; set; } = string.Empty;
    public TipoPago Tipo
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public bool Anulado
    {
        get; set;
    }
    public string MotivoAnulacion { get; set; } = string.Empty;
}
