using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class OrdenProduccionConfiguration : IEntityTypeConfiguration<OrdenProduccion>
{
    public void Configure(EntityTypeBuilder<OrdenProduccion> b)
    {
        b.ToTable("OrdenesProduccion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(80);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Observacion).HasMaxLength(1000);
        b.HasOne<Animal>().WithMany().HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property<Guid?>("AnimalEnOrden")
            .HasComputedColumnSql("CASE WHEN `Estado` <> 'ANULADA' THEN `AnimalId` ELSE NULL END", stored: true);
        b.HasIndex("AnimalEnOrden").IsUnique();
    }
}
