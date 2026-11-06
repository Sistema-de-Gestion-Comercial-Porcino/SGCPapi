using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> b)
    {
        b.ToTable("Permisos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(80).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Descripcion).HasMaxLength(200);
        b.Property(x => x.Modulo).HasMaxLength(40);
        b.Property(x => x.Accion).HasMaxLength(40);
    }
}
