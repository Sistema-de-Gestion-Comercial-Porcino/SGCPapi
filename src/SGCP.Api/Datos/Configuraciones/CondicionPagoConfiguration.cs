using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class CondicionPagoConfiguration : IEntityTypeConfiguration<CondicionPago>
{
    public void Configure(EntityTypeBuilder<CondicionPago> b)
    {
        b.ToTable("CondicionesPago");
        b.HasKey(x => x.Id);
        b.Property(x => x.Modalidad).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.ImporteAutorizado).HasPrecision(12, 2);
        b.HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.Property<Guid?>("PedidoActivo").HasComputedColumnSql("CASE WHEN `Activa` = 1 THEN `PedidoId` ELSE NULL END", stored: true);
        b.HasIndex("PedidoActivo").IsUnique();
    }
}
