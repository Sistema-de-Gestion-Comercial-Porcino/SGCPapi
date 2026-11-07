using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Pesaje
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
    public Guid ResponsableId
    {
        get; set;
    }
    public decimal PesoBruto
    {
        get; set;
    }
    public decimal Tara
    {
        get; set;
    }
    public decimal Neto
    {
        get; set;
    }
    public string Unidad { get; set; } = string.Empty;
    public DateTime Fecha
    {
        get; set;
    }
    public string Observacion { get; set; } = string.Empty;
}
