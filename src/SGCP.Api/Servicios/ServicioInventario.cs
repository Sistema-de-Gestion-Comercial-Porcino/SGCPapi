using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;

namespace SGCP.Api.Servicios;

public class ServicioInventario(SgcpContext db, UnidadTrabajo unidad, ActorActual actor, ServicioAuditoria auditoria)
{
    public async Task<Lote> LoteApto(Guid id)
    {
        var lote = await db.Lotes.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Lote no encontrado.", 404);
        if (lote.Estado != EstadoLote.APTO || lote.Vencimiento <= DateTime.UtcNow)
            throw new ReglaException("LOTE_NO_APTO", "El lote está bloqueado o vencido.");
        return lote;
    }

    public async Task<decimal> Disponible(Existencia existencia)
    {
        await LoteApto(existencia.LoteId);
        var cantidades = await db.Reservas.Where(x => x.ExistenciaId == existencia.Id && x.Estado == EstadoReserva.ACTIVA)
            .Select(x => x.Cantidad).ToListAsync();
        return existencia.CantidadFisica - cantidades.Sum();
    }

    public void Movimiento(Existencia existencia, decimal cantidad, string tipo, Guid referencia, string motivo = "")
    {
        existencia.CantidadFisica += cantidad;
        db.MovimientosStock.Add(new MovimientoStock
        {
            ExistenciaId = existencia.Id,
            Cantidad = cantidad,
            Tipo = tipo,
            ReferenciaId = referencia,
            ResponsableId = actor.Id,
            Fecha = DateTime.UtcNow,
            Motivo = motivo
        });
    }

    public Task<Guid> LoteExterno(LoteExternoPeticion p) => unidad.Ejecutar(async () =>
    {
        var producto = await db.Productos.FindAsync(p.ProductoId);
        if (producto is null || !producto.Activo)
            throw new ReglaException("PRODUCTO_NO_ACTIVO", "Selecciona un producto activo.");
        Validar.FechaReal(p.FechaProduccion);
        Validar.Previstas(p.FechaProduccion, p.Vencimiento);
        if (p.Vencimiento <= DateTime.UtcNow)
            throw new ReglaException("LOTE_VENCIDO", "No se puede recibir un lote vencido.");
        var lote = new Lote
        {
            Codigo = p.Codigo.Trim(),
            ProductoId = p.ProductoId,
            Procedencia = p.Procedencia.Trim(),
            ReferenciaExterna = p.Referencia.Trim(),
            Unidad = producto.UnidadVenta,
            FechaProduccion = p.FechaProduccion,
            Vencimiento = p.Vencimiento,
            Estado = EstadoLote.APTO
        };
        db.Lotes.Add(lote);
        auditoria.Registrar("LOTE_EXTERNO_DOCUMENTADO", "Lote", lote.Id);
        return lote.Id;
    });

    public Task<Guid> Pesar(PesajePeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Cantidad(p.PesoBruto);
        if (p.Tara < 0 || p.Tara >= p.PesoBruto || decimal.Round(p.Tara, 3) != p.Tara)
            throw new ReglaException("TARA_INVALIDA", "La tara debe ser no negativa y menor al peso bruto.", 400);
        var lote = await LoteApto(p.LoteId);
        if (lote.Unidad != p.Unidad)
            throw new ReglaException("UNIDAD_INCOMPATIBLE", "El pesaje debe usar la unidad del producto.");
        if (await db.Pesajes.AnyAsync(x => x.LoteId == lote.Id))
            throw new ReglaException("LOTE_YA_PESADO", "El lote ya tiene un pesaje confirmado.");
        var pesaje = new Pesaje
        {
            LoteId = lote.Id,
            ProductoId = lote.ProductoId,
            PesoBruto = p.PesoBruto,
            Tara = p.Tara,
            Neto = p.PesoBruto - p.Tara,
            Unidad = p.Unidad,
            Fecha = DateTime.UtcNow,
            ResponsableId = actor.Id,
            Observacion = (p.Observacion?.Trim() ?? "")
        };
        var resultado = await db.ResultadosProduccion.SingleOrDefaultAsync(x => x.LoteId == lote.Id);
        if (resultado is not null)
        {
            resultado.CantidadReal = pesaje.Neto;
            resultado.Estado = EstadoResultado.CONFIRMADO;
        }
        db.Pesajes.Add(pesaje);
        auditoria.Registrar("PESAJE_SIN_MOVIMIENTO_STOCK", "Pesaje", pesaje.Id);
        return pesaje.Id;
    });

    public Task<Guid> Entrada(EntradaPeticion p) => unidad.Ejecutar(async () =>
    {
        var peso = await db.Pesajes.FindAsync(p.PesajeId) ?? throw new ReglaException("NO_ENCONTRADO", "Pesaje no encontrado.", 404);
        await LoteApto(peso.LoteId);
        if (await db.EntradasInventario.AnyAsync(x => x.LoteId == peso.LoteId && x.ProductoId == peso.ProductoId))
            throw new ReglaException("ENTRADA_YA_REGISTRADA", "Este lote y producto ya tienen una entrada física.");
        var existencia = new Existencia { LoteId = peso.LoteId, ProductoId = peso.ProductoId, Ubicacion = p.Ubicacion.Trim() };
        var entrada = new EntradaInventario
        {
            PesajeId = peso.Id,
            LoteId = peso.LoteId,
            ProductoId = peso.ProductoId,
            ExistenciaId = existencia.Id,
            Cantidad = peso.Neto,
            ResponsableId = actor.Id,
            Fecha = DateTime.UtcNow
        };
        db.Existencias.Add(existencia);
        db.EntradasInventario.Add(entrada);
        Movimiento(existencia, peso.Neto, "ENTRADA", entrada.Id);
        auditoria.Registrar("ENTRADA_FISICA", "EntradaInventario", entrada.Id);
        return entrada.Id;
    });

    public Task<bool> AnularEntrada(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        var entrada = await db.EntradasInventario.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Entrada no encontrada.", 404);
        if (entrada.Anulada)
            throw new ReglaException("ENTRADA_ANULADA", "La entrada ya fue anulada.");
        var existencia = (await db.Existencias.FindAsync(entrada.ExistenciaId))!;
        if (existencia.CantidadFisica != entrada.Cantidad || await db.Reservas.AnyAsync(x => x.ExistenciaId == existencia.Id && x.Estado != EstadoReserva.LIBERADA))
            throw new ReglaException("ENTRADA_EN_USO", "No se puede anular stock consumido, despachado o reservado.");
        entrada.Anulada = true;
        entrada.AnuladaEn = DateTime.UtcNow;
        entrada.AnuladaPorId = actor.Id;
        entrada.MotivoAnulacion = motivo.Trim();
        Movimiento(existencia, -entrada.Cantidad, "ANULACION_ENTRADA", entrada.Id, motivo.Trim());
        auditoria.Registrar("ANULAR_ENTRADA", "EntradaInventario", id);
        return true;
    });

    public Task<Guid> Reservar(ReservaPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Cantidad(p.Cantidad);
        var detalle = await db.DetallesPedido.Include(x => x.Pedido).SingleOrDefaultAsync(x => x.Id == p.DetallePedidoId)
            ?? throw new ReglaException("NO_ENCONTRADO", "Detalle no encontrado.", 404);
        if (detalle.Pedido.EstadoComercial != EstadoComercial.CONFIRMADO)
            throw new ReglaException("PEDIDO_NO_VIGENTE", "El pedido no está confirmado.");
        var existencia = await db.Existencias.FindAsync(p.ExistenciaId)
            ?? throw new ReglaException("SIN_EXISTENCIA_DISPONIBLE", "No hay existencia física para reservar.");
        var reservadas = await db.Reservas.Where(x => x.DetallePedidoId == detalle.Id && x.Estado != EstadoReserva.LIBERADA)
            .Select(x => x.Cantidad).ToListAsync();
        if (existencia.ProductoId != detalle.ProductoId || reservadas.Sum() + p.Cantidad > detalle.CantidadConfirmada)
            throw new ReglaException("RESERVA_INVALIDA", "El producto o la cantidad no corresponden al pedido.");
        if (await Disponible(existencia) < p.Cantidad)
            throw new ReglaException("SIN_EXISTENCIA_DISPONIBLE", "Solo se reserva stock físico apto, descontando las reservas.");
        var reserva = new Reserva
        {
            DetallePedidoId = detalle.Id,
            ExistenciaId = existencia.Id,
            Cantidad = p.Cantidad,
            Estado = EstadoReserva.ACTIVA,
            Fecha = DateTime.UtcNow,
            ResponsableId = actor.Id
        };
        db.Reservas.Add(reserva);
        detalle.Estado = reservadas.Sum() + p.Cantidad == detalle.CantidadConfirmada ? EstadoDetalle.RESERVADO : EstadoDetalle.PENDIENTE;
        auditoria.Registrar("RESERVAR", "Reserva", reserva.Id);
        return reserva.Id;
    });

    public Task<bool> Liberar(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        var r = await db.Reservas.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Reserva no encontrada.", 404);
        if (r.Estado != EstadoReserva.ACTIVA)
            throw new ReglaException("RESERVA_NO_ACTIVA", "Solo se libera una reserva activa.");
        r.Estado = EstadoReserva.LIBERADA;
        r.MotivoLiberacion = motivo.Trim();
        var detalle = (await db.DetallesPedido.FindAsync(r.DetallePedidoId))!;
        detalle.Estado = EstadoDetalle.PENDIENTE;
        var pedido = (await db.Pedidos.FindAsync(detalle.PedidoId))!;
        pedido.EstadoPreparacion = EstadoPreparacion.PENDIENTE;
        auditoria.Registrar("LIBERAR", "Reserva", id);
        return true;
    });

    /// <summary>Bloquea el lote (en revisión), libera sus reservas, abre su incidencia y alcanza a los derivados que lo consumieron.</summary>
    /// <param name="id">Lote que se bloquea.</param>
    /// <param name="motivo">Texto de la incidencia.</param>
    /// <param name="visitados">Lotes ya recorridos en la cascada.</param>
    /// <param name="desde">Si se conoce cuándo empezó el problema, solo los consumos posteriores contaminan a los derivados;
    /// sin fecha, por precaución, se bloquea todo derivado que consumió del lote.</param>
    public async Task BloquearLote(Guid id, string motivo, HashSet<Guid>? visitados = null, DateTime? desde = null)
    {
        visitados ??= [];
        if (!visitados.Add(id))
            return;
        var lote = await db.Lotes.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Lote no encontrado.", 404);
        lote.Estado = EstadoLote.BLOQUEADO;
        var resultado = await db.ResultadosProduccion.SingleOrDefaultAsync(x => x.LoteId == id);
        if (resultado is not null)
            resultado.Estado = EstadoResultado.BLOQUEADO;
        db.Incidencias.Add(new Incidencia { LoteId = id, Motivo = motivo, Fecha = DateTime.UtcNow, ResponsableId = actor.Id });
        var existencias = db.Existencias.Where(x => x.LoteId == id).Select(x => x.Id);
        var reservas = await db.Reservas.Where(x => existencias.Contains(x.ExistenciaId) && x.Estado == EstadoReserva.ACTIVA).ToListAsync();
        foreach (var reserva in reservas)
        {
            reserva.Estado = EstadoReserva.LIBERADA;
            reserva.MotivoLiberacion = "Bloqueo sanitario o de calidad del lote";
            var detalle = (await db.DetallesPedido.FindAsync(reserva.DetallePedidoId))!;
            detalle.Estado = EstadoDetalle.PENDIENTE;
            var pedido = (await db.Pedidos.FindAsync(detalle.PedidoId))!;
            pedido.EstadoPreparacion = EstadoPreparacion.BLOQUEADO;
        }
        var ordenes = await db.InsumosProduccion.Where(x => existencias.Contains(x.ExistenciaId) && (desde == null || x.Fecha >= desde))
            .Select(x => x.OrdenProduccionId).Distinct().ToListAsync();
        foreach (var ordenId in ordenes)
        {
            var orden = (await db.OrdenesProduccion.FindAsync(ordenId))!;
            if (orden.Estado is not (EstadoOrdenProduccion.CONFIRMADA or EstadoOrdenProduccion.ANULADA))
                orden.Estado = EstadoOrdenProduccion.BLOQUEADA;
            foreach (var hijo in await db.Lotes.Where(x => x.OrdenProduccionId == ordenId).Select(x => x.Id).ToListAsync())
                await BloquearLote(hijo, "Insumo rechazado: " + lote.Codigo, visitados);
        }
        auditoria.Registrar("BLOQUEAR_SIN_REVERTIR_CONSUMO", "Lote", id);
    }

    public Task<bool> RechazarLote(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        await BloquearLote(id, motivo.Trim());
        return true;
    });

    public Task<bool> ResolverIncidencia(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        var incidencia = await db.Incidencias.FindAsync(id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Incidencia no encontrada.", 404);
        if (incidencia.Resuelta)
            throw new ReglaException("INCIDENCIA_YA_RESUELTA", "La incidencia ya está resuelta.", 400);
        incidencia.Resuelta = true;
        incidencia.Resolucion = motivo.Trim();
        incidencia.ResueltaEn = DateTime.UtcNow;
        auditoria.Registrar("RESOLVER", "Incidencia", id);
        return true;
    });

    public Task<Guid> Frio(FrioPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.FechaReal(p.Fecha);
        if (p.InicioFalla is DateTime inicio)
        {
            Validar.FechaReal(inicio);
            if (inicio > p.Fecha)
                throw new ReglaException("INICIO_FALLA_INVALIDO", "El inicio de la falla debe ser anterior o igual a la hora de la lectura.", 400);
        }
        var camara = p.LoteId is null && p.EntregaId is null;
        if (camara && !actor.Tiene("inventario.administrar"))
            throw new ReglaException("SIN_PERMISO", "El control de una cámara completa requiere permiso de inventario.", 403);
        if (camara && p.Evaluacion == EvaluacionFrio.RECHAZADO && p.InicioFalla is null)
            throw new ReglaException("INICIO_FALLA_REQUERIDO", "Para rechazar una cámara indica desde qué hora estuvo fuera de temperatura (tras revisar su registro).", 400);
        if (p.Temperatura < -100 || p.Temperatura > 100 || decimal.Round(p.Temperatura, 2) != p.Temperatura)
            throw new ReglaException("TEMPERATURA_INVALIDA", "Registra una temperatura en °C entre -100 y 100, con hasta dos decimales.", 400);
        if (p.LoteId is not null && !actor.Tiene("inventario.administrar"))
            throw new ReglaException("SIN_PERMISO", "El control de lotes en almacén requiere permiso de inventario.", 403);
        if (p.LoteId is not null && p.EntregaId is not null)
            throw new ReglaException("REFERENCIA_FRIO_INVALIDA", "Indica un lote, una entrega o solo la cámara, no varios a la vez.", 400);
        if (p.EntregaId is not null)
        {
            var entrega = await db.Entregas.FindAsync(p.EntregaId) ?? throw new ReglaException("NO_ENCONTRADO", "Entrega no encontrada.", 404);
            if (entrega.ResponsableId != actor.Id && !actor.GestionGlobal && !actor.Principal.IsInRole("ADMINISTRADOR"))
                throw new ReglaException("SIN_PERMISO", "La entrega no está asignada a tu cuenta.", 403);
        }
        if (p.LoteId is not null && !await db.Lotes.AnyAsync(x => x.Id == p.LoteId))
            throw new ReglaException("NO_ENCONTRADO", "Lote no encontrado.", 404);
        var control = new ControlFrio
        {
            LoteId = p.LoteId,
            EntregaId = p.EntregaId,
            Temperatura = p.Temperatura,
            Evaluacion = p.Evaluacion,
            Ubicacion = p.Ubicacion.Trim(),
            Observacion = (p.Observacion?.Trim() ?? ""),
            Fecha = p.Fecha,
            InicioFalla = p.InicioFalla,
            ResponsableId = actor.Id
        };
        db.ControlesFrio.Add(control);
        if (p.Evaluacion == EvaluacionFrio.RECHAZADO)
        {
            if (camara)
                await BloquearCamara(control.Ubicacion, p.InicioFalla!.Value, p.Fecha, control.Observacion);
            else if (p.LoteId is Guid loteId)
                await BloquearLote(loteId, (p.Observacion?.Trim() ?? ""), null, p.InicioFalla);
            else
            {
                var entrega = (await db.Entregas.FindAsync(p.EntregaId))!;
                var existencias = await (from r in db.Reservas
                                         join d in db.DetallesPedido on r.DetallePedidoId equals d.Id
                                         join e in db.Existencias on r.ExistenciaId equals e.Id
                                         where d.PedidoId == entrega.PedidoId && r.Estado != EstadoReserva.LIBERADA
                                         select e.LoteId).Distinct().ToListAsync();
                foreach (var lote in existencias)
                    await BloquearLote(lote, (p.Observacion?.Trim() ?? ""), null, p.InicioFalla);
                entrega.Version++;
            }
        }
        auditoria.Registrar("REGISTRAR_CONTROL_FRIO", "ControlFrio", control.Id);
        return control.Id;
    });

    private static string Bolivia(DateTime utc) => utc.AddHours(-4).ToString("dd/MM/yyyy HH:mm");

    /// <summary>Falla de una cámara: entra en revisión (bloqueado) todo lote que estuvo en ella desde el inicio de la falla hasta
    /// la lectura: el que seguía con stock y el que tuvo movimientos después del inicio. Lo que salió antes queda libre.
    /// Los derivados se bloquean solo si consumieron después del inicio. La incidencia deja anotado lo despachado en ese lapso.</summary>
    private async Task BloquearCamara(string ubicacion, DateTime inicio, DateTime lectura, string observacion)
    {
        var candidatas = await db.Existencias.Where(e => e.Ubicacion == ubicacion)
            .Select(e => new
            {
                e.Id,
                e.LoteId,
                e.CantidadFisica,
                Ingreso = db.MovimientosStock.Where(m => m.ExistenciaId == e.Id && m.Tipo == "ENTRADA").Min(m => (DateTime?)m.Fecha),
                MovioDespues = db.MovimientosStock.Any(m => m.ExistenciaId == e.Id && m.Fecha >= inicio),
                Despachado = db.MovimientosStock.Where(m => m.ExistenciaId == e.Id && m.Tipo == "DESPACHO" && m.Fecha >= inicio)
                    .Sum(m => (decimal?)-m.Cantidad) ?? 0
            }).ToListAsync();
        var visitados = new HashSet<Guid>();
        foreach (var grupo in candidatas.Where(x => x.Ingreso != null && x.Ingreso <= lectura && (x.CantidadFisica > 0 || x.MovioDespues))
                     .GroupBy(x => x.LoteId))
        {
            var lote = await db.Lotes.FindAsync(grupo.Key);
            if (lote is null || lote.Estado == EstadoLote.BLOQUEADO)
                continue;
            var despachado = grupo.Sum(x => x.Despachado);
            var motivo = $"Falla de frío en {ubicacion} desde {Bolivia(inicio)} (lectura {Bolivia(lectura)}): {observacion}"
                + (despachado > 0 ? $" Atención: después del inicio de la falla se despacharon {despachado:0.###} {lote.Unidad} de este lote." : "");
            await BloquearLote(lote.Id, motivo.Length > 1000 ? motivo[..1000] : motivo, visitados, inicio);
        }
    }
}
