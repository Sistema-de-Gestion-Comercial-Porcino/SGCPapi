using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> b)
    {
        b.ToTable("Sesiones");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.UsuarioId, x.Revocada });
        b.HasOne(x => x.Usuario).WithMany(x => x.Sesiones).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
