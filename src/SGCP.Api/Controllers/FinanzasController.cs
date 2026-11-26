using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Contratos.Respuestas;
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

    /// <summary>Lista seguimientos de cobro, filtrados por cliente y estado.</summary>
    /// <param name="clienteId">Cliente dueño del pedido de la condición.</param>
    /// <param name="estado">PENDIENTE, REALIZADO o CERRADO.</param>
    /// <param name="pagina">Página desde 1.</param>
    /// <param name="tamanoPagina">10, 20, 50 o 100.</param>
    [HttpGet("seguimientos-cobro"), Authorize(Policy = "saldos.consultar")]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Seguimientos(Guid? clienteId, EstadoSeguimiento? estado, int pagina = 1, int tamanoPagina = 20)
    {
        var q = db.SeguimientosCobro.AsNoTracking().Where(x => estado == null || x.Estado == estado);
        if (clienteId is not null)
            q = q.Where(x => db.CondicionesPago.Any(c => c.Id == x.CondicionPagoId
                && db.Pedidos.Any(p => p.Id == c.PedidoId && p.ClienteId == clienteId)));
        // Conserva los campos existentes y resuelve las etiquetas en una consulta paginada.
        var filas = from s in q
                    join c in db.CondicionesPago on s.CondicionPagoId equals c.Id
                    join p in db.Pedidos on c.PedidoId equals p.Id
                    join cliente in db.Clientes on p.ClienteId equals cliente.Id
                    orderby s.ProximaFecha, s.Id
                    select new { s.Id, s.CondicionPagoId, s.FechaGestion, s.Resultado, s.ProximaFecha, s.Estado,
                        PedidoId = p.Id, Pedido = p.Codigo, p.ClienteId, Cliente = cliente.Nombre, c.Vencimiento };
        return Ok(await filas.Paginar(pagina, tamanoPagina));
    }

    /// <summary>Abre un seguimiento en PENDIENTE para una condición a crédito con saldo pendiente.</summary>
    /// <response code="201">Seguimiento creado.</response>
    /// <response code="400">La condición es de contado o el pedido no tiene saldo.</response>
    /// <response code="404">No existe la condición de pago.</response>
    [HttpPost("condiciones-pago/{id:guid}/seguimientos"), Authorize(Policy = "pagos.administrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Seguimiento(Guid id, SeguimientoCobroPeticion p) =>
        StatusCode(201, new { Id = await servicio.CrearSeguimiento(id, p.ProximaFecha) });

    /// <summary>
    /// Registra una gestión. Pasa a REALIZADO y, si queda saldo, vuelve a PENDIENTE con la próxima fecha.
    /// Si el saldo llegó a cero, queda CERRADO.
    /// </summary>
    /// <response code="400">El seguimiento está cerrado, falta el resultado o, con saldo, la próxima fecha.</response>
    /// <response code="404">No existe el seguimiento.</response>
    [HttpPost("seguimientos-cobro/{id:guid}/gestion"), Authorize(Policy = "pagos.administrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Gestion(Guid id, GestionCobroPeticion p) =>
        Ok(new { Id = id, Estado = await servicio.RegistrarGestion(id, p.Resultado, p.ProximaFecha) });

    /// <summary>Cierra el seguimiento cuando el pedido ya no tiene saldo. CERRADO no vuelve a modificarse.</summary>
    /// <response code="204">Seguimiento cerrado.</response>
    /// <response code="400">Sigue habiendo saldo o el seguimiento ya está cerrado.</response>
    /// <response code="404">No existe el seguimiento.</response>
    [HttpPost("seguimientos-cobro/{id:guid}/cerrar"), Authorize(Policy = "pagos.administrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CerrarSeguimiento(Guid id)
    {
        await servicio.CerrarSeguimiento(id);
        return NoContent();
    }
}
