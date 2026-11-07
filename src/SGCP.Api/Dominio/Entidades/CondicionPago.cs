using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class CondicionPago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId
    {
        get; set;
    }
    public FormaPago Modalidad
    {
        get; set;
    }
    public decimal ImporteAutorizado
    {
        get; set;
    }
    public DateTime FechaAcuerdo
    {
        get; set;
    }
    public DateTime? Vencimiento
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public bool Activa
    {
        get; set;
    }
}
