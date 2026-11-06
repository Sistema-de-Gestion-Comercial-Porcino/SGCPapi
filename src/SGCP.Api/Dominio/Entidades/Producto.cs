using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Presentacion { get; set; } = string.Empty;
    public string UnidadVenta { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
    public Guid ClasificacionProductoId { get; set; }

    public ClasificacionProducto ClasificacionProducto { get; set; } = null!;
    public ICollection<PrecioProducto> Precios { get; set; } = new List<PrecioProducto>();
}
