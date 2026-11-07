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


[ApiController, Route("api/v1/pedidos-operativos"), Authorize(Policy = "operacion.consultar")]
public class PedidosOperativosController(SgcpContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(int pagina = 1, int tamanoPagina = 20) => Ok(await db.Pedidos.AsNoTracking()
        .Where(x => x.EstadoComercial == EstadoComercial.CONFIRMADO && !db.Entregas.Any(e => e.PedidoId == x.Id && e.Estado != EstadoEntrega.PREPARADA))
        .OrderBy(x => x.FechaComprometida).ThenBy(x => x.Id)
        .Select(x => new
        {
            x.Id,
            x.Codigo,
            x.FechaComprometida,
            x.EstadoPreparacion,
            Cliente = x.Cliente.Nombre,
            Detalles = x.Detalles.Select(d => new
            {
                d.Id,
                d.ProductoId,
                Producto = d.Producto.Nombre,
                d.CantidadConfirmada,
                d.Unidad,
                d.Estado
            })
        }).Paginar(pagina, tamanoPagina));
}
