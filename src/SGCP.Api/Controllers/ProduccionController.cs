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


[ApiController, Route("api/v1"), Authorize(Policy = "produccion.consultar")]
public class ProduccionController(SgcpContext db, ServicioProduccion servicio) : ControllerBase
{
    [HttpGet("animales")]
    public async Task<IActionResult> Animales(string? busqueda, EstadoAnimal? estado, int pagina = 1, int tamanoPagina = 20)
    {
        var q = db.Animales.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Identificacion.Contains(busqueda.Trim()) || x.Proveedor.Contains(busqueda.Trim()));
        if (estado is not null)
            q = q.Where(x => x.Estado == estado);
        return Ok(await q.OrderBy(x => x.Identificacion).Paginar(pagina, tamanoPagina));
    }

    [HttpPost("animales"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> CrearAnimal(AnimalPeticion p) => StatusCode(201, new { Id = await servicio.Animal(null, p) });

    [HttpPut("animales/{id:guid}"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> EditarAnimal(Guid id, AnimalPeticion p) => Ok(new { Id = await servicio.Animal(id, p) });

    [HttpPost("animales/{id:guid}/estado"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> EstadoAnimal(Guid id, EstadoPeticion p)
    {
        await servicio.CambiarEstadoAnimal(id, p.Activo);
        return NoContent();
    }

    [HttpGet("animales/{id:guid}/controles-sanitarios")]
    public async Task<IActionResult> Controles(Guid id) => Ok(await db.ControlesSanitarios.AsNoTracking()
        .Where(x => x.AnimalId == id).OrderByDescending(x => x.Fecha).ToListAsync());

    [HttpPost("animales/{id:guid}/controles-sanitarios"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> Sanidad(Guid id, SanidadPeticion p) => StatusCode(201, new { Id = await servicio.Sanidad(id, p) });

    [HttpGet("ordenes-produccion")]
    public async Task<IActionResult> Ordenes(int pagina = 1, int tamanoPagina = 20) =>
        Ok(await db.OrdenesProduccion.AsNoTracking().OrderByDescending(x => x.FechaCreacion).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));

    [HttpGet("ordenes-produccion/{id:guid}")]
    public async Task<IActionResult> Orden(Guid id)
    {
        var orden = await db.OrdenesProduccion.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Orden no encontrada.", 404);
        var resultados = await (from r in db.ResultadosProduccion.AsNoTracking()
                                join p in db.Productos on r.ProductoId equals p.Id
                                join c in db.ClasificacionesProducto on p.ClasificacionProductoId equals c.Id
                                join l in db.Lotes on r.LoteId equals l.Id
                                where r.OrdenProduccionId == id
                                select new
                                {
                                    r.Id,
                                    r.ProductoId,
                                    Producto = p.Nombre,
                                    TipoCarneODerivado = c.Nombre,
                                    c.Familia,
                                    r.LoteId,
                                    Lote = l.Codigo,
                                    r.Estado,
                                    r.CantidadReal,
                                    l.Unidad
                                }).ToListAsync();
        var insumos = await db.InsumosProduccion.AsNoTracking().Where(x => x.OrdenProduccionId == id).ToListAsync();
        var previsiones = await db.PrevisionesProducto.AsNoTracking().Where(x => x.OrdenProduccionId == id).ToListAsync();
        return Ok(new
        {
            Orden = orden,
            Previsiones = previsiones,
            Resultados = resultados,
            Insumos = insumos
        });
    }

    [HttpPost("ordenes-produccion"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> Crear(OrdenPeticion p) => StatusCode(201, new { Id = await servicio.Crear(p) });

    [HttpPost("ordenes-produccion/{id:guid}/iniciar"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> Iniciar(Guid id)
    {
        await servicio.Iniciar(id);
        return NoContent();
    }

    [HttpPost("ordenes-produccion/{id:guid}/consumos"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> Consumir(Guid id, InsumoPeticion p) => StatusCode(201, new { Id = await servicio.Consumir(id, p) });

    [HttpPost("ordenes-produccion/{id:guid}/confirmar"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> Confirmar(Guid id, ConfirmarProduccionPeticion p) => Ok(new { Lotes = await servicio.Confirmar(id, p) });

    [HttpPost("ordenes-produccion/{id:guid}/anular"), Authorize(Policy = "produccion.administrar")]
    public async Task<IActionResult> Anular(Guid id, MotivoPeticion p)
    {
        await servicio.Anular(id, p.Motivo);
        return NoContent();
    }
}
