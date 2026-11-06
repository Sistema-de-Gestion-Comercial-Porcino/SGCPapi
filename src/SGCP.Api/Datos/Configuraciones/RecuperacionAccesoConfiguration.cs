using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class RecuperacionAccesoConfiguration : IEntityTypeConfiguration<RecuperacionAcceso>
{
    public void Configure(EntityTypeBuilder<RecuperacionAcceso> b)
    {
        b.ToTable("RecuperacionesAcceso");
        b.HasKey(x => x.Id);
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasOne(x => x.Usuario).WithMany(x => x.Recuperaciones).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.UtilizadoEn).IsConcurrencyToken();
    }
}
