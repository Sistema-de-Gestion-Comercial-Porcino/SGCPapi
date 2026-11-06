using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
namespace SGCP.Api.Servicios;

public class ServicioPrecios(SgcpContext db, UnidadTrabajo unidad, ServicioAuditoria auditoria)
{
    public async Task<PrecioProducto?> Vigente(Guid producto, DateTime? fecha = null)
    {
        var ahora = fecha ?? DateTime.UtcNow;
        return await db.PreciosProducto.Where(x => x.ProductoId == producto
            && x.VigenteDesde <= ahora
            && (x.VigenteHasta == null || x.VigenteHasta > ahora))
            .OrderByDescending(x => x.VigenteDesde)
            .FirstOrDefaultAsync();
    }
    public Task<PrecioProducto> Abrir(Guid producto, PrecioPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar(p);
        if (!await db.Productos.AnyAsync(x => x.Id == producto && x.Activo))
            throw new ReglaException("PRODUCTO_NO_ACTIVO", "Selecciona un producto activo.");
        var inicio = Consultas.Utc(p.VigenteDesde);
        var historial = await db.PreciosProducto.Where(x => x.ProductoId == producto).ToListAsync();
        if (historial.Any(x => x.VigenteDesde >= inicio || x.VigenteHasta > inicio))
            throw new ReglaException("VIGENCIA_SUPERPUESTA", "La nueva vigencia debe comenzar después de las anteriores.");
        var anterior = historial.SingleOrDefault(x => x.VigenteHasta == null);
        if (anterior is not null)
        { anterior.VigenteHasta = inicio; await db.SaveChangesAsync(); }
        var n = new PrecioProducto
        {
            ProductoId = producto,
            CostoReferencia = p.CostoReferencia,
            PrecioVenta = p.PrecioVenta,
            VigenteDesde = inicio
        };
        db.PreciosProducto.Add(n);
        auditoria.Registrar("ABRIR_VIGENCIA", "PrecioProducto", n.Id);
        return n;
    });
    public Task<PrecioProducto> Editar(Guid id, PrecioPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar(p);
        var x = await db.PreciosProducto.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Vigencia no encontrada.", 404);
        var inicio = Consultas.Utc(p.VigenteDesde);
        if (x.VigenteHasta <= inicio)
            throw new ReglaException("FECHAS_INVALIDAS", "El inicio debe preceder al fin.", 400);
        if (await db.PreciosProducto.AnyAsync(y => y.ProductoId == x.ProductoId
            && y.Id != id
            && (x.VigenteHasta == null || y.VigenteDesde < x.VigenteHasta)
            && (y.VigenteHasta == null || y.VigenteHasta > inicio)))
            throw new ReglaException("VIGENCIA_SUPERPUESTA", "La vigencia se cruza con otra.");
        x.CostoReferencia = p.CostoReferencia;
        x.PrecioVenta = p.PrecioVenta;
        x.VigenteDesde = inicio;
        auditoria.Registrar("CORREGIR_VIGENCIA", "PrecioProducto", id);
        return x;
    });
    public Task<bool> Finalizar(Guid id, DateTime fin) => unidad.Ejecutar(async () =>
    {
        var x = await db.PreciosProducto.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Vigencia no encontrada.", 404);
        fin = Consultas.Utc(fin);
        if (x.VigenteHasta is not null || fin <= x.VigenteDesde)
            throw new ReglaException("FECHAS_INVALIDAS", "Finaliza una vigencia abierta después de su inicio.", 400);
        x.VigenteHasta = fin;
        auditoria.Registrar("FINALIZAR_VIGENCIA", "PrecioProducto", id);
        return true;
    });
    private static void Validar(PrecioPeticion p)
    {
        if (p.CostoReferencia != Consultas.Dinero(p.CostoReferencia) || p.PrecioVenta != Consultas.Dinero(p.PrecioVenta))
            throw new ReglaException("DECIMALES_INVALIDOS", "Los importes admiten dos decimales.", 400);
        if (p.VigenteDesde == default)
            throw new ReglaException("FECHA_REQUERIDA", "Indica el inicio de vigencia.", 400);
        if (p.PrecioVenta <= p.CostoReferencia && !p.ConfirmarMargenNoPositivo)
            throw new ReglaException("CONFIRMAR_MARGEN", "El precio no supera el costo. Confirma expresamente para guardar.");
    }
}
