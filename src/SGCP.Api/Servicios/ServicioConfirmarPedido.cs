using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Patrones.Comercial;
namespace SGCP.Api.Servicios;

public class ServicioConfirmarPedido(SgcpContext db,
        UnidadTrabajo unidad,
        ServicioCotizaciones cotizaciones,
        ServicioPrecios precios,
        GeneradorCodigo codigos,
        ServicioAuditoria auditoria)
{
    public Task<Guid> ConfirmarAsync(PedidoPeticion p) => unidad.Ejecutar(async () =>
    {
        var c = await cotizaciones.Obtener(p.CotizacionId);
        await cotizaciones.VerificarVersion(c.Id);
        if (c.Pedido is not null)
            throw new ReglaException("COTIZACION_YA_TIENE_PEDIDO", "La cotización ya generó un pedido.");
        if (c.Estado != EstadoCotizacion.ACEPTADA)
            throw new ReglaException("COTIZACION_NO_ACEPTADA", "Solo una cotización aceptada permite confirmar.");
        if (c.VigenteHasta <= DateTime.UtcNow)
            throw new ReglaException("COTIZACION_NO_VIGENTE", "La aceptación ya no está vigente; renueva el acuerdo.");
        if (!c.Cliente.Activo)
            throw new ReglaException("CLIENTE_NO_ACTIVO", "El cliente está desactivado.");
        var builder = new PedidoBuilder()
            .ParaCliente(c.Cliente)
            .DesdeCotizacion(c)
            .ConCodigo(await codigos.SiguienteAsync("PED"))
            .ConFechaComprometida(Consultas.Utc(p.FechaComprometida))
            .ConCondicionPago(p.CondicionPago);
        foreach (var d in c.Detalles)
        {
            if (!d.Producto.Activo)
                throw new ReglaException("PRODUCTO_NO_ACTIVO", "La cotización contiene un producto desactivado.");
            var precio = await precios.Vigente(d.ProductoId) ?? throw new ReglaException("SIN_COSTO_VIGENTE",
                    "El producto no tiene costo vigente.",
                    409,
                    d.Producto.Nombre);
            builder.AgregarDetalle(d.ProductoId, d.Cantidad, d.Unidad, d.PrecioUnitario, precio.CostoReferencia);
        }
        var pedido = builder.Construir();
        db.Pedidos.Add(pedido);
        auditoria.Registrar("CONFIRMAR", "Pedido", pedido.Id);
        return pedido.Id;
    });
    public Task<bool> Cancelar(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        var x = await db.Pedidos.Include(x => x.Detalles)
            .SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO", "Pedido no encontrado.", 404);
        if (x.EstadoComercial != EstadoComercial.CONFIRMADO)
            throw new ReglaException("TRANSICION_INVALIDA", "Solo puedes cancelar un pedido confirmado.");
        x.EstadoComercial = EstadoComercial.CANCELADO;
        x.MotivoCancelacion = motivo.Trim();
        foreach (var d in x.Detalles)
            d.Estado = EstadoDetalle.CANCELADO;
        auditoria.Registrar("CANCELAR: " + motivo.Trim(), "Pedido", id);
        return true;
    });
}
