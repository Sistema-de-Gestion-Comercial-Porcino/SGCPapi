using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class DetalleCotizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public Guid CotizacionId { get; set; }
    public Guid ProductoId { get; set; }

    public Cotizacion Cotizacion { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
