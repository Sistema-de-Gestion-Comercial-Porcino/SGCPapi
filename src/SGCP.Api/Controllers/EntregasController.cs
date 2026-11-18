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


[ApiController, Route("api/v1"), Authorize(Policy = "entregas.administrar")]
public class EntregasController(SgcpContext db, ServicioEntregas servicio, SGCP.Api.Seguridad.ActorActual actor) : ControllerBase
{
    [HttpGet("transportes")]
    public async Task<IActionResult> Transportes(int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.Transportes.AsNoTracking().OrderBy(x => x.Codigo).Paginar(pagina, tamanoPagina));
    [HttpPost("transportes")]
    public async Task<IActionResult> Transporte(TransportePeticion p) => StatusCode(201, new { Id = await servicio.Transporte(null, p) });
    [HttpPut("transportes/{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, TransportePeticion p) => Ok(new { Id = await servicio.Transporte(id, p) });
    [HttpPost("transportes/{id:guid}/estado")]
    public async Task<IActionResult> Estado(Guid id, EstadoPeticion p)
    {
        await servicio.EstadoTransporte(id, p.Activo);
        return NoContent();
    }
    [HttpGet("responsables-entrega")]
    public async Task<IActionResult> Responsables() => Ok(await db.Usuarios.AsNoTracking()
        .Where(x => x.Estado == EstadoUsuario.ACTIVO && x.Roles.Any(r => r.Nombre == "ENTREGA" || r.Nombre == "ADMINISTRACION"))
        .Select(x => new { x.Id, x.Nombre }).ToListAsync());
    [HttpGet("rutas")]
    public async Task<IActionResult> Rutas(int pagina = 1, int tamanoPagina = 20) => Ok(await db.Rutas.AsNoTracking()
        .Where(x => actor.Principal.IsInRole("ADMINISTRACION") || x.ResponsableId == actor.Id)
        .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));
    [HttpPost("rutas")]
    public async Task<IActionResult> Ruta(RutaPeticion p) => StatusCode(201, new { Id = await servicio.CrearRuta(p) });
    [HttpPost("rutas/{id:guid}/finalizar")]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        await servicio.FinalizarRuta(id);
        return NoContent();
    }
    [HttpGet("entregas")]
    public async Task<IActionResult> Entregas(int pagina = 1, int tamanoPagina = 20) => Ok(await db.Entregas.AsNoTracking()
        .Where(x => actor.Principal.IsInRole("ADMINISTRACION") || x.ResponsableId == actor.Id)
        .OrderByDescending(x => x.FechaPrevista).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));
    [HttpPost("entregas")]
    public async Task<IActionResult> Preparar(EntregaPeticion p) => StatusCode(201, new { Id = await servicio.Preparar(p) });
    [HttpPost("entregas/{id:guid}/despachar")]
    public async Task<IActionResult> Despachar(Guid id)
    {
        await servicio.Despachar(id);
        return NoContent();
    }
    [HttpPost("entregas/{id:guid}/reasignar"), Authorize(Policy = "entregas.revisar")]
    public async Task<IActionResult> Reasignar(Guid id, ReasignarPeticion p)
    {
        await servicio.Reasignar(id, p);
        return NoContent();
    }
    [HttpGet("mis-entregas")]
    public async Task<IActionResult> Asignadas()
    {
        var entregas = await (from e in db.Entregas.AsNoTracking()
                              join p in db.Pedidos on e.PedidoId equals p.Id
                              join c in db.Clientes on p.ClienteId equals c.Id
                              where e.ResponsableId == actor.Id && e.Estado == EstadoEntrega.DESPACHADA && e.Modalidad == ModalidadEntrega.DOMICILIO
                              orderby e.FechaPrevista, e.OrdenEnRuta
                              select new
                              {
                                  e.Id,
                                  e.Version,
                                  e.PedidoId,
                                  Pedido = p.Codigo,
                                  e.RutaId,
                                  e.Modalidad,
                                  e.FechaPrevista,
                                  e.Estado,
                                  e.ResponsableId,
                                  Cliente = c.Nombre,
                                  c.Direccion,
                                  c.Telefono
                              }).Take(200).ToListAsync();
        var ids = entregas.Select(x => x.PedidoId).ToArray();
        var detalles = await db.DetallesPedido.AsNoTracking().Where(x => ids.Contains(x.PedidoId))
            .Select(x => new { x.PedidoId, x.ProductoId, Producto = x.Producto.Nombre, x.CantidadConfirmada, x.Unidad }).ToListAsync();
        return Ok(new
        {
            Entregas = entregas,
            Detalles = detalles
        });
    }
}

[ApiController, Route("api/v1/recepciones"), Authorize]
public class RecepcionesController(SgcpContext db, ServicioEntregas servicio, SGCP.Api.Seguridad.ActorActual actor) : ControllerBase
{
    [HttpPost("sincronizar"), Authorize(Policy = "entregas.administrar"), RequestSizeLimit(12582912)]
    public async Task<IActionResult> Sincronizar(SincronizarPeticion p) => Ok(new { Resultados = await servicio.Sincronizar(p) });
    [HttpGet("conflictos"), Authorize(Policy = "entregas.revisar")]
    public async Task<IActionResult> Conflictos(int pagina = 1, int tamanoPagina = 20) =>
        Ok(await (from r in db.Recepciones.AsNoTracking()
                  join e in db.Entregas on r.EntregaId equals e.Id
                  join p in db.Pedidos on e.PedidoId equals p.Id
                  where r.Estado == EstadoRecepcion.CONFLICTO
                  orderby r.FechaSincronizacion, r.Id
                  // Versión y estado actuales de la entrega: Ventas los necesita para revisar (VersionActual)
                  // sin tener entregas.administrar.
                  select new
                  {
                      r.Id,
                      r.EntregaId,
                      r.ClaveUnica,
                      r.VersionBase,
                      r.AutorId,
                      r.FechaReal,
                      r.FechaSincronizacion,
                      r.PesoRecibido,
                      r.Unidad,
                      r.Conforme,
                      r.Receptor,
                      r.Observacion,
                      r.Estado,
                      r.MotivoConflicto,
                      r.RevisadaPorId,
                      r.RevisadaEn,
                      r.Resolucion,
                      VersionEntrega = e.Version,
                      EstadoEntrega = e.Estado,
                      e.PedidoId,
                      Pedido = p.Codigo
                  }).Paginar(pagina, tamanoPagina));
    [HttpPost("{id:guid}/revisar"), Authorize(Policy = "entregas.revisar")]
    public async Task<IActionResult> Revisar(Guid id, RevisarRecepcionPeticion p)
    {
        await servicio.Revisar(id, p);
        return NoContent();
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Ver(Guid id)
    {
        var (r, e) = await Acceso(id);
        var evidencias = await db.EvidenciasRecepcion.AsNoTracking().Where(x => x.RecepcionId == id)
            .Select(x => new { x.Id, x.Tipo, x.Sha256 }).ToListAsync();
        return Ok(new
        {
            Recepcion = new
            {
                r.Id,
                r.EntregaId,
                r.ClaveUnica,
                r.VersionBase,
                r.AutorId,
                r.FechaReal,
                r.FechaSincronizacion,
                r.PesoRecibido,
                r.Unidad,
                r.Conforme,
                r.Receptor,
                r.Observacion,
                r.Estado,
                r.MotivoConflicto,
                r.RevisadaPorId,
                r.RevisadaEn,
                r.Resolucion
            },
            Entrega = new
            {
                e.Id,
                e.Version,
                e.Estado,
                e.PedidoId
            },
            Evidencias = evidencias
        });
    }
    [HttpGet("{id:guid}/evidencias/{evidenciaId:guid}")]
    public async Task<IActionResult> Evidencia(Guid id, Guid evidenciaId)
    {
        await Acceso(id);
        var e = await db.EvidenciasRecepcion.AsNoTracking().SingleOrDefaultAsync(x => x.Id == evidenciaId && x.RecepcionId == id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Evidencia no encontrada.", 404);
        Response.Headers["Cache-Control"] = "no-store";
        return File(e.Contenido, e.Tipo, e.Id + (e.Tipo == "image/png" ? ".png" : ".jpg"));
    }
    private async Task<(Recepcion Recepcion, Entrega Entrega)> Acceso(Guid id)
    {
        var r = await db.Recepciones.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Recepción no encontrada.", 404);
        var e = await servicio.Obtener(r.EntregaId);
        if (actor.Comprador)
        {
            var cliente = await db.Pedidos.Where(x => x.Id == e.PedidoId).Select(x => x.ClienteId).SingleAsync();
            actor.VerCliente(cliente);
        }
        else if (!actor.Tiene("entregas.revisar") && e.ResponsableId != actor.Id && r.AutorId != actor.Id)
            throw new ReglaException("SIN_PERMISO", "No tienes acceso a esta recepción.", 403);
        return (r, e);
    }
}
