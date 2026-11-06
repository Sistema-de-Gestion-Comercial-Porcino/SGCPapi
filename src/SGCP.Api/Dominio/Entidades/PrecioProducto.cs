using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class PrecioProducto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal CostoReferencia { get; set; }
    public decimal PrecioVenta { get; set; }
    public DateTime VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
    public Guid ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;
}
