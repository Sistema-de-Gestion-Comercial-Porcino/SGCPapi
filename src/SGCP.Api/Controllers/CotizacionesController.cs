using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Contratos.Respuestas;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api.Controllers;

[ApiController, Route("api/v1/cotizaciones"), Authorize(Policy = "cotizaciones.consultar")]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status403Forbidden)]
public class CotizacionesController(ServicioCotizaciones servicio, SgcpContext db, ActorActual actor) : ControllerBase
{
    /// <summary>Lista las cotizaciones visibles para la sesión con filtros y paginación.</summary>
    /// <param name="busqueda">Código de cotización o nombre del cliente.</param>
    /// <param name="estado">Estado de la cotización.</param>
    /// <param name="clienteId">Identificador del cliente.</param>
    /// <param name="pagina">Número de página desde 1.</param>
    /// <param name="tamanoPagina">Cantidad por página. Admite 10, 20, 50 o 100.</param>
    /// <response code="400">Un filtro o la paginación no tiene un valor válido.</response>
    /// <response code="409">Conflicto al actualizar el estado de cotizaciones vencidas.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Listar(string? busqueda,
            EstadoCotizacion? estado,
            Guid? clienteId,
            int pagina = 1,
            int tamanoPagina = 20)
    {
        await servicio.Vencer();
        var q = servicio.Consulta().AsNoTracking();
        if (actor.Comprador)
            q = q.Where(x => x.ClienteId == actor.ClienteId);
        if (clienteId is not null)
            q = q.Where(x => x.ClienteId == clienteId);
        if (estado is not null)
            q = q.Where(x => x.Estado == estado);
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Codigo.Contains(busqueda) || x.Cliente.Nombre.Contains(busqueda));
        return Ok(await q.OrderByDescending(x => x.Fecha).ThenBy(x => x.Id).Select(x => new
        {
            x.Id,
            x.Codigo,
            x.Version,
            x.Estado,
            x.Fecha,
            x.VigenteHasta,
            PendienteRevision = x.VigenteHasta == null,
            x.ClienteId,
            Cliente = x.Cliente.Nombre
        }).Paginar(pagina, tamanoPagina));
    }
    /// <summary>Consulta la cotización, sus productos, el total y sus referencias de versión y pedido.</summary>
    /// <param name="id">Identificador de la cotización.</param>
    /// <response code="404">La cotización no existe.</response>
    /// <response code="409">Conflicto al actualizar el estado de cotizaciones vencidas.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Ver(Guid id)
    {
        await servicio.Vencer();
        var c = await servicio.Obtener(id);
        var posterior = await db.Cotizaciones.Where(x => x.CotizacionOrigenId == id)
            .Select(x => new { x.Id, x.Codigo, x.Version })
            .SingleOrDefaultAsync();
        return Ok(new
        {
            c.Id,
            c.Codigo,
            c.Version,
            c.Fecha,
            c.VigenteHasta,
            PendienteRevision = c.VigenteHasta == null,
            c.Canal,
            c.Estado,
            c.MotivoRespuesta,
            c.ClienteId,
            Cliente = c.Cliente.Nombre,
            c.CotizacionOrigenId,
            VersionPosterior = posterior,
            PedidoId = c.Pedido?.Id,
            Total = c.VigenteHasta is null ? (decimal?)null : c.Detalles.Sum(x => x.Subtotal),
            Detalles = c.Detalles.Select(x => new
            {
                x.Id,
                x.ProductoId,
                Producto = x.Producto.Nombre,
                x.Cantidad,
                x.Unidad,
                PrecioUnitario = c.VigenteHasta is null ? (decimal?)null : x.PrecioUnitario,
                Subtotal = c.VigenteHasta is null ? (decimal?)null : x.Subtotal
            })
        });
    }
    /// <summary>Envía a Ventas una solicitud del comprador con productos y cantidades.</summary>
    /// <response code="201">Solicitud propia en borrador, sin oferta ni pedido; Location permite consultarla.</response>
    /// <response code="400">Datos inválidos, cliente inactivo o producto no disponible.</response>
    [HttpPost("solicitudes"), Authorize(Policy = "cotizaciones.solicitar"), Authorize(Roles = "COMPRADOR")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Solicitar(SolicitudCotizacionPeticion p)
    {
        var id = await servicio.Solicitar(null, p);
        return CreatedAtAction(nameof(Ver), new { id }, new { Id = id });
    }

    /// <summary>Modifica los productos y cantidades de una solicitud propia aún pendiente de revisión.</summary>
    [HttpPut("{id:guid}/solicitud"), Authorize(Policy = "cotizaciones.solicitar"), Authorize(Roles = "COMPRADOR")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EditarSolicitud(Guid id, SolicitudCotizacionPeticion p) =>
        Ok(new { Id = await servicio.Solicitar(id, p) });

    /// <summary>Crea un borrador de cotización para un cliente activo.</summary>
    /// <param name="p">Cliente, canal, vigencia y productos cotizados.</param>
    /// <response code="201">Devuelve el identificador del borrador y su URL en la cabecera Location.</response>
    /// <response code="400">Datos inválidos, cliente inexistente o inactivo, o vigencia no futura.</response>
    /// <response code="409">Un producto está inactivo o carece de precio vigente, o hubo un conflicto al guardar.</response>
    [HttpPost,
            Authorize(Policy = "cotizaciones.administrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(CotizacionPeticion p)
    {
        var id = await servicio.Guardar(null, p);
        // 201 con Location: GET cotizaciones/{id}.
        return CreatedAtAction(nameof(Ver), new { id }, new { Id = id });
    }
    /// <summary>Actualiza un borrador de cotización.</summary>
    /// <param name="id">Identificador de la cotización.</param>
    /// <param name="p">Datos actualizados de la cotización y sus productos.</param>
    /// <response code="200">Devuelve el identificador de la cotización actualizada.</response>
    /// <response code="404">La cotización no existe.</response>
    /// <response code="409">El estado, la versión o los productos impiden el cambio, o hubo un conflicto al guardar.</response>
    [HttpPut("{id:guid}"),
            Authorize(Policy = "cotizaciones.administrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Editar(Guid id,
            CotizacionPeticion p) => Ok(new { Id = await servicio.Guardar(id, p) });

    /// <summary>Presenta un borrador vigente para que el comprador pueda responder.</summary>
    /// <param name="id">Identificador de la cotización.</param>
    /// <response code="409">La vigencia, el estado, la versión o los datos comerciales impiden presentar la cotización.</response>
    [HttpPost("{id:guid}/presentar"), Authorize(Policy = "cotizaciones.administrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Presentar(Guid id) { await servicio.Cambiar(id, EstadoCotizacion.PRESENTADA); return NoContent(); }

    /// <summary>Registra la aceptación de una cotización presentada y vigente.</summary>
    /// <param name="id">Identificador de la cotización.</param>
    /// <response code="409">La vigencia, el estado, la versión o los datos comerciales impiden aceptar la cotización.</response>
    [HttpPost("{id:guid}/aceptar"), Authorize(Policy = "cotizaciones.responder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Aceptar(Guid id) { await servicio.Cambiar(id, EstadoCotizacion.ACEPTADA); return NoContent(); }

    /// <summary>Rechaza una cotización presentada y registra el motivo.</summary>
    /// <param name="id">Identificador de la cotización.</param>
    /// <param name="p">Motivo del rechazo.</param>
    /// <response code="400">Falta el motivo o no es válido.</response>
    /// <response code="409">La vigencia, el estado, la versión o los datos comerciales impiden rechazar la cotización.</response>
    [HttpPost("{id:guid}/rechazar"),
            Authorize(Policy = "cotizaciones.responder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rechazar(Guid id,
            MotivoPeticion p)
    { await servicio.Cambiar(id, EstadoCotizacion.RECHAZADA, p.Motivo); return NoContent(); }
    /// <summary>Crea una versión de la cotización con una nueva fecha de vencimiento.</summary>
    /// <param name="id">Identificador de la cotización de origen.</param>
    /// <param name="p">Fecha de vencimiento de la nueva versión.</param>
    /// <response code="201">Devuelve el identificador de la nueva versión.</response>
    /// <response code="400">La fecha de vencimiento no es válida o no es futura.</response>
    /// <response code="409">La cotización es un borrador, ya tiene un pedido o ya existe una versión posterior.</response>
    [HttpPost("{id:guid}/nueva-version"),
            Authorize(Policy = "cotizaciones.administrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Version(Guid id,
            NuevaVersionPeticion p) => StatusCode(201,
            new { Id = await servicio.NuevaVersion(id, p.VigenteHasta) });
}
