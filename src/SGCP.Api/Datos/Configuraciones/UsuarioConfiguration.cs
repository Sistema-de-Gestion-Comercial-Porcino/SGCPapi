using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Property(x => x.Login).HasMaxLength(80).IsRequired();
        b.HasIndex(x => x.Login).IsUnique();
        b.Property(x => x.Correo).HasMaxLength(180).IsRequired();
        b.HasIndex(x => x.Correo).IsUnique();
        b.Property(x => x.HashClave).HasMaxLength(500).IsRequired();
        b.Property(x => x.Telefono).HasMaxLength(30);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.HasOne(x => x.Cliente).WithMany(x => x.CuentasPortal).HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Roles)
            .WithMany(x => x.Usuarios)
            .UsingEntity<Dictionary<string, object>>("UsuariosRoles",
                    r => r.HasOne<Rol>().WithMany().HasForeignKey("RolId").OnDelete(DeleteBehavior.Restrict),
                    l => l.HasOne<Usuario>().WithMany().HasForeignKey("UsuarioId").OnDelete(DeleteBehavior.Cascade));
    }
}
