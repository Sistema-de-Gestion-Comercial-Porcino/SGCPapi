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


[ApiController, Route("api/v1"), Authorize(Policy = "inventario.consultar")]
public class InventarioController(SgcpContext db, ServicioInventario servicio) : ControllerBase
{
    [HttpGet("lotes")]
    public async Task<IActionResult> Lotes(int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Lotes.AsNoTracking().OrderByDescending(x => x.FechaProduccion).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    [HttpGet("existencias")]
    public async Task<IActionResult> Existencias(Guid? productoId, int pagina = 1, int tamanoPagina = 20)
    {
        var ahora = DateTime.UtcNow;
        var q = from e in db.Existencias.AsNoTracking()
                join l in db.Lotes on e.LoteId equals l.Id
                join p in db.Productos on e.ProductoId equals p.Id
                where productoId == null || e.ProductoId == productoId
                orderby p.Nombre, l.Codigo
                select new
                {
                    e.Id,
                    e.ProductoId,
                    Producto = p.Nombre,
                    e.LoteId,
                    Lote = l.Codigo,
                    l.Unidad,
                    e.Ubicacion,
                    e.CantidadFisica,
                    l.Estado,
                    l.Vencimiento,
                    Reservada = db.Reservas.Where(r => r.ExistenciaId == e.Id && r.Estado == EstadoReserva.ACTIVA).Sum(r => (decimal?)r.Cantidad) ?? 0,
                    Disponible = l.Estado == EstadoLote.APTO && l.Vencimiento > ahora ? e.CantidadFisica -
                        (db.Reservas.Where(r => r.ExistenciaId == e.Id && r.Estado == EstadoReserva.ACTIVA).Sum(r => (decimal?)r.Cantidad) ?? 0) : 0
                };
        return Ok(await q.Paginar(pagina, tamanoPagina));
    }

    [HttpPost("lotes/externos"), Authorize(Policy = "inventario.administrar")]
    public async Task<IActionResult> Externo(LoteExternoPeticion p) => StatusCode(201, new { Id = await servicio.LoteExterno(p) });

    [HttpPost("lotes/{id:guid}/rechazar"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> Rechazar(Guid id, MotivoPeticion p)
    {
        await servicio.RechazarLote(id, p.Motivo);
        return NoContent();
    }

    [HttpGet("pesajes")]
    public async Task<IActionResult> Pesajes(Guid? loteId, int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Pesajes.AsNoTracking().Where(x => loteId == null || x.LoteId == loteId)
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    [HttpPost("pesajes"), Authorize(Policy = "inventario.administrar")]
    public async Task<IActionResult> Pesar(PesajePeticion p) => StatusCode(201, new { Id = await servicio.Pesar(p) });

    [HttpGet("entradas-inventario")]
    public async Task<IActionResult> Entradas(int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.EntradasInventario.AsNoTracking().OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    [HttpPost("entradas-inventario"), Authorize(Policy = "inventario.administrar")]
    public async Task<IActionResult> Entrada(EntradaPeticion p) => StatusCode(201, new { Id = await servicio.Entrada(p) });

    [HttpPost("entradas-inventario/{id:guid}/anular"), Authorize(Policy = "inventario.administrar")]
    public async Task<IActionResult> Anular(Guid id, MotivoPeticion p)
    {
        await servicio.AnularEntrada(id, p.Motivo);
        return NoContent();
    }

    [HttpGet("reservas")]
    public async Task<IActionResult> Reservas(Guid? detallePedidoId, int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Reservas.AsNoTracking().Where(x => detallePedidoId == null || x.DetallePedidoId == detallePedidoId)
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    [HttpPost("reservas"), Authorize(Policy = "inventario.administrar")]
    public async Task<IActionResult> Reservar(ReservaPeticion p) => StatusCode(201, new { Id = await servicio.Reservar(p) });

    [HttpPost("reservas/{id:guid}/liberar"), Authorize(Policy = "inventario.administrar")]
    public async Task<IActionResult> Liberar(Guid id, MotivoPeticion p)
    {
        await servicio.Liberar(id, p.Motivo);
        return NoContent();
    }

    [HttpGet("movimientos-stock")]
    public async Task<IActionResult> Movimientos(Guid? existenciaId, int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.MovimientosStock.AsNoTracking().Where(x => existenciaId == null || x.ExistenciaId == existenciaId)
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    /// <summary>Lista incidencias. El filtro resuelta es opcional.</summary>
    /// <param name="resuelta">Si se indica, solo devuelve las resueltas o las pendientes.</param>
    /// <param name="pagina">Página desde 1.</param>
    /// <param name="tamanoPagina">10, 20, 50 o 100.</param>
    [HttpGet("incidencias")]
    [ProducesResponseType(typeof(ResultadoPaginado<Incidencia>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Incidencias(bool? resuelta, int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Incidencias.AsNoTracking().Where(x => resuelta == null || x.Resuelta == resuelta)
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    /// <summary>Registra la resolución de una incidencia sin rehabilitar el animal ni el lote.</summary>
    /// <param name="id">Incidencia.</param>
    /// <param name="p">Motivo de la resolución.</param>
    /// <response code="200">Incidencia resuelta.</response>
    /// <response code="400">Ya estaba resuelta o el motivo está vacío.</response>
    /// <response code="404">No existe la incidencia.</response>
    [HttpPost("incidencias/{id:guid}/resolver"), Authorize(Policy = "produccion.administrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolverIncidencia(Guid id, MotivoPeticion p)
    {
        await servicio.ResolverIncidencia(id, p.Motivo);
        return Ok(new { Id = id });
    }
}

[ApiController, Route("api/v1/controles-frio"), Authorize(Policy = "frio.administrar")]
public class FrioController(SgcpContext db, ServicioInventario servicio, SGCP.Api.Seguridad.ActorActual actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.ControlesFrio.AsNoTracking()
            .Where(x => actor.Tiene("inventario.administrar") || db.Entregas.Any(e => e.Id == x.EntregaId && e.ResponsableId == actor.Id))
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    [HttpPost]
    public async Task<IActionResult> Crear(FrioPeticion p) => StatusCode(201, new { Id = await servicio.Frio(p) });
}
