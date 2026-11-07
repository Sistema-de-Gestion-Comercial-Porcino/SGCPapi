using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class ExistenciaConfiguration : IEntityTypeConfiguration<Existencia>
{
    public void Configure(EntityTypeBuilder<Existencia> b)
    {
        b.ToTable("Existencias");
        b.HasKey(x => x.Id);
        b.Property(x => x.Ubicacion).HasMaxLength(180);
        b.Property(x => x.CantidadFisica).HasPrecision(12, 3);
        b.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.LoteId, x.ProductoId }).IsUnique();
    }
}
