using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Contratos.Respuestas;
using SGCP.Api.Datos;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api.Controllers;

/// <summary>
/// Clientes: recurso principal de la API. Responde siempre con <see cref="ClienteRespuesta"/>, nunca con la entidad.
/// Pedidos es el recurso relacionado (un cliente tiene muchos pedidos).
/// </summary>
[ApiController, Route("api/v1/clientes"), Authorize, Produces("application/json")]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status403Forbidden)]
public class ClientesController(SgcpContext db, ServicioClientes servicio, ActorActual actor) : ControllerBase
{
    /// <summary>Lista los clientes con búsqueda por nombre, teléfono o identificación, y filtros por estado y tipo.</summary>
    /// <param name="busqueda">Texto que se busca en el nombre, el teléfono o la identificación fiscal.</param>
    /// <param name="activo">true solo activos, false solo inactivos; vacío, todos.</param>
    /// <param name="tipo">Tipo de cliente, por ejemplo Restaurante.</param>
    /// <param name="pagina">Página desde 1.</param>
    /// <param name="tamanoPagina">10, 20, 50 o 100.</param>
    [HttpGet, Authorize(Policy = "clientes.consultar")]
    [ProducesResponseType(typeof(ResultadoPaginado<ClienteRespuesta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginado<ClienteRespuesta>>> Listar(string? busqueda, bool? activo, string? tipo, int pagina = 1, int tamanoPagina = 20)
    {
        var q = db.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Nombre.Contains(busqueda) || x.Telefono.Contains(busqueda) || x.IdentificacionFiscal.Contains(busqueda));
        if (activo is not null)
            q = q.Where(x => x.Activo == activo);
        if (!string.IsNullOrWhiteSpace(tipo))
            q = q.Where(x => x.Tipo == tipo);
        return Ok(await q.OrderBy(x => x.Nombre).ThenBy(x => x.Id)
            .Select(x => new ClienteRespuesta(x.Id, x.Nombre, x.Tipo, x.IdentificacionFiscal, x.Telefono, x.Direccion, x.CondicionHabitual, x.Activo, x.FechaAlta, x.Observaciones, x.TipoDocumento))
            .Paginar(pagina, tamanoPagina));
    }

    /// <summary>Consulta un cliente por su identificador.</summary>
    [HttpGet("{id:guid}"), Authorize(Policy = "clientes.consultar")]
    [ProducesResponseType(typeof(ClienteRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteRespuesta>> Ver(Guid id) =>
        ClienteRespuesta.Desde(await db.Clientes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Cliente no encontrado.", 404));

    /// <summary>Registra un cliente. Responde 201 con la cabecera Location apuntando a GET clientes/{id}.</summary>
    /// <response code="201">Cliente creado; Location lleva su URL.</response>
    /// <response code="400">Faltan campos obligatorios o superan su largo.</response>
    /// <response code="409">Ya existe un cliente activo con esa identificación fiscal.</response>
    [HttpPost, Authorize(Policy = "clientes.administrar")]
    [ProducesResponseType(typeof(ClienteRespuesta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteRespuesta>> Crear(ClientePeticion p)
    {
        var respuesta = ClienteRespuesta.Desde(await servicio.Guardar(null, p));
        return CreatedAtAction(nameof(Ver), new { id = respuesta.Id }, respuesta);
    }

    /// <summary>Actualiza los datos de un cliente.</summary>
    /// <response code="409">Otra ficha activa usa esa identificación fiscal.</response>
    [HttpPut("{id:guid}"), Authorize(Policy = "clientes.administrar")]
    [ProducesResponseType(typeof(ClienteRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteRespuesta>> Editar(Guid id, ClientePeticion p) =>
        ClienteRespuesta.Desde(await servicio.Guardar(id, p));

    /// <summary>
    /// Elimina un cliente sin historial. Si tiene cotizaciones, pedidos, pagos o una cuenta de comprador responde 409:
    /// borrarlo rompería la trazabilidad de sus saldos, así que se desactiva en lugar de borrarse.
    /// </summary>
    /// <response code="204">Cliente eliminado y registrado en la auditoría.</response>
    /// <response code="404">El cliente no existe.</response>
    /// <response code="409">CLIENTE_CON_HISTORIAL: hay que desactivarlo.</response>
    [HttpDelete("{id:guid}"), Authorize(Policy = "clientes.administrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        await servicio.Eliminar(id);
        return NoContent();
    }

    /// <summary>Pedidos del cliente (recurso relacionado), del más reciente al más antiguo. El comprador solo ve los suyos.</summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="pagina">Página desde 1.</param>
    /// <param name="tamanoPagina">10, 20, 50 o 100.</param>
    [HttpGet("{id:guid}/pedidos"), Authorize(Policy = "pedidos.consultar")]
    [ProducesResponseType(typeof(ResultadoPaginado<PedidoClienteRespuesta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultadoPaginado<PedidoClienteRespuesta>>> Pedidos(Guid id, int pagina = 1, int tamanoPagina = 20)
    {
        if (!await db.Clientes.AnyAsync(x => x.Id == id))
            throw new ReglaException("NO_ENCONTRADO", "Cliente no encontrado.", 404);
        actor.VerCliente(id);
        return Ok(await db.Pedidos.AsNoTracking().Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.Fecha).ThenBy(x => x.Id)
            .Select(x => new PedidoClienteRespuesta(x.Id, x.Codigo, x.Fecha, x.EstadoComercial,
                Math.Round(x.Detalles.Sum(d => d.CantidadConfirmada * d.PrecioAcordado), 2)))
            .Paginar(pagina, tamanoPagina));
    }

    /// <summary>Desactiva un cliente. No se permite con pedidos o cotizaciones pendientes ni con cuentas activas.</summary>
    [HttpPost("{id:guid}/desactivar"), Authorize(Policy = "clientes.administrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Desactivar(Guid id) { await servicio.Estado(id, false); return NoContent(); }

    /// <summary>Reactiva un cliente si su identificación fiscal no la usa otra ficha activa.</summary>
    [HttpPost("{id:guid}/activar"), Authorize(Policy = "clientes.administrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activar(Guid id) { await servicio.Estado(id, true); return NoContent(); }
}
