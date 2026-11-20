using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Patrones.Comercial;
using SGCP.Api.Seguridad;

namespace SGCP.Api.Servicios;

public record SaldoCargo(Guid CargoId, Guid PedidoId, Guid ClienteId, TipoCargo Tipo, string Concepto,
    decimal Importe, decimal Aplicado, decimal Saldo, string Condicion, DateTime? Vencimiento);

public class ServicioFinanzas(SgcpContext db, UnidadTrabajo unidad, ActorActual actor,
    ServicioAuditoria auditoria, SelectorCondicion estrategias)
{
    public async Task<List<SaldoCargo>> Saldos(Guid? clienteId = null, Guid? pedidoId = null)
    {
        if (actor.Comprador)
        {
            if (clienteId is not null)
                actor.VerCliente(clienteId.Value);
            clienteId = actor.ClienteId ?? throw new ReglaException("SIN_CLIENTE", "Cuenta sin cliente asociado.", 403);
        }
        var filas = await (from c in db.Cargos.AsNoTracking()
                           join p in db.Pedidos on c.PedidoId equals p.Id
                           where !c.Anulado && (clienteId == null || p.ClienteId == clienteId) && (pedidoId == null || p.Id == pedidoId)
                           select new
                           {
                               Cargo = c,
                               p.ClienteId
                           }).ToListAsync();
        var ids = filas.Select(x => x.Cargo.Id).ToArray();
        var pedidos = filas.Select(x => x.Cargo.PedidoId).Distinct().ToArray();
        var pagos = await (from a in db.AplicacionesPago.AsNoTracking()
                           join p in db.Pagos on a.PagoId equals p.Id
                           where ids.Contains(a.CargoId) && !p.Anulado
                           select a).ToListAsync();
        var condiciones = await db.CondicionesPago.AsNoTracking().Where(x => pedidos.Contains(x.PedidoId) && x.Activa).ToDictionaryAsync(x => x.PedidoId);
        return filas.Select(x =>
        {
            var aplicado = pagos.Where(p => p.CargoId == x.Cargo.Id).Sum(p => p.Importe);
            condiciones.TryGetValue(x.Cargo.PedidoId, out var condicion);
            var estado = condicion is null ? "SIN_ACUERDO" : condicion.Modalidad != FormaPago.CREDITO_ACORDADO ? "CONTADO" :
                condicion.Vencimiento <= DateTime.UtcNow ? "CREDITO_VENCIDO" : "CREDITO_VIGENTE";
            return new SaldoCargo(x.Cargo.Id, x.Cargo.PedidoId, x.ClienteId, x.Cargo.Tipo,
                x.Cargo.Concepto, x.Cargo.Importe, aplicado, x.Cargo.Importe - aplicado, estado, condicion?.Vencimiento);
        }).ToList();
    }

    public Task<Guid> CargoServicio(CargoPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Dinero(p.Importe);
        await PedidoVigente(p.PedidoId);
        Validar.FechaReal(p.PrestadoEn);
        var c = new Cargo { PedidoId = p.PedidoId, Tipo = TipoCargo.SERVICIO, AcuerdoServicio = p.AcuerdoServicio.Trim(), PrestadoEn = p.PrestadoEn, Concepto = p.Concepto.Trim(), Importe = p.Importe, Fecha = DateTime.UtcNow };
        db.Cargos.Add(c);
        auditoria.Registrar("CARGO_SERVICIO", "Cargo", c.Id);
        return c.Id;
    });

    public Task<bool> AnularCargo(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        var cargo = await db.Cargos.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Cargo no encontrado.", 404);
        if (cargo.Anulado)
            throw new ReglaException("CARGO_ANULADO", "El cargo ya está anulado.");
        if (cargo.Tipo == TipoCargo.PRODUCTO && await db.Pedidos.AnyAsync(x => x.Id == cargo.PedidoId && x.EstadoComercial == EstadoComercial.CONFIRMADO))
            throw new ReglaException("CARGO_HISTORICO", "No se anula el producto de un pedido confirmado; resuelve primero el pedido.");
        if (await db.AplicacionesPago.AnyAsync(x => x.CargoId == id && db.Pagos.Any(p => p.Id == x.PagoId && !p.Anulado)))
            throw new ReglaException("CARGO_CON_PAGOS", "El cargo tiene pagos aplicados; requiere resolución comercial.");
        cargo.Anulado = true;
        cargo.MotivoAnulacion = motivo.Trim();
        auditoria.Registrar("ANULAR_CARGO", "Cargo", id);
        return true;
    });

    public Task<Guid> Condicion(Guid id, CondicionPeticion p) => unidad.Ejecutar(async () =>
    {
        var pedido = await PedidoVigente(id);
        if (await db.Entregas.AnyAsync(x => x.PedidoId == id && x.Estado != EstadoEntrega.PREPARADA))
            throw new ReglaException("CONDICION_HISTORICA", "No se sustituye el acuerdo de un pedido ya despachado.");
        if (p.Modalidad == FormaPago.CREDITO_ACORDADO)
        {
            Validar.Dinero(p.ImporteAutorizado);
            if (p.Vencimiento is null)
                throw new ReglaException("VENCIMIENTO_REQUERIDO", "Acuerda el vencimiento, sin plazo por defecto.", 400);
            Validar.Previstas(DateTime.UtcNow, p.Vencimiento.Value);
        }
        else if (p.ImporteAutorizado != 0 || p.Vencimiento is not null)
            throw new ReglaException("CONDICION_INVALIDA", "Contado no lleva importe de crédito ni vencimiento.", 400);
        var anterior = await db.CondicionesPago.SingleOrDefaultAsync(x => x.PedidoId == id && x.Activa);
        if (anterior is not null)
        {
            anterior.Activa = false;
            await db.SaveChangesAsync();
        }
        var c = new CondicionPago
        {
            PedidoId = id,
            Modalidad = p.Modalidad,
            ImporteAutorizado = p.ImporteAutorizado,
            FechaAcuerdo = DateTime.UtcNow,
            Vencimiento = p.Vencimiento,
            ResponsableId = actor.Id,
            Activa = true
        };
        pedido.CondicionPago = p.Modalidad;
        db.CondicionesPago.Add(c);
        auditoria.Registrar("ACORDAR_CONDICION_SIN_COBRAR", "CondicionPago", c.Id);
        return c.Id;
    });

    public async Task AutorizarDespacho(Guid id)
    {
        var condicion = await db.CondicionesPago.SingleOrDefaultAsync(x => x.PedidoId == id && x.Activa)
            ?? throw new ReglaException("CONDICION_PENDIENTE", "Registra primero la condición acordada de esta operación.");
        var saldo = (await Saldos(pedidoId: id)).Sum(x => x.Saldo);
        estrategias.Para(condicion.Modalidad).Validar(condicion, saldo);
    }

    public Task<Guid> Pago(PagoPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Dinero(p.Importe);
        Validar.FechaReal(p.Fecha);
        if (!await db.Clientes.AnyAsync(x => x.Id == p.ClienteId))
            throw new ReglaException("NO_ENCONTRADO", "Cliente no encontrado.", 404);
        if (p.Aplicaciones.Sum(x => x.Importe) != p.Importe || p.Aplicaciones.Select(x => x.CargoId).Distinct().Count() != p.Aplicaciones.Count)
            throw new ReglaException("APLICACIONES_INVALIDAS", "Distribuye exactamente el pago entre cargos distintos.", 400);
        var saldos = await Saldos(p.ClienteId);
        var pago = new Pago
        {
            ClienteId = p.ClienteId,
            ReferenciaUnica = p.ReferenciaUnica.Trim(),
            Importe = p.Importe,
            Fecha = p.Fecha,
            Medio = p.Medio.Trim(),
            Tipo = TipoPago.PAGO,
            ResponsableId = actor.Id
        };
        foreach (var a in p.Aplicaciones)
        {
            Validar.Dinero(a.Importe);
            var cargo = saldos.SingleOrDefault(x => x.CargoId == a.CargoId);
            if (cargo is null || cargo.Saldo < a.Importe)
                throw new ReglaException("APLICACION_NO_VALIDA", "El cargo debe ser del cliente y tener saldo suficiente.");
            db.AplicacionesPago.Add(new AplicacionPago { PagoId = pago.Id, CargoId = a.CargoId, Importe = a.Importe, Fecha = DateTime.UtcNow });
        }
        db.Pagos.Add(pago);
        auditoria.Registrar("REGISTRAR_PAGO", "Pago", pago.Id);
        return pago.Id;
    });

    public Task<Guid> Sena(SenaPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Dinero(p.Importe);
        Validar.FechaReal(p.Fecha);
        var pedido = await PedidoVigente(p.PedidoId);
        var pago = new Pago
        {
            ClienteId = pedido.ClienteId,
            ReferenciaUnica = p.ReferenciaUnica.Trim(),
            Importe = p.Importe,
            Fecha = p.Fecha,
            Medio = p.Medio.Trim(),
            Tipo = TipoPago.SENA,
            ResponsableId = actor.Id
        };
        var sena = new Sena { PedidoId = pedido.Id, PagoId = pago.Id };
        db.Pagos.Add(pago);
        db.Senas.Add(sena);
        auditoria.Registrar("RECIBIR_SENA", "Sena", sena.Id);
        return sena.Id;
    });

    public Task<bool> AplicarSena(Guid id, Guid cargoId) => unidad.Ejecutar(async () =>
    {
        var sena = await db.Senas.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Seña no encontrada.", 404);
        if (sena.ResueltaEn is not null || await db.AplicacionesPago.AnyAsync(x => x.SenaId == id))
            throw new ReglaException("SENA_YA_RESUELTA", "La seña ya se aplicó o tiene un acuerdo de resolución.");
        var pago = (await db.Pagos.FindAsync(sena.PagoId))!;
        if (pago.Anulado)
            throw new ReglaException("PAGO_ANULADO", "El pago está anulado.");
        var cargo = (await Saldos(pedidoId: sena.PedidoId)).SingleOrDefault(x => x.CargoId == cargoId);
        if (cargo is null || cargo.Saldo < pago.Importe)
            throw new ReglaException("APLICACION_NO_VALIDA", "El cargo del pedido debe cubrir la seña completa.");
        db.AplicacionesPago.Add(new AplicacionPago { SenaId = id, PagoId = pago.Id, CargoId = cargoId, Importe = pago.Importe, Fecha = DateTime.UtcNow });
        auditoria.Registrar("APLICAR_SENA", "Sena", id);
        return true;
    });

    public Task<bool> ResolverSena(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        var sena = await db.Senas.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Seña no encontrada.", 404);
        if (sena.ResueltaEn is not null)
            throw new ReglaException("SENA_YA_RESUELTA", "La seña ya tiene un acuerdo de resolución.");
        sena.ResueltaEn = DateTime.UtcNow;
        sena.AcuerdoResolucion = motivo.Trim();
        auditoria.Registrar("ACUERDO_SENA_SIN_DEVOLUCION_AUTOMATICA", "Sena", id);
        return true;
    });

    public Task<bool> AnularPago(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        var pago = await db.Pagos.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Pago no encontrado.", 404);
        if (pago.Tipo == TipoPago.SENA)
            throw new ReglaException("SENA_REQUIERE_ACUERDO", "Una seña requiere acuerdo explícito; no se anula automáticamente.");
        if (pago.Anulado)
            throw new ReglaException("PAGO_ANULADO", "El pago ya está anulado.");
        pago.Anulado = true;
        pago.MotivoAnulacion = motivo.Trim();
        auditoria.Registrar("ANULAR_PAGO", "Pago", id);
        return true;
    });

    public Task<Guid> CrearSeguimiento(Guid condicionId, DateTime proximaFecha) => unidad.Ejecutar(async () =>
    {
        var condicion = await db.CondicionesPago.FindAsync(condicionId)
            ?? throw new ReglaException("NO_ENCONTRADO", "Condición de pago no encontrada.", 404);
        if (condicion.Modalidad != FormaPago.CREDITO_ACORDADO)
            throw new ReglaException("CONDICION_DE_CONTADO", "El seguimiento solo nace si la condición de pago es a crédito.", 400);
        var saldo = await SaldoPedido(condicion.PedidoId);
        if (saldo <= 0)
            throw new ReglaException("SIN_SALDO", "No hay saldo pendiente para abrir el seguimiento.");
        var seguimiento = new SeguimientoCobro
        {
            CondicionPagoId = condicion.Id,
            ProximaFecha = Consultas.Utc(proximaFecha),
            Estado = EstadoSeguimiento.PENDIENTE
        };
        db.SeguimientosCobro.Add(seguimiento);
        auditoria.Registrar("CREAR_SEGUIMIENTO_COBRO", "SeguimientoCobro", seguimiento.Id);
        return seguimiento.Id;
    });

    public Task<EstadoSeguimiento> RegistrarGestion(Guid id, string resultado, DateTime? proximaFecha) => unidad.Ejecutar(async () =>
    {
        if (string.IsNullOrWhiteSpace(resultado))
            throw new ReglaException("DATOS_INVALIDOS", "Indica el resultado de la gestión.", 400);
        var seguimiento = await db.SeguimientosCobro.FindAsync(id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Seguimiento no encontrado.", 404);
        if (seguimiento.Estado == EstadoSeguimiento.CERRADO)
            throw new ReglaException("SEGUIMIENTO_CERRADO", "Un seguimiento cerrado no admite modificaciones.", 400);
        if (seguimiento.Estado != EstadoSeguimiento.PENDIENTE)
            throw new ReglaException("TRANSICION_INVALIDA", "Solo un seguimiento pendiente admite una gestión.", 400);
        seguimiento.Estado = EstadoSeguimiento.REALIZADO;
        seguimiento.FechaGestion = DateTime.UtcNow;
        seguimiento.Resultado = resultado.Trim();
        var condicion = (await db.CondicionesPago.FindAsync(seguimiento.CondicionPagoId))!;
        var saldo = await SaldoPedido(condicion.PedidoId);
        if (saldo <= 0)
            seguimiento.Estado = EstadoSeguimiento.CERRADO;
        else
        {
            if (proximaFecha is null)
                throw new ReglaException("PROXIMA_FECHA_REQUERIDA", "Con saldo pendiente indica la próxima fecha de gestión.", 400);
            seguimiento.ProximaFecha = Consultas.Utc(proximaFecha.Value);
            seguimiento.Estado = EstadoSeguimiento.PENDIENTE;
        }
        auditoria.Registrar("REGISTRAR_GESTION_COBRO", "SeguimientoCobro", id);
        return seguimiento.Estado;
    });

    public Task<bool> CerrarSeguimiento(Guid id) => unidad.Ejecutar(async () =>
    {
        var seguimiento = await db.SeguimientosCobro.FindAsync(id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Seguimiento no encontrado.", 404);
        if (seguimiento.Estado == EstadoSeguimiento.CERRADO)
            throw new ReglaException("SEGUIMIENTO_CERRADO", "Un seguimiento cerrado no admite modificaciones.", 400);
        var condicion = (await db.CondicionesPago.FindAsync(seguimiento.CondicionPagoId))!;
        if (await SaldoPedido(condicion.PedidoId) > 0)
            throw new ReglaException("SALDO_PENDIENTE", "No se cierra el seguimiento mientras el pedido conserva saldo.", 400);
        seguimiento.Estado = EstadoSeguimiento.CERRADO;
        auditoria.Registrar("CERRAR_SEGUIMIENTO_COBRO", "SeguimientoCobro", id);
        return true;
    });

    private async Task<decimal> SaldoPedido(Guid pedidoId) => (await Saldos(pedidoId: pedidoId)).Sum(x => x.Saldo);

    private async Task<Pedido> PedidoVigente(Guid id)
    {
        var pedido = await db.Pedidos.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Pedido no encontrado.", 404);
        if (pedido.EstadoComercial != EstadoComercial.CONFIRMADO)
            throw new ReglaException("PEDIDO_NO_VIGENTE", "El pedido no está confirmado.");
        return pedido;
    }
}
