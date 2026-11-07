using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class RutaConfiguration : IEntityTypeConfiguration<Ruta>
{
    public void Configure(EntityTypeBuilder<Ruta> b)
    {
        b.ToTable("Rutas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(80);
        b.Property(x => x.Zona).HasMaxLength(180);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.HasOne<Transporte>().WithMany().HasForeignKey(x => x.TransporteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
