using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class EvidenciaRecepcionConfiguration : IEntityTypeConfiguration<EvidenciaRecepcion>
{
    public void Configure(EntityTypeBuilder<EvidenciaRecepcion> b)
    {
        b.ToTable("EvidenciasRecepcion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Tipo).HasMaxLength(40);
        b.Property(x => x.Sha256).HasMaxLength(64);
        b.HasOne<Recepcion>().WithMany().HasForeignKey(x => x.RecepcionId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Contenido).HasColumnType("mediumblob");
    }
}
