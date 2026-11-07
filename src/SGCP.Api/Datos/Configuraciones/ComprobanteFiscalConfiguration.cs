using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGCP.Api.Dominio.Entidades;

namespace SGCP.Api.Datos.Configuraciones;

public class ComprobanteFiscalConfiguration : IEntityTypeConfiguration<ComprobanteFiscal>
{
    public void Configure(EntityTypeBuilder<ComprobanteFiscal> b)
    {
        b.ToTable("ComprobantesFiscales");
        b.HasKey(x => x.Id);
        b.Property(x => x.ReferenciaUnica).HasMaxLength(80);
        b.Property(x => x.Estado).HasMaxLength(40);
        b.Property(x => x.NumeroFiscal).HasMaxLength(80);
        b.Property(x => x.RespuestaProveedor).HasMaxLength(1000);
        b.HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PedidoId).IsUnique();
        b.HasIndex(x => x.ReferenciaUnica).IsUnique();
    }
}
