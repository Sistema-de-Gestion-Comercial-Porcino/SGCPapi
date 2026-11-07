using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Servicios;

public class ServicioCatalogo(SgcpContext db, UnidadTrabajo unidad, ServicioAuditoria auditoria)
{
    public Task<ClasificacionProducto> Clasificacion(Guid? id, ClasificacionPeticion p) => unidad.Ejecutar(async () =>
    {
        var c = id is null ? new ClasificacionProducto() : await db.ClasificacionesProducto.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO",
                "Clasificación no encontrada.",
                404);
        if (id is not null && (c.Familia != p.Familia || c.Nombre != p.Nombre.Trim()) &&
            await db.Lotes.AnyAsync(l => db.Productos.Any(pr => pr.Id == l.ProductoId && pr.ClasificacionProductoId == id)))
            throw new ReglaException("CLASIFICACION_CON_HISTORIAL", "El tipo de carne o derivado ya tiene lotes; crea una clasificación nueva.");
        c.Codigo = p.Codigo.Trim().ToUpperInvariant();
        c.Nombre = p.Nombre.Trim();
        c.Familia = p.Familia;
        c.Descripcion = p.Descripcion ?? "";
        if (await db.ClasificacionesProducto.AnyAsync(x => x.Id != c.Id && x.Codigo == c.Codigo))
            throw new ReglaException("CODIGO_DUPLICADO", "El código de clasificación ya existe.");
        if (id is null)
            db.ClasificacionesProducto.Add(c);
        auditoria.Registrar(id is null ? "CREAR" : "EDITAR", "ClasificacionProducto", c.Id);
        return c;
    });
    public Task<bool> EstadoClasificacion(Guid id, bool activo) => unidad.Ejecutar(async () =>
    {
        var c = await db.ClasificacionesProducto.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO",
                "Clasificación no encontrada.",
                404);
        if (!activo)
        {
            var productos = await db.Productos.Where(x => x.ClasificacionProductoId == id
            && x.Activo).Select(x => x.Nombre).ToArrayAsync();
            if (productos.Length > 0)
                throw new ReglaException("CLASIFICACION_EN_USO",
                "Primero desactiva sus productos.",
                409,
                productos);
        }
        c.Activo = activo;
        auditoria.Registrar(activo ? "ACTIVAR" : "DESACTIVAR", "ClasificacionProducto", id);
        return true;
    });
    public Task<Producto> Producto(Guid? id, ProductoPeticion p) => unidad.Ejecutar(async () =>
    {
        if (!await db.ClasificacionesProducto.AnyAsync(x => x.Id == p.ClasificacionProductoId && x.Activo))
            throw new ReglaException("CLASIFICACION_NO_ACTIVA", "Selecciona una clasificación activa.", 400);
        var c = id is null ? new Producto { FechaAlta = DateTime.UtcNow } : await db.Productos.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO",
                "Producto no encontrado.",
                404);
        if (id is not null
            && c.UnidadVenta != p.UnidadVenta
                                        && (await db.PreciosProducto.AnyAsync(x => x.ProductoId == id) || await db.DetallesCotizacion.AnyAsync(x => x.ProductoId == id) || await db.Lotes.AnyAsync(x => x.ProductoId == id)))
            throw new ReglaException("UNIDAD_INCOMPATIBLE", "La unidad tiene precios u operaciones asociados; crea otra presentación.");
        c.Codigo = p.Codigo.Trim().ToUpperInvariant();
        c.Nombre = p.Nombre.Trim();
        c.Presentacion = p.Presentacion.Trim();
        c.UnidadVenta = p.UnidadVenta;
        c.Descripcion = p.Descripcion ?? "";
        if (id is not null && c.ClasificacionProductoId != p.ClasificacionProductoId && await db.Lotes.AnyAsync(x => x.ProductoId == id))
            throw new ReglaException("PRODUCTO_CON_HISTORIAL", "No se cambia el tipo de carne o derivado de un producto con lotes.");
        c.ClasificacionProductoId = p.ClasificacionProductoId;
        if (await db.Productos.AnyAsync(x => x.Id != c.Id && x.Codigo == c.Codigo))
            throw new ReglaException("CODIGO_DUPLICADO", "El código de producto ya existe.");
        if (id is null)
            db.Productos.Add(c);
        auditoria.Registrar(id is null ? "CREAR" : "EDITAR", "Producto", c.Id);
        return c;
    });
    public Task<bool> EstadoProducto(Guid id, bool activo) => unidad.Ejecutar(async () =>
    {
        var c = await db.Productos.Include(x => x.ClasificacionProducto)
            .SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO", "Producto no encontrado.", 404);
        if (activo && !c.ClasificacionProducto.Activo)
            throw new ReglaException("CLASIFICACION_NO_ACTIVA", "Activa primero su clasificación.");
        if (!activo && await db.Existencias.AnyAsync(x => x.ProductoId == id && x.CantidadFisica > 0))
            throw new ReglaException("PRODUCTO_CON_EXISTENCIA", "Resuelve las existencias físicas antes de desactivar el producto.");
        if (!activo && (await db.DetallesPedido.AnyAsync(x => x.ProductoId == id && x.Pedido.EstadoComercial != EstadoComercial.CANCELADO && x.Pedido.EstadoComercial != EstadoComercial.CERRADO) || await db.DetallesCotizacion.AnyAsync(x => x.ProductoId == id && (x.Cotizacion.Estado == EstadoCotizacion.BORRADOR || ((x.Cotizacion.Estado == EstadoCotizacion.PRESENTADA || x.Cotizacion.Estado == EstadoCotizacion.ACEPTADA) && x.Cotizacion.VigenteHasta > DateTime.UtcNow)))))
            throw new ReglaException("PRODUCTO_EN_USO", "El producto tiene operaciones comerciales pendientes.");
        c.Activo = activo;
        auditoria.Registrar(activo ? "ACTIVAR" : "DESACTIVAR", "Producto", id);
        return true;
    });
}
