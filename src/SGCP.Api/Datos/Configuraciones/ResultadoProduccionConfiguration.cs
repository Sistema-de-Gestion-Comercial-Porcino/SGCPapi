using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class ResultadoProduccionConfiguration : IEntityTypeConfiguration<ResultadoProduccion>
{
    public void Configure(EntityTypeBuilder<ResultadoProduccion> b)
    {
        b.ToTable("ResultadosProduccion");
        b.HasKey(x => x.Id);
        b.Property(x => x.CantidadReal).HasPrecision(12, 3);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.HasOne<OrdenProduccion>().WithMany().HasForeignKey(x => x.OrdenProduccionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.LoteId).IsUnique();
    }
}
