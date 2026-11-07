using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class PrevisionProductoConfiguration : IEntityTypeConfiguration<PrevisionProducto>
{
    public void Configure(EntityTypeBuilder<PrevisionProducto> b)
    {
        b.ToTable("PrevisionesProducto");
        b.HasKey(x => x.Id);
        b.Property(x => x.CantidadEstimada).HasPrecision(12, 3);
        b.Property(x => x.Unidad).HasMaxLength(20);
        b.HasOne<OrdenProduccion>().WithMany().HasForeignKey(x => x.OrdenProduccionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.OrdenProduccionId, x.ProductoId }).IsUnique();
    }
}
