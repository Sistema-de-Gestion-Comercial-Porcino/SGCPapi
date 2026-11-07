using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> b)
    {
        b.ToTable("Reservas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Cantidad).HasPrecision(12, 3);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.MotivoLiberacion).HasMaxLength(1000);
        b.HasOne<DetallePedido>().WithMany().HasForeignKey(x => x.DetallePedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Existencia>().WithMany().HasForeignKey(x => x.ExistenciaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.Property<Guid?>("DetalleActivo")
            .HasComputedColumnSql("CASE WHEN `Estado` = 'ACTIVA' THEN `DetallePedidoId` ELSE NULL END", stored: true);
        b.HasIndex("DetalleActivo", nameof(Reserva.ExistenciaId)).IsUnique();
    }
}
