using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> b)
    {
        b.ToTable("Pedidos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.EstadoComercial).HasConversion<string>().HasMaxLength(20).IsConcurrencyToken();
        b.Property(x => x.EstadoPreparacion).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CondicionPago).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CostoEstimadoConfirmado).HasPrecision(14, 2);
        b.Property(x => x.MargenEstimadoConfirmado).HasPrecision(14, 2);
        b.Property(x => x.MotivoCancelacion).HasMaxLength(500);
        b.HasOne(x => x.Cliente).WithMany(x => x.Pedidos).HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cotizacion).WithOne(x => x.Pedido).HasForeignKey<Pedido>(x => x.CotizacionId).OnDelete(DeleteBehavior.Restrict);
    }
}
