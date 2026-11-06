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

[ApiController, Route("api/v1/clientes"), Authorize(Policy = "clientes.consultar")]
public class ClientesController(SgcpContext db, ServicioClientes servicio) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(string? busqueda, bool? activo, string? tipo, int pagina = 1, int tamanoPagina = 20)
    {
        var q = db.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(x => x.Nombre.Contains(busqueda) || x.Telefono.Contains(busqueda) || x.IdentificacionFiscal.Contains(busqueda));
        if (activo is not null)
            q = q.Where(x => x.Activo == activo);
        if (!string.IsNullOrWhiteSpace(tipo))
            q = q.Where(x => x.Tipo == tipo);
        return Ok(await q.OrderBy(x => x.Nombre).ThenBy(x => x.Id).Paginar(pagina, tamanoPagina));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Ver(Guid id) => Ok(await db.Clientes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaException("NO_ENCONTRADO",
            "Cliente no encontrado.",
            404));
    [HttpPost,
            Authorize(Policy = "clientes.administrar")]
    public async Task<IActionResult> Crear(ClientePeticion p) => StatusCode(201,
            await servicio.Guardar(null, p));
    [HttpPut("{id:guid}"),
            Authorize(Policy = "clientes.administrar")]
    public async Task<IActionResult> Editar(Guid id,
            ClientePeticion p) => Ok(await servicio.Guardar(id, p));
    [HttpPost("{id:guid}/desactivar"), Authorize(Policy = "clientes.administrar")] public async Task<IActionResult> Desactivar(Guid id) { await servicio.Estado(id, false); return NoContent(); }
    [HttpPost("{id:guid}/activar"), Authorize(Policy = "clientes.administrar")] public async Task<IActionResult> Activar(Guid id) { await servicio.Estado(id, true); return NoContent(); }
}
