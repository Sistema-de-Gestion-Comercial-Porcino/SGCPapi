using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class DetalleCotizacionConfiguration : IEntityTypeConfiguration<DetalleCotizacion>
{
    public void Configure(EntityTypeBuilder<DetalleCotizacion> b)
    {
        b.ToTable("DetallesCotizacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Cantidad).HasPrecision(12, 3);
        b.Property(x => x.Unidad).HasMaxLength(10).IsRequired();
        b.Property(x => x.PrecioUnitario).HasPrecision(12, 2);
        b.Property(x => x.Subtotal).HasPrecision(14, 2);
        b.HasOne(x => x.Cotizacion).WithMany(x => x.Detalles).HasForeignKey(x => x.CotizacionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
    }
}
