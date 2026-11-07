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
    public async Task<IActionResult> Consultar(TipoReporte tipo, [FromQuery] FiltroReporte filtro) => Ok(await facade.Consultar(tipo, filtro));
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
    public async Task<IActionResult> Constancia(Guid id)
    {
        var entrega = await db.Entregas.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Entrega no encontrada.", 404);
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
            ["Pedido", pedido.Codigo], ["Entrega", entrega.Id], ["Cliente", pedido.Cliente.Nombre],
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
