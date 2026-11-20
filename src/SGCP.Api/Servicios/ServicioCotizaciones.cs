using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
namespace SGCP.Api.Servicios;

public class ServicioCotizaciones(SgcpContext db,
        UnidadTrabajo unidad,
        ServicioAuditoria auditoria,
        ServicioPrecios precios,
        GeneradorCodigo codigos,
        ActorActual actor)
{
    public IQueryable<Cotizacion> Consulta() => db.Cotizaciones.Include(x => x.Cliente)
        .Include(x => x.Detalles)
        .ThenInclude(x => x.Producto)
        .Include(x => x.Pedido);
    public async Task Vencer()
    {
        var vencidas = await db.Cotizaciones.Where(x => x.Estado == EstadoCotizacion.PRESENTADA
            && x.VigenteHasta <= DateTime.UtcNow
            && (!actor.Comprador || x.ClienteId == actor.ClienteId))
            .ToListAsync();
        foreach (var c in vencidas)
        { c.Estado = EstadoCotizacion.VENCIDA; auditoria.Registrar("VENCER", "Cotizacion", c.Id); }
        if (vencidas.Count > 0)
            await db.SaveChangesAsync();
    }
    public async Task<Cotizacion> Obtener(Guid id)
    {
        var c = await Consulta()
            .SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO", "Cotización no encontrada.", 404);
        actor.VerCliente(c.ClienteId);
        return c;
    }
    public Task<Guid> Solicitar(Guid? id, SolicitudCotizacionPeticion p) => unidad.Ejecutar(async () =>
    {
        if (!actor.Comprador || actor.ClienteId is not Guid clienteId)
            throw new ReglaException("COMPRADOR_REQUERIDO", "Inicia sesión con una cuenta de comprador vinculada a un cliente.", 403);
        if (!await db.Clientes.AnyAsync(x => x.Id == clienteId && x.Activo))
            throw new ReglaException("CLIENTE_NO_ACTIVO", "Tu ficha de cliente está inactiva; contacta a Ventas.", 400);
        if (p.Detalles is null || p.Detalles.Count is < 1 or > 100 || p.Detalles.Any(x => x is null))
            throw new ReglaException("DATOS_INVALIDOS", "Selecciona entre 1 y 100 productos.", 400);

        var c = id is null
            ? new Cotizacion { Codigo = await codigos.SiguienteAsync("COT"), Fecha = DateTime.UtcNow,
                ClienteId = clienteId, Canal = CanalSolicitud.PORTAL }
            : await Obtener(id.Value);
        if (c.Estado != EstadoCotizacion.BORRADOR || c.VigenteHasta is not null || c.Canal != CanalSolicitud.PORTAL)
            throw new ReglaException("SOLICITUD_YA_REVISADA", "Ventas ya preparó esta cotización; solicita los cambios mediante su respuesta.");
        await VerificarVersion(c.Id);
        if (id is not null) db.DetallesCotizacion.RemoveRange(c.Detalles);
        c.Detalles = new List<DetalleCotizacion>();
        foreach (var d in p.Detalles)
        {
            if (d.Cantidad <= 0 || d.Cantidad > 999999.999m || d.Cantidad != decimal.Round(d.Cantidad, 3))
                throw new ReglaException("CANTIDAD_INVALIDA", "Indica una cantidad positiva con hasta tres decimales.", 400);
            var producto = await db.Productos.SingleOrDefaultAsync(x => x.Id == d.ProductoId && x.Activo)
                ?? throw new ReglaException("PRODUCTO_NO_ACTIVO", "Uno de los productos ya no está disponible; revisa tu solicitud.", 400);
            c.Detalles.Add(new DetalleCotizacion { ProductoId = producto.Id, Cantidad = d.Cantidad, Unidad = producto.UnidadVenta });
        }
        if (id is null) db.Cotizaciones.Add(c);
        else db.DetallesCotizacion.AddRange(c.Detalles);
        auditoria.Registrar(id is null ? "SOLICITAR" : "EDITAR_SOLICITUD", "Cotizacion", c.Id);
        return c.Id;
    });
    public Task<Guid> Guardar(Guid? id, CotizacionPeticion p) => unidad.Ejecutar(async () =>
    {
        if (!await db.Clientes.AnyAsync(x => x.Id == p.ClienteId && x.Activo))
            throw new ReglaException("CLIENTE_NO_ACTIVO", "Selecciona un cliente activo.", 400);
        if (Consultas.Utc(p.VigenteHasta) <= DateTime.UtcNow)
            throw new ReglaException("VIGENCIA_INVALIDA", "La cotización debe vencer en el futuro.", 400);
        var c = id is null ? new Cotizacion { Codigo = await codigos.SiguienteAsync("COT"), Fecha = DateTime.UtcNow } : await Obtener(id.Value);
        if (c.Estado != EstadoCotizacion.BORRADOR)
            throw new ReglaException("TRANSICION_INVALIDA", "Crea una versión nueva para modificar una cotización presentada.");
        await VerificarVersion(c.Id);
        c.ClienteId = p.ClienteId;
        c.Canal = p.Canal;
        c.VigenteHasta = Consultas.Utc(p.VigenteHasta);
        if (id is not null)
            db.DetallesCotizacion.RemoveRange(c.Detalles);
        c.Detalles = new List<DetalleCotizacion>();
        foreach (var d in p.Detalles)
        {
            var producto = await db.Productos.SingleOrDefaultAsync(x => x.Id == d.ProductoId
                && x.Activo) ?? throw new ReglaException("PRODUCTO_NO_ACTIVO", "Uno de los productos no está habilitado.");
            if (producto.UnidadVenta != d.Unidad)
                throw new ReglaException("UNIDAD_INCOMPATIBLE", "Usa la unidad de venta del producto.", 400);
            if (d.Cantidad != decimal.Round(d.Cantidad, 3) || d.PrecioUnitario != Consultas.Dinero(d.PrecioUnitario))
                throw new ReglaException("DECIMALES_INVALIDOS", "Cantidad: tres decimales. Precio: dos.", 400);
            if (await precios.Vigente(d.ProductoId) is null)
                throw new ReglaException("SIN_PRECIO_VIGENTE", "El producto requiere costo y precio vigentes.");
            c.Detalles.Add(new DetalleCotizacion
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                Unidad = d.Unidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = Consultas.Dinero(d.Cantidad * d.PrecioUnitario)
            });
        }
        if (id is null)
            db.Cotizaciones.Add(c);
        else
            db.DetallesCotizacion.AddRange(c.Detalles);
        auditoria.Registrar(id is null ? "CREAR" : "EDITAR", "Cotizacion", c.Id);
        return c.Id;
    });
    public Task<bool> Cambiar(Guid id, EstadoCotizacion estado, string motivo = "") => unidad.Ejecutar(async () =>
    {
        var c = await Obtener(id);
        await VerificarVersion(id);
        if (c.VigenteHasta is null)
            throw new ReglaException("SOLICITUD_PENDIENTE", "Ventas debe revisar los productos, precios y vigencia antes de presentar la cotización.");
        var esperado = estado == EstadoCotizacion.PRESENTADA ? EstadoCotizacion.BORRADOR : EstadoCotizacion.PRESENTADA;
        if (c.VigenteHasta <= DateTime.UtcNow)
            throw new ReglaException("COTIZACION_NO_VIGENTE", "La cotización venció; prepara otra versión.");
        if (c.Estado != esperado)
            throw new ReglaException("TRANSICION_INVALIDA", "Esta acción no corresponde al estado actual.");
        if (c.Detalles.Any(x => !x.Producto.Activo))
            throw new ReglaException("PRODUCTO_NO_ACTIVO", "Revisa los productos de esta versión antes de continuar.");
        if (c.Detalles.Count == 0)
            throw new ReglaException("COTIZACION_SIN_DETALLE", "Agrega al menos un producto.");
        if (estado == EstadoCotizacion.RECHAZADA && string.IsNullOrWhiteSpace(motivo))
            throw new ReglaException("MOTIVO_REQUERIDO", "Registra el motivo de la respuesta.", 400);
        if (!c.Cliente.Activo)
            throw new ReglaException("CLIENTE_NO_ACTIVO", "El cliente fue desactivado.");
        c.Estado = estado;
        c.MotivoRespuesta = motivo.Trim();
        auditoria.Registrar(estado.ToString(), "Cotizacion", id);
        return true;
    });
    public Task<Guid> NuevaVersion(Guid id, DateTime hasta) => unidad.Ejecutar(async () =>
    {
        var anterior = await Obtener(id);
        await VerificarVersion(id);
        if (anterior.Pedido is not null || anterior.Estado == EstadoCotizacion.BORRADOR)
            throw new ReglaException("VERSION_NO_PERMITIDA", "Edita el borrador o crea una cotización independiente para un pedido ya confirmado.");
        hasta = Consultas.Utc(hasta);
        if (hasta <= DateTime.UtcNow)
            throw new ReglaException("VIGENCIA_INVALIDA", "Indica una vigencia futura.", 400);
        var c = new Cotizacion
        {
            Codigo = await codigos.SiguienteAsync("COT"),
            Version = anterior.Version + 1,
            CotizacionOrigenId = id,
            ClienteId = anterior.ClienteId,
            Canal = anterior.Canal,
            Fecha = DateTime.UtcNow,
            VigenteHasta = hasta,
            Detalles = anterior.Detalles.Select(d => new DetalleCotizacion
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                Unidad = d.Unidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            })
                    .ToList()
        };
        db.Cotizaciones.Add(c);
        auditoria.Registrar("NUEVA_VERSION", "Cotizacion", c.Id);
        return c.Id;
    });
    public async Task VerificarVersion(Guid id)
    {
        if (await db.Cotizaciones.AnyAsync(x => x.CotizacionOrigenId == id))
            throw new ReglaException("VERSION_SUPERADA", "Existe una versión más reciente; utiliza esa cotización.");
    }
}
