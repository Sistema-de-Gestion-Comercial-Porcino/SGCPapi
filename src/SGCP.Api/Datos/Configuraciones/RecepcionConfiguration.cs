using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class RecepcionConfiguration : IEntityTypeConfiguration<Recepcion>
{
    public void Configure(EntityTypeBuilder<Recepcion> b)
    {
        b.ToTable("Recepciones");
        b.HasKey(x => x.Id);
        b.Property(x => x.PesoRecibido).HasPrecision(12, 3);
        b.Property(x => x.Unidad).HasMaxLength(20);
        b.Property(x => x.SaldoPendienteAlConfirmar).HasPrecision(12, 2);
        b.Property(x => x.Receptor).HasMaxLength(180);
        b.Property(x => x.Observacion).HasMaxLength(1000);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.HuellaContenido).HasMaxLength(64);
        b.Property(x => x.MotivoConflicto).HasMaxLength(1000);
        b.Property(x => x.Resolucion).HasMaxLength(1000);
        b.HasOne<Entrega>().WithMany().HasForeignKey(x => x.EntregaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.AutorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.RevisadaPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ClaveUnica).IsUnique();
        b.Property<Guid?>("EntregaConfirmada").HasComputedColumnSql("CASE WHEN `Estado` = 'CONFIRMADA' THEN `EntregaId` ELSE NULL END", stored: true);
        b.HasIndex("EntregaConfirmada").IsUnique();
    }
}
