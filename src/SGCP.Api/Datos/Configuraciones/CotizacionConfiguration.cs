using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class CotizacionConfiguration : IEntityTypeConfiguration<Cotizacion>
{
    public void Configure(EntityTypeBuilder<Cotizacion> b)
    {
        b.ToTable("Cotizaciones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20).IsConcurrencyToken();
        b.Property(x => x.Canal).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.MotivoRespuesta).HasMaxLength(500);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CotizacionOrigen).WithMany().HasForeignKey(x => x.CotizacionOrigenId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.CotizacionOrigenId).IsUnique();
    }
}
