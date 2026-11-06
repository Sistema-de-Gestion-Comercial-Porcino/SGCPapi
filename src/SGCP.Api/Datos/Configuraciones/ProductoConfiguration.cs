using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> b)
    {
        b.ToTable("Productos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Property(x => x.Presentacion).HasMaxLength(60).IsRequired();
        b.Property(x => x.UnidadVenta).HasMaxLength(10).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(400);
        b.HasOne(x => x.ClasificacionProducto)
            .WithMany(x => x.Productos)
            .HasForeignKey(x => x.ClasificacionProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
