using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos.Configuraciones;

public class DetallePedidoConfiguration : IEntityTypeConfiguration<DetallePedido>
{
    public void Configure(EntityTypeBuilder<DetallePedido> b)
    {
        b.ToTable("DetallesPedido");
        b.HasKey(x => x.Id);
        b.Property(x => x.CantidadSolicitada).HasPrecision(12, 3);
        b.Property(x => x.CantidadConfirmada).HasPrecision(12, 3);
        b.Property(x => x.Unidad).HasMaxLength(10).IsRequired();
        b.Property(x => x.PrecioAcordado).HasPrecision(12, 2);
        b.Property(x => x.CostoReferenciaAcordado).HasPrecision(12, 2);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.HasOne(x => x.Pedido).WithMany(x => x.Detalles).HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
    }
}
