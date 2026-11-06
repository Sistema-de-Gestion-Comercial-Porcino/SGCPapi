using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class PrecioProductoConfiguration : IEntityTypeConfiguration<PrecioProducto>
{
    public void Configure(EntityTypeBuilder<PrecioProducto> b)
    {
        b.ToTable("PreciosProducto");
        b.HasKey(x => x.Id);
        b.Property(x => x.CostoReferencia).HasPrecision(12, 2);
        b.Property(x => x.PrecioVenta).HasPrecision(12, 2);
        b.HasIndex(x => new { x.ProductoId, x.VigenteDesde }).IsUnique();
        b.Property<Guid?>("ProductoConVigenciaAbierta")
            .HasComputedColumnSql("CASE WHEN `VigenteHasta` IS NULL THEN `ProductoId` ELSE NULL END", stored: true);
        b.HasIndex("ProductoConVigenciaAbierta").IsUnique();
        b.HasOne(x => x.Producto).WithMany(x => x.Precios).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
    }
}
