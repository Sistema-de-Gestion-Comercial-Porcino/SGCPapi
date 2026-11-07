using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;

namespace SGCP.Api.Patrones.Reportes;

public enum TipoReporte
{
    CLIENTES, COTIZACIONES, PEDIDOS, PRODUCCION, INVENTARIO, PAGOS, ENTREGAS
}
public record FiltroReporte(DateTime? Desde = null, DateTime? Hasta = null, Guid? ProductoId = null, Guid? ClienteId = null, string? Estado = null, Guid? ClasificacionId = null);
public record TablaReporte(string Titulo, string[] Columnas, List<object?[]> Filas);

public class ReportesFacade(SgcpContext db, ActorActual actor)
{
    private static readonly Dictionary<string, TipoReporte[]> Matriz = new()
    {
        ["ADMINISTRACION"] = Enum.GetValues<TipoReporte>(),
        ["VENTAS"] = [TipoReporte.CLIENTES, TipoReporte.COTIZACIONES, TipoReporte.PEDIDOS, TipoReporte.PAGOS, TipoReporte.ENTREGAS],
        ["PRODUCCION"] = [TipoReporte.PRODUCCION, TipoReporte.INVENTARIO],
        ["ALMACEN"] = [TipoReporte.PRODUCCION, TipoReporte.INVENTARIO],
        ["ENTREGA"] = [TipoReporte.ENTREGAS],
        ["COMPRADOR"] = [TipoReporte.COTIZACIONES, TipoReporte.PEDIDOS, TipoReporte.PAGOS, TipoReporte.ENTREGAS]
    };
    public TipoReporte[] Disponibles() => actor.Comprador ? Matriz["COMPRADOR"] :
        Matriz.Where(x => actor.Principal.IsInRole(x.Key)).SelectMany(x => x.Value).Distinct().ToArray();

    public async Task<TablaReporte> Consultar(TipoReporte tipo, FiltroReporte f)
    {
        if (!Disponibles().Contains(tipo))
            throw new ReglaException("REPORTE_NO_PERMITIDO", "Tu perfil no puede consultar este reporte.", 403);
        if ((f.Desde is not null && f.Desde.Value.Kind != DateTimeKind.Utc) || (f.Hasta is not null && f.Hasta.Value.Kind != DateTimeKind.Utc) || f.Desde > f.Hasta)
            throw new ReglaException("FILTRO_INVALIDO", "Usa un intervalo UTC válido.", 400);
        if ((f.ProductoId is not null || f.ClasificacionId is not null) && tipo is TipoReporte.CLIENTES or TipoReporte.PAGOS)
            throw new ReglaException("FILTRO_INVALIDO", "Este reporte no admite filtro por producto.", 400);
        if (f.ClienteId is not null && tipo is TipoReporte.PRODUCCION or TipoReporte.INVENTARIO)
            throw new ReglaException("FILTRO_INVALIDO", "Este reporte no admite filtro por cliente.", 400);
        if (actor.Comprador)
        {
            if (f.ClienteId is Guid cliente)
                actor.VerCliente(cliente);
            f = f with
            {
                ClienteId = actor.ClienteId ?? throw new ReglaException("SIN_CLIENTE", "Cuenta sin cliente asociado.", 403)
            };
        }
        var costos = actor.Tiene("costos.consultar") && !actor.Comprador;
        var desde = f.Desde ?? DateTime.MinValue;
        var hasta = f.Hasta ?? DateTime.MaxValue;
        TablaReporte tabla;
        switch (tipo)
        {
            case TipoReporte.CLIENTES:
                {
                    bool? activo = f.Estado is null ? null : f.Estado == "ACTIVO" ? true : f.Estado == "INACTIVO" ? false :
                        throw new ReglaException("FILTRO_INVALIDO", "Estado: ACTIVO o INACTIVO.", 400);
                    var filas = await db.Clientes.AsNoTracking().Where(x => x.FechaAlta >= desde && x.FechaAlta <= hasta &&
                        (f.ClienteId == null || x.Id == f.ClienteId) && (activo == null || x.Activo == activo))
                        .OrderBy(x => x.Nombre).ThenBy(x => x.Id).Select(x => new { x.Id, x.Nombre, x.Tipo, x.Telefono, x.Activo }).Take(10001).ToListAsync();
                    tabla = new("Clientes", ["Id", "Nombre", "Tipo", "Teléfono", "Activo"], filas.Select(x => new object?[] { x.Id, x.Nombre, x.Tipo, x.Telefono, x.Activo }).ToList());
                    break;
                }
            case TipoReporte.COTIZACIONES:
                {
                    var estado = Estado<EstadoCotizacion>(f.Estado);
                    var filas = await db.Cotizaciones.AsNoTracking().Where(x => x.Fecha >= desde && x.Fecha <= hasta &&
                        (f.ClienteId == null || x.ClienteId == f.ClienteId) && (estado == null || x.Estado == estado) &&
                        (f.ProductoId == null || x.Detalles.Any(d => d.ProductoId == f.ProductoId)) &&
                    (f.ClasificacionId == null || x.Detalles.Any(d => d.Producto.ClasificacionProductoId == f.ClasificacionId)))
                        .OrderBy(x => x.Fecha).ThenBy(x => x.Id)
                        .Select(x => new
                        {
                            x.Codigo,
                            x.Version,
                            x.Fecha,
                            Cliente = x.Cliente.Nombre,
                            x.Estado,
                            x.VigenteHasta,
                            Total = x.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
                        }).Take(10001).ToListAsync();
                    tabla = new("Cotizaciones", ["Código", "Versión", "Fecha UTC", "Cliente", "Estado", "Vigente hasta UTC", "Total Bs"],
                        filas.Select(x => new object?[] { x.Codigo, x.Version, x.Fecha, x.Cliente, x.Estado.ToString(), x.VigenteHasta, Consultas.Dinero(x.Total) }).ToList());
                    break;
                }
            case TipoReporte.PEDIDOS:
                {
                    var estado = Estado<EstadoComercial>(f.Estado);
                    var filas = await db.Pedidos.AsNoTracking().Where(x => x.Fecha >= desde && x.Fecha <= hasta &&
                        (f.ClienteId == null || x.ClienteId == f.ClienteId) && (estado == null || x.EstadoComercial == estado) &&
                        (f.ProductoId == null || x.Detalles.Any(d => d.ProductoId == f.ProductoId)) &&
                    (f.ClasificacionId == null || x.Detalles.Any(d => d.Producto.ClasificacionProductoId == f.ClasificacionId)))
                        .OrderBy(x => x.Fecha).ThenBy(x => x.Id)
                        .Select(x => new
                        {
                            x.Codigo,
                            x.Fecha,
                            Cliente = x.Cliente.Nombre,
                            x.EstadoComercial,
                            x.EstadoPreparacion,
                            Total = x.Detalles.Sum(d => d.CantidadConfirmada * d.PrecioAcordado),
                            Costo = costos ? x.CostoEstimadoConfirmado : (decimal?)null,
                            Margen = costos ? x.MargenEstimadoConfirmado : (decimal?)null
                        })
                        .Take(10001).ToListAsync();
                    string[] columnas = ["Código", "Fecha UTC", "Cliente", "Estado comercial", "Preparación", "Total Bs"];
                    if (costos)
                        columnas = [.. columnas, "Costo histórico Bs", "Margen histórico Bs"];
                    tabla = new("Pedidos", columnas, filas.Select(x =>
                    {
                        object?[] fila = [x.Codigo, x.Fecha, x.Cliente, x.EstadoComercial.ToString(), x.EstadoPreparacion.ToString(), Consultas.Dinero(x.Total)];
                        return costos ? [.. fila, x.Costo, x.Margen] : fila;
                    }).ToList());
                    break;
                }
            case TipoReporte.PRODUCCION:
                {
                    var estado = Estado<EstadoResultado>(f.Estado);
                    var filas = await (from r in db.ResultadosProduccion.AsNoTracking()
                                       join l in db.Lotes on r.LoteId equals l.Id
                                       join p in db.Productos on r.ProductoId equals p.Id
                                       join c in db.ClasificacionesProducto on p.ClasificacionProductoId equals c.Id
                                       join o in db.OrdenesProduccion on r.OrdenProduccionId equals o.Id
                                       where (f.ClasificacionId == null || c.Id == f.ClasificacionId) && r.FechaRegistro >= desde && r.FechaRegistro <= hasta && (f.ProductoId == null || r.ProductoId == f.ProductoId) && (estado == null || r.Estado == estado)
                                       orderby r.FechaRegistro, r.Id
                                       select new
                                       {
                                           Orden = o.Codigo,
                                           Producto = p.Nombre,
                                           Tipo = c.Nombre,
                                           c.Familia,
                                           Lote = l.Codigo,
                                           r.CantidadReal,
                                           l.Unidad,
                                           r.Estado
                                       }).Take(10001).ToListAsync();
                    tabla = new("Producción por producto y lote", ["Orden", "Producto", "Corte o derivado", "Familia", "Lote", "Cantidad real", "Unidad", "Estado"],
                        filas.Select(x => new object?[] { x.Orden, x.Producto, x.Tipo, x.Familia.ToString(), x.Lote, x.CantidadReal, x.Unidad, x.Estado.ToString() }).ToList());
                    break;
                }
            case TipoReporte.INVENTARIO:
                {
                    var estado = Estado<EstadoLote>(f.Estado);
                    var ahora = DateTime.UtcNow;
                    var filas = await (from e in db.Existencias.AsNoTracking()
                                       join l in db.Lotes on e.LoteId equals l.Id
                                       join p in db.Productos on e.ProductoId equals p.Id
                                       where (f.ClasificacionId == null || p.ClasificacionProductoId == f.ClasificacionId) && l.FechaProduccion >= desde && l.FechaProduccion <= hasta && (f.ProductoId == null || e.ProductoId == f.ProductoId) && (estado == null || l.Estado == estado)
                                       orderby p.Nombre, l.Codigo
                                       select new
                                       {
                                           Producto = p.Nombre,
                                           Lote = l.Codigo,
                                           e.CantidadFisica,
                                           l.Unidad,
                                           l.Estado,
                                           l.Vencimiento,
                                           Reservada = db.Reservas.Where(r => r.ExistenciaId == e.Id && r.Estado == EstadoReserva.ACTIVA).Sum(r => (decimal?)r.Cantidad) ?? 0
                                       }).Take(10001).ToListAsync();
                    tabla = new("Inventario físico actual (fechas por producción del lote)", ["Producto", "Lote", "Físico", "Reservado", "Disponible", "Unidad", "Estado", "Vencimiento UTC"],
                        filas.Select(x => new object?[] { x.Producto, x.Lote, x.CantidadFisica, x.Reservada,
                        x.Estado == EstadoLote.APTO && x.Vencimiento > ahora ? x.CantidadFisica - x.Reservada : 0, x.Unidad, x.Estado.ToString(), x.Vencimiento }).ToList());
                    break;
                }
            case TipoReporte.PAGOS:
                {
                    bool? anulado = f.Estado is null ? null : f.Estado == "ANULADO" ? true : f.Estado == "REGISTRADO" ? false :
                        throw new ReglaException("FILTRO_INVALIDO", "Estado: REGISTRADO o ANULADO.", 400);
                    var filas = await (from p in db.Pagos.AsNoTracking()
                                       join c in db.Clientes on p.ClienteId equals c.Id
                                       where p.Fecha >= desde && p.Fecha <= hasta && (f.ClienteId == null || p.ClienteId == f.ClienteId) && (anulado == null || p.Anulado == anulado)
                                       orderby p.Fecha, p.Id
                                       select new
                                       {
                                           p.ReferenciaUnica,
                                           Cliente = c.Nombre,
                                           p.Fecha,
                                           p.Tipo,
                                           p.Importe,
                                           p.Medio,
                                           p.Anulado
                                       }).Take(10001).ToListAsync();
                    tabla = new("Pagos recibidos y señas", ["Referencia", "Cliente", "Fecha UTC", "Tipo", "Importe Bs", "Medio", "Anulado"],
                        filas.Select(x => new object?[] { x.ReferenciaUnica, x.Cliente, x.Fecha, x.Tipo.ToString(), x.Importe, x.Medio, x.Anulado }).ToList());
                    break;
                }
            case TipoReporte.ENTREGAS:
                {
                    var estado = Estado<EstadoEntrega>(f.Estado);
                    var soloAsignadas = !actor.Comprador && !actor.Principal.IsInRole("ADMINISTRACION") && !actor.Principal.IsInRole("VENTAS");
                    var filas = await (from e in db.Entregas.AsNoTracking()
                                       join p in db.Pedidos on e.PedidoId equals p.Id
                                       join c in db.Clientes on p.ClienteId equals c.Id
                                       where e.FechaPrevista >= desde && e.FechaPrevista <= hasta && (f.ClienteId == null || p.ClienteId == f.ClienteId)
                                           && (estado == null || e.Estado == estado) && (!soloAsignadas || e.ResponsableId == actor.Id)
                                           && (f.ProductoId == null || p.Detalles.Any(d => d.ProductoId == f.ProductoId))
                    && (f.ClasificacionId == null || p.Detalles.Any(d => d.Producto.ClasificacionProductoId == f.ClasificacionId))
                                       orderby e.FechaPrevista, e.Id
                                       select new
                                       {
                                           Pedido = p.Codigo,
                                           Cliente = c.Nombre,
                                           e.FechaPrevista,
                                           e.FechaReal,
                                           e.Estado,
                                           e.Receptor
                                       }).Take(10001).ToListAsync();
                    tabla = new("Entregas", ["Pedido", "Cliente", "Fecha prevista UTC", "Fecha real UTC", "Estado", "Receptor"],
                        filas.Select(x => new object?[] { x.Pedido, x.Cliente, x.FechaPrevista, x.FechaReal, x.Estado.ToString(), x.Receptor }).ToList());
                    break;
                }
            default:
                throw new ReglaException("REPORTE_INVALIDO", "Reporte no disponible.", 400);
        }
        if (tabla.Filas.Count > 10000)
            throw new ReglaException("REPORTE_MUY_AMPLIO", "Acota los filtros a un máximo de 10000 filas.", 400);
        return tabla;
    }

    private static T? Estado<T>(string? texto) where T : struct, Enum => texto is null ? null :
        Enum.TryParse<T>(texto, out var valor) && Enum.IsDefined(valor) ? valor :
        throw new ReglaException("FILTRO_INVALIDO", "Estado no válido para este reporte.", 400);
}
