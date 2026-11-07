using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> b)
    {
        b.ToTable("Cargos");
        b.HasKey(x => x.Id);
        b.Property(x => x.AcuerdoServicio).HasMaxLength(300);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Concepto).HasMaxLength(300);
        b.Property(x => x.Importe).HasPrecision(12, 2);
        b.Property(x => x.MotivoAnulacion).HasMaxLength(1000);
        b.HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DetallePedido>().WithMany().HasForeignKey(x => x.DetallePedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.DetallePedidoId).IsUnique();
    }
}
