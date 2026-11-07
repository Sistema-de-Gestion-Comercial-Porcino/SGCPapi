using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class TransporteConfiguration : IEntityTypeConfiguration<Transporte>
{
    public void Configure(EntityTypeBuilder<Transporte> b)
    {
        b.ToTable("Transportes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(80);
        b.Property(x => x.Modalidad).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Transportista).HasMaxLength(180);
        b.Property(x => x.Contacto).HasMaxLength(100);
        b.Property(x => x.Placa).HasMaxLength(40);
        b.Property(x => x.DescripcionVehiculo).HasMaxLength(300);
        b.Property(x => x.Conservacion).HasMaxLength(300);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
