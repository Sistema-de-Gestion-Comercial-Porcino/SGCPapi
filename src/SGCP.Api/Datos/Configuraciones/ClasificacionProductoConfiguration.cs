using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class ClasificacionProductoConfiguration : IEntityTypeConfiguration<ClasificacionProducto>
{
    public void Configure(EntityTypeBuilder<ClasificacionProducto> b)
    {
        b.ToTable("ClasificacionesProducto");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(400);
        b.Property(x => x.Familia).HasConversion<string>().HasMaxLength(20);
    }
}
