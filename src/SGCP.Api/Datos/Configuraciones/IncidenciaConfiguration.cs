using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class IncidenciaConfiguration : IEntityTypeConfiguration<Incidencia>
{
    public void Configure(EntityTypeBuilder<Incidencia> b)
    {
        b.ToTable("Incidencias");
        b.HasKey(x => x.Id);
        b.Property(x => x.Motivo).HasMaxLength(1000);
        b.Property(x => x.Resolucion).HasMaxLength(1000);
        b.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Animal>().WithMany().HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
    }
}
