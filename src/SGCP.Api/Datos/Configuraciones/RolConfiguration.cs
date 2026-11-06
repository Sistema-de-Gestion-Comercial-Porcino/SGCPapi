using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> b)
    {
        b.ToTable("Roles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(40).IsRequired();
        b.HasIndex(x => x.Nombre).IsUnique();
        b.Property(x => x.Descripcion).HasMaxLength(200);
        b.HasMany(x => x.Permisos)
            .WithMany(x => x.Roles)
            .UsingEntity<Dictionary<string, object>>("RolesPermisos",
                    r => r.HasOne<Permiso>().WithMany().HasForeignKey("PermisoId").OnDelete(DeleteBehavior.Restrict),
                    l => l.HasOne<Rol>().WithMany().HasForeignKey("RolId").OnDelete(DeleteBehavior.Cascade));
    }
}
