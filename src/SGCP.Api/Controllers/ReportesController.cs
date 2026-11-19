using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Patrones.Reportes;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;

namespace SGCP.Api.Controllers;

[ApiController, Route("api/v1/reportes"), Authorize(Policy = "reportes.consultar")]
public class ReportesController(ReportesFacade facade, ExportadorReportes exportador) : ControllerBase
{
    [HttpGet]
    public IActionResult Disponibles() => Ok(facade.Disponibles());
    [HttpGet("{tipo}")]
    public async Task<IActionResult> Consultar(TipoReporte tipo, [FromQuery] FiltroReporte filtro, int versionVista = 1)
    {
        var tabla = await facade.Consultar(tipo, filtro);
        // El cliente anterior usa esta columna como enlace, nunca como texto visible.
        // Las exportaciones siempre utilizan la tabla limpia de Consultar, sin este adaptador.
        if (tipo == TipoReporte.ENTREGAS && versionVista < 2)
            return Ok(tabla with { Columnas = [.. tabla.Columnas, "Entrega"],
                Filas = tabla.Filas.Select((fila, i) => fila.Concat(new object?[] {
                    tabla.Constancias is not null && i < tabla.Constancias.Count ? tabla.Constancias[i] : null }).ToArray()).ToList() });
        return Ok(tabla);
    }
    [HttpGet("{tipo}/exportar/{formato}")]
    public async Task<IActionResult> Exportar(TipoReporte tipo, string formato, [FromQuery] FiltroReporte filtro)
    {
        var reporte = await facade.Consultar(tipo, filtro);
        Response.Headers["Cache-Control"] = "no-store";
        return formato.ToLowerInvariant() switch
        {
            "pdf" => File(exportador.Pdf(reporte), "application/pdf", "SGCP_" + tipo + ".pdf"),
            "xlsx" => File(exportador.Excel(reporte), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SGCP_" + tipo + ".xlsx"),
            _ => throw new ReglaException("FORMATO_INVALIDO", "Selecciona pdf o xlsx.", 400)
        };
    }
}

[ApiController, Route("api/v1"), Authorize]
public class ConstanciasController(SgcpContext db, ActorActual actor, ExportadorReportes exportador) : ControllerBase
{
    [HttpGet("entregas/{id:guid}/constancia")]
    public async Task<IActionResult> Constancia(Guid id) => await Emitir(await db.Entregas.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
        ?? throw new ReglaException("NO_ENCONTRADO", "Entrega no encontrada.", 404));

    // Para quien solo conoce el pedido (Ventas, comprador): toma la entrega ENTREGADA más reciente del pedido.
    [HttpGet("pedidos/{pedidoId:guid}/constancia")]
    public async Task<IActionResult> ConstanciaPorPedido(Guid pedidoId)
    {
        if (!actor.Comprador && !actor.Tiene("saldos.consultar"))
            throw new ReglaException("SIN_PERMISO", "No tienes acceso a esta constancia.", 403);
        var clienteId = await db.Pedidos.AsNoTracking().Where(x => x.Id == pedidoId).Select(x => (Guid?)x.ClienteId).SingleOrDefaultAsync()
            ?? throw new ReglaException("NO_ENCONTRADO", "Pedido no encontrado.", 404);
        if (actor.Comprador)
            actor.VerCliente(clienteId);
        var entrega = await db.Entregas.AsNoTracking().Where(x => x.PedidoId == pedidoId && x.Estado == EstadoEntrega.ENTREGADA)
            .OrderByDescending(x => x.FechaReal).ThenBy(x => x.Id).FirstOrDefaultAsync()
            ?? throw new ReglaException("RECEPCION_PENDIENTE", "El pedido todavía no tiene una entrega confirmada.");
        return await Emitir(entrega);
    }

    private async Task<IActionResult> Emitir(SGCP.Api.Dominio.Entidades.Entrega entrega)
    {
        var id = entrega.Id;
        var pedido = await db.Pedidos.AsNoTracking().Include(x => x.Cliente).SingleAsync(x => x.Id == entrega.PedidoId);
        if (actor.Comprador)
            actor.VerCliente(pedido.ClienteId);
        else if (!actor.Tiene("saldos.consultar"))
            throw new ReglaException("SIN_PERMISO", "No tienes acceso a esta constancia.", 403);
        var recepcion = await db.Recepciones.AsNoTracking().SingleOrDefaultAsync(x => x.EntregaId == id && x.Estado == EstadoRecepcion.CONFIRMADA);
        if (recepcion is null)
            throw new ReglaException("RECEPCION_PENDIENTE", "La recepción todavía no está confirmada en el servidor.");
        if (!recepcion.Conforme || entrega.Estado != EstadoEntrega.ENTREGADA)
            throw new ReglaException("RECEPCION_OBSERVADA", "Una recepción observada no emite constancia definitiva.");
        var evidencias = await db.EvidenciasRecepcion.AsNoTracking().Where(x => x.RecepcionId == recepcion.Id).ToListAsync();
        var tabla = new TablaReporte("Constancia de entrega", ["Dato", "Valor"],
        [
            ["Pedido", pedido.Codigo], ["Cliente", pedido.Cliente.Nombre],
            ["Receptor", recepcion.Receptor], ["Fecha real UTC", recepcion.FechaReal],
            ["Fecha de sincronización UTC", recepcion.FechaSincronizacion], ["Resultado", "Conforme"],
            ["Peso recibido", recepcion.PesoRecibido.ToString("0.000", System.Globalization.CultureInfo.GetCultureInfo("es-BO")) + " " + recepcion.Unidad],
            ["Saldo pendiente al confirmar (Bs)", recepcion.SaldoPendienteAlConfirmar?.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("es-BO")) ?? "Sin dato histórico"],
            ["Observación", recepcion.Observacion]
        ]);
        foreach (var evidencia in evidencias)
            tabla.Filas.Add(["Evidencia SHA-256", evidencia.Sha256]);
        Response.Headers["Cache-Control"] = "no-store";
        return File(exportador.Pdf(tabla, evidencias.Select(x => (x.Tipo, x.Contenido)).ToList()), "application/pdf", "Constancia_" + pedido.Codigo + ".pdf");
    }
}

[ApiController, Route("api/v1/pedidos/{pedidoId:guid}/registro-fiscal"), Authorize(Policy = "fiscal.administrar")]
public class FiscalController(ServicioFiscal servicio, SgcpContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Registrar(Guid pedidoId) => Ok(await servicio.Registrar(pedidoId));
    [HttpGet]
    public async Task<IActionResult> Consultar(Guid pedidoId) => Ok(await db.ComprobantesFiscales.AsNoTracking()
        .SingleOrDefaultAsync(x => x.PedidoId == pedidoId) ?? throw new ReglaException("NO_ENCONTRADO", "Operación fiscal no registrada.", 404));
}
