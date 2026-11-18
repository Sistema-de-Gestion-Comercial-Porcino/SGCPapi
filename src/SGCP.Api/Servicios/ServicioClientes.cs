using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Servicios;

public class ServicioClientes(SgcpContext db, UnidadTrabajo unidad, ServicioAuditoria auditoria)
{
    public Task<Cliente> Guardar(Guid? id, ClientePeticion p) => unidad.Ejecutar(async () =>
    {
        var c = id is null ? new Cliente { FechaAlta = DateTime.UtcNow } : await db.Clientes.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO",
                "Cliente no encontrado.",
                404);
        c.Nombre = p.Nombre.Trim();
        c.Tipo = p.Tipo.Trim();
        c.Telefono = p.Telefono.Trim();
        c.Direccion = p.Direccion?.Trim() ?? "";
        c.IdentificacionFiscal = p.IdentificacionFiscal?.Trim().ToUpperInvariant() ?? "";
        c.CondicionHabitual = p.CondicionHabitual;
        c.Observaciones = p.Observaciones ?? "";
        if (c.Activo
            && c.IdentificacionFiscal != ""
            && await db.Clientes.AnyAsync(x => x.Id != c.Id && x.Activo && x.IdentificacionFiscal == c.IdentificacionFiscal))
            throw new ReglaException("CLIENTE_DUPLICADO", "Ya existe un cliente activo con esa identificación.");
        if (id is null)
            db.Clientes.Add(c);
        auditoria.Registrar(id is null ? "CREAR" : "EDITAR", "Cliente", c.Id);
        return c;
    });
    public Task<bool> Estado(Guid id, bool activo) => unidad.Ejecutar(async () =>
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Cliente no encontrado.", 404);
        if (!activo)
            await VerificarDependenciasAsync(id);
        if (activo
            && c.IdentificacionFiscal != ""
            && await db.Clientes.AnyAsync(x => x.Id != id && x.Activo && x.IdentificacionFiscal == c.IdentificacionFiscal))
            throw new ReglaException("CLIENTE_DUPLICADO", "Otra ficha activa usa esa identificación.");
        c.Activo = activo;
        auditoria.Registrar(activo ? "ACTIVAR" : "DESACTIVAR", "Cliente", id);
        return true;
    });
    /// <summary>
    /// Borrado físico solo sin historial. Un cliente con cotizaciones, pedidos, pagos o cuenta de comprador se desactiva:
    /// borrarlo dejaría saldos y pagos sin dueño y rompería su trazabilidad.
    /// </summary>
    public Task<bool> Eliminar(Guid id) => unidad.Ejecutar(async () =>
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Cliente no encontrado.", 404);
        if (await db.Cotizaciones.AnyAsync(x => x.ClienteId == id)
            || await db.Pedidos.AnyAsync(x => x.ClienteId == id)
            || await db.Pagos.AnyAsync(x => x.ClienteId == id)
            || await db.Usuarios.AnyAsync(x => x.ClienteId == id))
            throw new ReglaException("CLIENTE_CON_HISTORIAL",
                "El cliente tiene cotizaciones, pedidos, pagos o una cuenta de comprador. Desactívalo en lugar de borrarlo.", 409);
        db.Clientes.Remove(c);
        auditoria.Registrar("ELIMINAR_CLIENTE", "Cliente", id);
        return true;
    });
    public async Task VerificarDependenciasAsync(Guid id)
    {
        var ahora = DateTime.UtcNow;
        if (await db.Pedidos.AnyAsync(x => x.ClienteId == id
            && x.EstadoComercial != EstadoComercial.CANCELADO
            && x.EstadoComercial != EstadoComercial.CERRADO))
            throw new ReglaException("CLIENTE_CON_PEDIDOS", "Resuelve los pedidos pendientes antes de desactivar.");
        if (await db.Cotizaciones.AnyAsync(x => x.ClienteId == id
            && x.Pedido == null
                                        && (x.Estado == EstadoCotizacion.BORRADOR || ((x.Estado == EstadoCotizacion.PRESENTADA || x.Estado == EstadoCotizacion.ACEPTADA) && x.VigenteHasta > ahora))))
            throw new ReglaException("CLIENTE_CON_COTIZACIONES", "Resuelve las cotizaciones pendientes antes de desactivar.");
        if (await db.Usuarios.AnyAsync(x => x.ClienteId == id && x.Estado != EstadoUsuario.DESHABILITADO))
            throw new ReglaException("CLIENTE_CON_CUENTA", "Deshabilita primero sus cuentas de acceso.");
    }
}
