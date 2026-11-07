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

[ApiController, Route("api/v1/productos"), Authorize(Policy = "catalogo.consultar")]
public class ProductosController(SgcpContext db, ServicioCatalogo servicio, ActorActual actor) : ControllerBase
{
    private IQueryable<object> Proyectar(IQueryable<SGCP.Api.Dominio.Entidades.Producto> q)
    {
        var ahora = DateTime.UtcNow;
        var costos = actor.Tiene("costos.consultar") && !actor.Comprador;
        var verPrecio = actor.Tiene("precios.consultar");
        return q.Select(x => new
        {
            x.Id,
            x.Codigo,
            x.Nombre,
            x.Presentacion,
            x.UnidadVenta,
            x.Descripcion,
            x.Activo,
            x.ClasificacionProductoId,
            Clasificacion = new
            {
                x.ClasificacionProducto.Id,
                x.ClasificacionProducto.Codigo,
                x.ClasificacionProducto.Nombre,
                x.ClasificacionProducto.Familia
            },
            PrecioVigente = x.Precios.Where(p => verPrecio && p.VigenteDesde <= ahora && (p.VigenteHasta == null || p.VigenteHasta > ahora))
                    .Select(p => new { p.PrecioVenta, CostoReferencia = costos ? (decimal?)p.CostoReferencia : null, p.VigenteDesde })
                    .FirstOrDefault()
        });
    }
    [HttpGet]
    public async Task<IActionResult> Listar(string? busqueda,
            Guid? clasificacionId,
            TipoProducto? familia,
            bool? activo,
            int pagina = 1,
            int tamanoPagina = 20)
    {
        var q = db.Productos.AsNoTracking();
        if (actor.Comprador)
            q = q.Where(x => x.Activo && x.ClasificacionProducto.Activo);
        if (activo is not null)
            q = q.Where(x => x.Activo == activo);
        if (clasificacionId is not null)
            q = q.Where(x => x.ClasificacionProductoId == clasificacionId);
        if (familia is not null)
            q = q.Where(x => x.ClasificacionProducto.Familia == familia);
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Nombre.Contains(busqueda) || x.Codigo.Contains(busqueda));
        return Ok(await Proyectar(q.OrderBy(x => x.Nombre).ThenBy(x => x.Id)).Paginar(pagina, tamanoPagina));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Ver(Guid id) => Ok(await Proyectar(db.Productos.Where(x => x.Id == id
        && (!actor.Comprador || x.Activo))).SingleOrDefaultAsync() ?? throw new ReglaException("NO_ENCONTRADO",
            "Producto no encontrado.",
            404));
    [HttpPost,
            Authorize(Policy = "catalogo.administrar")]
    public async Task<IActionResult> Crear(ProductoPeticion p) => StatusCode(201,
            await servicio.Producto(null, p));
    [HttpPut("{id:guid}"),
            Authorize(Policy = "catalogo.administrar")]
    public async Task<IActionResult> Editar(Guid id,
            ProductoPeticion p) => Ok(await servicio.Producto(id, p));
    [HttpPost("{id:guid}/estado"),
            Authorize(Policy = "catalogo.administrar")]
    public async Task<IActionResult> Estado(Guid id,
            EstadoPeticion p)
    {
        await servicio.EstadoProducto(id, p.Activo);
        return NoContent();
    }
}
