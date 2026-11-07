using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class MovimientoStockConfiguration : IEntityTypeConfiguration<MovimientoStock>
{
    public void Configure(EntityTypeBuilder<MovimientoStock> b)
    {
        b.ToTable("MovimientosStock");
        b.HasKey(x => x.Id);
        b.Property(x => x.Tipo).HasMaxLength(40);
        b.Property(x => x.Cantidad).HasPrecision(12, 3);
        b.Property(x => x.Motivo).HasMaxLength(1000);
        b.HasOne<Existencia>().WithMany().HasForeignKey(x => x.ExistenciaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.Tipo, x.ReferenciaId }).IsUnique();
    }
}
