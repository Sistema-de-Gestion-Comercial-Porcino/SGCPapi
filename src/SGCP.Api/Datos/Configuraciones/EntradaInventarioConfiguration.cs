using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class EntradaInventarioConfiguration : IEntityTypeConfiguration<EntradaInventario>
{
    public void Configure(EntityTypeBuilder<EntradaInventario> b)
    {
        b.ToTable("EntradasInventario");
        b.HasKey(x => x.Id);
        b.Property(x => x.Cantidad).HasPrecision(12, 3);
        b.Property(x => x.MotivoAnulacion).HasMaxLength(1000);
        b.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Pesaje>().WithMany().HasForeignKey(x => x.PesajeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Existencia>().WithMany().HasForeignKey(x => x.ExistenciaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.AnuladaPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.LoteId, x.ProductoId }).IsUnique();
    }
}
