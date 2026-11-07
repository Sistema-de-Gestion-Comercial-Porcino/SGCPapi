using Microsoft.EntityFrameworkCore;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Datos;

public class SgcpContext(DbContextOptions<SgcpContext> opciones) : DbContext(opciones)
{
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<RecuperacionAcceso> RecuperacionesAcceso => Set<RecuperacionAcceso>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ClasificacionProducto> ClasificacionesProducto => Set<ClasificacionProducto>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<PrecioProducto> PreciosProducto => Set<PrecioProducto>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<DetalleCotizacion> DetallesCotizacion => Set<DetalleCotizacion>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
    public DbSet<Animal> Animales => Set<Animal>();
    public DbSet<ControlSanitario> ControlesSanitarios => Set<ControlSanitario>();
    public DbSet<OrdenProduccion> OrdenesProduccion => Set<OrdenProduccion>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<ResultadoProduccion> ResultadosProduccion => Set<ResultadoProduccion>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<InsumoProduccion> InsumosProduccion => Set<InsumoProduccion>();
    public DbSet<Pesaje> Pesajes => Set<Pesaje>();
    public DbSet<EntradaInventario> EntradasInventario => Set<EntradaInventario>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ControlFrio> ControlesFrio => Set<ControlFrio>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<Incidencia> Incidencias => Set<Incidencia>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<CondicionPago> CondicionesPago => Set<CondicionPago>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Sena> Senas => Set<Sena>();
    public DbSet<AplicacionPago> AplicacionesPago => Set<AplicacionPago>();
    public DbSet<Transporte> Transportes => Set<Transporte>();
    public DbSet<Ruta> Rutas => Set<Ruta>();
    public DbSet<Entrega> Entregas => Set<Entrega>();
    public DbSet<Recepcion> Recepciones => Set<Recepcion>();
    public DbSet<EvidenciaRecepcion> EvidenciasRecepcion => Set<EvidenciaRecepcion>();
    public DbSet<ComprobanteFiscal> ComprobantesFiscales => Set<ComprobanteFiscal>();
    public DbSet<PrevisionProducto> PrevisionesProducto => Set<PrevisionProducto>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SgcpContext).Assembly);
        var utc = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
            foreach (var propiedad in entidad.GetProperties())
                if (propiedad.ClrType == typeof(DateTime) || propiedad.ClrType == typeof(DateTime?))
                    propiedad.SetValueConverter(utc);
    }
}
