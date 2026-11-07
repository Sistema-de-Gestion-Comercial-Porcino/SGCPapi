using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Patrones.Fiscal;
using SGCP.Api.Seguridad;

namespace SGCP.Api.Servicios;

public class ServicioFiscal(SgcpContext db, UnidadTrabajo unidad, ActorActual actor,
    ServicioAuditoria auditoria, IProveedorFiscal proveedor)
{
    public Task<ComprobanteFiscal> Registrar(Guid pedidoId) => unidad.Ejecutar(async () =>
    {
        var existente = await db.ComprobantesFiscales.SingleOrDefaultAsync(x => x.PedidoId == pedidoId);
        if (existente is not null)
            return existente;
        var pedido = await db.Pedidos.FindAsync(pedidoId) ?? throw new ReglaException("NO_ENCONTRADO", "Pedido no encontrado.", 404);
        if (pedido.EstadoComercial != EstadoComercial.CONFIRMADO)
            throw new ReglaException("PEDIDO_NO_VIGENTE", "El pedido no está confirmado.");
        var referencia = "FIS-" + pedidoId.ToString("N");
        var respuesta = await proveedor.Registrar(referencia, pedidoId);
        var operacion = new ComprobanteFiscal
        {
            PedidoId = pedidoId,
            ReferenciaUnica = referencia,
            Estado = respuesta.Estado,
            NumeroFiscal = respuesta.Numero,
            RespuestaProveedor = respuesta.Respuesta,
            Fecha = DateTime.UtcNow,
            ResponsableId = actor.Id
        };
        db.ComprobantesFiscales.Add(operacion);
        auditoria.Registrar("REGISTRAR_SIN_COMPROBANTE_FISCAL", "ComprobanteFiscal", operacion.Id);
        return operacion;
    });
}
