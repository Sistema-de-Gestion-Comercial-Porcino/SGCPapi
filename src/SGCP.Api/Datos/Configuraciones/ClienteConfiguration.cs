using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Clientes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Property(x => x.Tipo).HasMaxLength(60);
        b.Property(x => x.Telefono).HasMaxLength(30);
        b.Property(x => x.Direccion).HasMaxLength(250);
        b.Property(x => x.IdentificacionFiscal).HasMaxLength(40);
        b.Property(x => x.Observaciones).HasMaxLength(500);
        b.Property(x => x.CondicionHabitual).HasConversion<string>().HasMaxLength(20);
        b.Property<string>("IdentificacionFiscalUnica")
            .HasMaxLength(40)
            .HasComputedColumnSql("CASE WHEN `Activo` = 1 AND `IdentificacionFiscal` <> '' THEN `IdentificacionFiscal` ELSE NULL END", stored: true);
        b.HasIndex("IdentificacionFiscalUnica").IsUnique();
    }
}
