using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class SenaConfiguration : IEntityTypeConfiguration<Sena>
{
    public void Configure(EntityTypeBuilder<Sena> b)
    {
        b.ToTable("Senas");
        b.HasKey(x => x.Id);
        b.Property(x => x.AcuerdoResolucion).HasMaxLength(1000);
        b.HasOne<Pago>().WithMany().HasForeignKey(x => x.PagoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PagoId).IsUnique();
    }
}
