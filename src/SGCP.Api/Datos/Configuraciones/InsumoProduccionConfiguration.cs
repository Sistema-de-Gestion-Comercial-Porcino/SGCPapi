using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class InsumoProduccionConfiguration : IEntityTypeConfiguration<InsumoProduccion>
{
    public void Configure(EntityTypeBuilder<InsumoProduccion> b)
    {
        b.ToTable("InsumosProduccion");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.ReferenciaUnica).IsUnique();
        b.Property(x => x.Cantidad).HasPrecision(12, 3);
        b.Property(x => x.Unidad).HasMaxLength(20);
        b.HasOne<OrdenProduccion>().WithMany().HasForeignKey(x => x.OrdenProduccionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Existencia>().WithMany().HasForeignKey(x => x.ExistenciaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
    }
}
