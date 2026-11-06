using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> b)
    {
        b.ToTable("RegistrosAuditoria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Operacion).HasMaxLength(1200).IsRequired();
        b.Property(x => x.Entidad).HasMaxLength(80).IsRequired();
        b.HasOne(x => x.Usuario).WithMany(x => x.Auditorias).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
