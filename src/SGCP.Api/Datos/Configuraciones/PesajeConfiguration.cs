using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class PesajeConfiguration : IEntityTypeConfiguration<Pesaje>
{
    public void Configure(EntityTypeBuilder<Pesaje> b)
    {
        b.ToTable("Pesajes");
        b.HasKey(x => x.Id);
        b.Property(x => x.PesoBruto).HasPrecision(12, 3);
        b.Property(x => x.Tara).HasPrecision(12, 3);
        b.Property(x => x.Neto).HasPrecision(12, 3);
        b.Property(x => x.Unidad).HasMaxLength(20);
        b.Property(x => x.Observacion).HasMaxLength(1000);
        b.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.LoteId).IsUnique();
    }
}
