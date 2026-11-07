using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Servicios;

namespace SGCP.Api.Controllers;


[ApiController, Route("api/v1"), Authorize]
public class FinanzasController(SgcpContext db, ServicioFinanzas servicio) : ControllerBase
{
    [HttpGet("saldos"), Authorize(Policy = "saldos.consultar")]
    public async Task<IActionResult> Saldos(Guid? clienteId, Guid? pedidoId)
    {
        var cargos = await servicio.Saldos(clienteId, pedidoId);
        return Ok(new
        {
            Cargos = cargos,
            Total = cargos.Sum(x => x.Saldo),
            PorConcepto = cargos.GroupBy(x => new { x.Tipo, x.Condicion })
                .Select(g => new { g.Key.Tipo, g.Key.Condicion, Saldo = g.Sum(x => x.Saldo) })
        });
    }
    [HttpGet("cargos"), Authorize(Policy = "cargos.administrar")]
    public async Task<IActionResult> Cargos(Guid? pedidoId, int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Cargos.AsNoTracking().Where(x => pedidoId == null || x.PedidoId == pedidoId)
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));
    [HttpPost("cargos"), Authorize(Policy = "cargos.administrar")]
    public async Task<IActionResult> Cargo(CargoPeticion p) => StatusCode(201, new { Id = await servicio.CargoServicio(p) });
    [HttpPost("cargos/{id:guid}/anular"), Authorize(Policy = "cargos.administrar")]
    public async Task<IActionResult> AnularCargo(Guid id, MotivoPeticion p)
    {
        await servicio.AnularCargo(id, p.Motivo);
        return NoContent();
    }
    [HttpGet("pedidos/{id:guid}/condiciones-pago"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Condiciones(Guid id) => Ok(await db.CondicionesPago.AsNoTracking()
        .Where(x => x.PedidoId == id).OrderByDescending(x => x.FechaAcuerdo).ToListAsync());
    [HttpPost("pedidos/{id:guid}/condiciones-pago"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Condicion(Guid id, CondicionPeticion p) => StatusCode(201, new { Id = await servicio.Condicion(id, p) });
    [HttpGet("pagos"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Pagos(Guid? clienteId, int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Pagos.AsNoTracking().Where(x => clienteId == null || x.ClienteId == clienteId)
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));
    [HttpPost("pagos"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Pago(PagoPeticion p) => StatusCode(201, new { Id = await servicio.Pago(p) });
    [HttpPost("pagos/{id:guid}/anular"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> AnularPago(Guid id, MotivoPeticion p)
    {
        await servicio.AnularPago(id, p.Motivo);
        return NoContent();
    }
    [HttpGet("senas"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Senas(Guid? pedidoId, int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Senas.AsNoTracking().Where(x => pedidoId == null || x.PedidoId == pedidoId)
            .OrderBy(x => x.Id).Paginar(pagina, tamanoPagina));
    [HttpPost("senas"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Sena(SenaPeticion p) => StatusCode(201, new { Id = await servicio.Sena(p) });
    [HttpPost("senas/{id:guid}/aplicar"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Aplicar(Guid id, AplicarSenaPeticion p)
    {
        await servicio.AplicarSena(id, p.CargoId);
        return NoContent();
    }
    [HttpPost("senas/{id:guid}/resolver"), Authorize(Policy = "pagos.administrar")]
    public async Task<IActionResult> Resolver(Guid id, MotivoPeticion p)
    {
        await servicio.ResolverSena(id, p.Motivo);
        return NoContent();
    }
}
