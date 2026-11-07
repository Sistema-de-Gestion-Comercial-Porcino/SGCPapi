using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class ControlFrioConfiguration : IEntityTypeConfiguration<ControlFrio>
{
    public void Configure(EntityTypeBuilder<ControlFrio> b)
    {
        b.ToTable("ControlesFrio");
        b.HasOne<Entrega>().WithMany().HasForeignKey(x => x.EntregaId).OnDelete(DeleteBehavior.Restrict);
        b.HasKey(x => x.Id);
        b.Property(x => x.Unidad).HasMaxLength(8);
        b.Property(x => x.Temperatura).HasPrecision(12, 2);
        b.Property(x => x.Ubicacion).HasMaxLength(180);
        b.Property(x => x.Evaluacion).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Observacion).HasMaxLength(1000);
        b.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
    }
}
