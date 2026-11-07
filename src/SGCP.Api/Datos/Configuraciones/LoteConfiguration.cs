using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> b)
    {
        b.ToTable("Lotes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(80);
        b.Property(x => x.Unidad).HasMaxLength(20);
        b.Property(x => x.Procedencia).HasMaxLength(500);
        b.Property(x => x.ReferenciaExterna).HasMaxLength(180);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<OrdenProduccion>().WithMany().HasForeignKey(x => x.OrdenProduccionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
