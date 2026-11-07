using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class EntradaInventario
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
    public Guid PesajeId
    {
        get; set;
    }
    public Guid ExistenciaId
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
    public decimal Cantidad
    {
        get; set;
    }
    public bool Anulada
    {
        get; set;
    }
    public Guid? AnuladaPorId
    {
        get; set;
    }
    public DateTime? AnuladaEn
    {
        get; set;
    }
    public string MotivoAnulacion { get; set; } = string.Empty;
}
