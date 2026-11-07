using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class ControlSanitarioConfiguration : IEntityTypeConfiguration<ControlSanitario>
{
    public void Configure(EntityTypeBuilder<ControlSanitario> b)
    {
        b.ToTable("ControlesSanitarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Resultado).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Observacion).HasMaxLength(1000);
        b.HasOne<Animal>().WithMany().HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.AnimalId, x.Fecha });
    }
}
