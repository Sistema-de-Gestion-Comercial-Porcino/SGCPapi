using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class AplicacionPago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PagoId
    {
        get; set;
    }
    public Guid CargoId
    {
        get; set;
    }
    public Guid? SenaId
    {
        get; set;
    }
    public decimal Importe
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
}
