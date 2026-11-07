using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> b)
    {
        b.ToTable("Animales");
        b.HasKey(x => x.Id);
        b.Property(x => x.Identificacion).HasMaxLength(80);
        b.Property(x => x.Procedencia).HasMaxLength(300);
        b.Property(x => x.Proveedor).HasMaxLength(180);
        b.Property(x => x.PesoIngreso).HasPrecision(12, 3);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.HasIndex(x => x.Identificacion).IsUnique();
    }
}
