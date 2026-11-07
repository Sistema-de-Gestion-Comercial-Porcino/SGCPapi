using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Sena
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PagoId
    {
        get; set;
    }
    public Guid PedidoId
    {
        get; set;
    }
    public string AcuerdoResolucion { get; set; } = string.Empty;
    public DateTime? ResueltaEn
    {
        get; set;
    }
}
