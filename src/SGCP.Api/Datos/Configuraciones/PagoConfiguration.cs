using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> b)
    {
        b.ToTable("Pagos");
        b.HasKey(x => x.Id);
        b.Property(x => x.ReferenciaUnica).HasMaxLength(80);
        b.Property(x => x.Importe).HasPrecision(12, 2);
        b.Property(x => x.Medio).HasMaxLength(80);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.MotivoAnulacion).HasMaxLength(1000);
        b.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ReferenciaUnica).IsUnique();
        b.HasIndex(x => new { x.ClienteId, x.Fecha });
    }
}
