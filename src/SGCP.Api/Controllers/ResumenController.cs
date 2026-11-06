using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api.Controllers;

[ApiController, Route("api/v1/resumen"), Authorize]
public class ResumenController(SgcpContext db, ActorActual actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Ver()
    {
        var hoy = DateTime.UtcNow;
        var mes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var cotizaciones = db.Cotizaciones.Where(x => !actor.Comprador || x.ClienteId == actor.ClienteId);
        var pedidos = db.Pedidos.Where(x => !actor.Comprador || x.ClienteId == actor.ClienteId);
        return Ok(new
        {
            ClientesActivos = actor.Tiene("clientes.consultar") ? (int?)await db.Clientes.CountAsync(x => x.Activo) : null,
            ProductosActivos = actor.Tiene("catalogo.consultar") ? (int?)await db.Productos.CountAsync(x => x.Activo) : null,
            CotizacionesVigentes = actor.Tiene("cotizaciones.consultar") ? (int?)await cotizaciones.CountAsync(x => x.VigenteHasta > hoy
                && (x.Estado == EstadoCotizacion.PRESENTADA || x.Estado == EstadoCotizacion.ACEPTADA)) : null,
            PedidosConfirmados = actor.Tiene("pedidos.consultar") ? (int?)await pedidos.CountAsync(x => x.Fecha >= mes
                && x.EstadoComercial == EstadoComercial.CONFIRMADO) : null,
            UltimosPedidos = actor.Tiene("pedidos.consultar") ? await pedidos.OrderByDescending(x => x.Fecha)
                    .Take(6)
                    .Select(x => new { x.Id, x.Codigo, Cliente = x.Cliente.Nombre, x.FechaComprometida, x.EstadoComercial })
                    .ToListAsync() : null
        });
    }
}
