using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Contratos.Respuestas;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api.Controllers;

[ApiController, Route("api/v1/productos"), Authorize(Policy = "catalogo.consultar")]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status403Forbidden)]
public class ProductosController(SgcpContext db, ServicioCatalogo servicio, ActorActual actor) : ControllerBase
{
    private IQueryable<object> Proyectar(IQueryable<Producto> q)
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
    /// <summary>Lista los productos del catálogo con filtros y paginación.</summary>
    /// <param name="busqueda">Nombre o código del producto.</param>
    /// <param name="clasificacionId">Identificador de la clasificación.</param>
    /// <param name="familia">Familia del producto.</param>
    /// <param name="activo">Filtra por productos activos o inactivos.</param>
    /// <param name="pagina">Número de página desde 1.</param>
    /// <param name="tamanoPagina">Cantidad por página. Admite 10, 20, 50 o 100.</param>
    /// <response code="400">Un filtro o la paginación no tiene un valor válido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
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
    /// <summary>Consulta el producto y su precio vigente según los permisos de la sesión.</summary>
    /// <param name="id">Identificador del producto.</param>
    /// <response code="404">El producto no existe o está inactivo para el comprador.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ver(Guid id) => Ok(await Proyectar(db.Productos.Where(x => x.Id == id
        && (!actor.Comprador || x.Activo))).SingleOrDefaultAsync() ?? throw new ReglaException("NO_ENCONTRADO",
            "Producto no encontrado.",
            404));
    /// <summary>Registra un producto en el catálogo.</summary>
    /// <param name="p">Datos del producto y su clasificación activa.</param>
    /// <response code="201">Producto creado. La cabecera Location apunta a su ficha.</response>
    /// <response code="400">Datos inválidos o clasificación inexistente o inactiva.</response>
    /// <response code="409">Código duplicado o conflicto al guardar.</response>
    [HttpPost,
            Authorize(Policy = "catalogo.administrar")]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(ProductoPeticion p)
    {
        var producto = await servicio.Producto(null, p);
        // 201 con Location: GET productos/{id}.
        return CreatedAtAction(nameof(Ver), new { id = producto.Id }, producto);
    }
    /// <summary>Actualiza los datos de un producto del catálogo.</summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="p">Datos actualizados del producto.</param>
    /// <response code="400">Datos inválidos o clasificación inexistente o inactiva.</response>
    /// <response code="404">El producto no existe.</response>
    /// <response code="409">Código duplicado, cambio incompatible con el historial o conflicto al guardar.</response>
    [HttpPut("{id:guid}"),
            Authorize(Policy = "catalogo.administrar")]
    [ProducesResponseType(typeof(Producto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Editar(Guid id,
            ProductoPeticion p) => Ok(await servicio.Producto(id, p));
    /// <summary>Activa o desactiva un producto del catálogo.</summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="p">Estado activo que se desea asignar.</param>
    /// <response code="404">El producto no existe.</response>
    /// <response code="409">La clasificación está inactiva, el producto tiene existencias u operaciones pendientes, o hubo un conflicto al guardar.</response>
    [HttpPost("{id:guid}/estado"),
            Authorize(Policy = "catalogo.administrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Estado(Guid id,
            EstadoPeticion p)
    {
        await servicio.EstadoProducto(id, p.Activo);
        return NoContent();
    }
}
