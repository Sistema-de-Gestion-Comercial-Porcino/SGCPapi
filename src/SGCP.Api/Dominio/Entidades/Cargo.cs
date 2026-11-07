using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Cargo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId
    {
        get; set;
    }
    public Guid? DetallePedidoId
    {
        get; set;
    }
    public TipoCargo Tipo
    {
        get; set;
    }
    public string Concepto { get; set; } = string.Empty;
    public decimal Importe
    {
        get; set;
    }
    public string AcuerdoServicio { get; set; } = string.Empty;
    public DateTime? PrestadoEn
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
    public bool Anulado
    {
        get; set;
    }
    public string MotivoAnulacion { get; set; } = string.Empty;
}
