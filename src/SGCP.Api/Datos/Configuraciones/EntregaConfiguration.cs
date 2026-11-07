using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class EntregaConfiguration : IEntityTypeConfiguration<Entrega>
{
    public void Configure(EntityTypeBuilder<Entrega> b)
    {
        b.ToTable("Entregas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Modalidad).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Receptor).HasMaxLength(180);
        b.Property(x => x.ObservacionRecepcion).HasMaxLength(1000);
        b.HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Ruta>().WithMany().HasForeignKey(x => x.RutaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PedidoId).IsUnique();
        b.HasIndex(x => new { x.ResponsableId, x.Estado });
        b.Property(x => x.Version).IsConcurrencyToken();
    }
}
