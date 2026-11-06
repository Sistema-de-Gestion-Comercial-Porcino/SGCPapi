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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SgcpContext).Assembly);
    }
}
