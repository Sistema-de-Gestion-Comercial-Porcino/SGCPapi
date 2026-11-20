using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class SeguimientoCobroConfiguration : IEntityTypeConfiguration<SeguimientoCobro>
{
    public void Configure(EntityTypeBuilder<SeguimientoCobro> b)
    {
        b.ToTable("SeguimientosCobro");
        b.HasKey(x => x.Id);
        b.Property(x => x.Resultado).HasMaxLength(500);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.HasOne<CondicionPago>().WithMany().HasForeignKey(x => x.CondicionPagoId).OnDelete(DeleteBehavior.Restrict);
    }
}
