using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class AplicacionPagoConfiguration : IEntityTypeConfiguration<AplicacionPago>
{
    public void Configure(EntityTypeBuilder<AplicacionPago> b)
    {
        b.ToTable("AplicacionesPago");
        b.HasKey(x => x.Id);
        b.Property(x => x.Importe).HasPrecision(12, 2);
        b.HasOne<Pago>().WithMany().HasForeignKey(x => x.PagoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cargo>().WithMany().HasForeignKey(x => x.CargoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sena>().WithMany().HasForeignKey(x => x.SenaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.SenaId).IsUnique();
        b.HasIndex(x => new { x.PagoId, x.CargoId }).IsUnique();
    }
}
