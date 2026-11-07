using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;

namespace SGCP.Api.Servicios;

public record ResultadoSincronizacion(Guid ClaveUnica, Guid RecepcionId, EstadoRecepcion Estado, bool Duplicada, DateTime FechaSincronizacion);

public class ServicioEntregas(SgcpContext db, UnidadTrabajo unidad, ActorActual actor,
    ServicioAuditoria auditoria, ServicioFinanzas finanzas, ServicioInventario inventario, GeneradorCodigo codigos)
{
    public Task<Guid> Transporte(Guid? id, TransportePeticion p) => unidad.Ejecutar(async () =>
    {
        var t = id is null ? new Transporte { Activo = true } : await db.Transportes.FindAsync(id)
            ?? throw new ReglaException("NO_ENCONTRADO", "Transporte no encontrado.", 404);
        if (id is not null && await db.Rutas.AnyAsync(x => x.TransporteId == id && x.Estado == EstadoRuta.EN_REPARTO))
            throw new ReglaException("TRANSPORTE_EN_USO", "No se cambia un transporte en reparto.");
        t.Codigo = p.Codigo.Trim();
        t.Modalidad = p.Modalidad;
        t.Transportista = p.Transportista.Trim();
        t.Contacto = p.Contacto.Trim();
        t.Placa = p.Placa.Trim();
        t.DescripcionVehiculo = p.DescripcionVehiculo.Trim();
        t.Conservacion = p.Conservacion.Trim();
        if (id is null)
            db.Transportes.Add(t);
        auditoria.Registrar("GUARDAR_TRANSPORTE", "Transporte", t.Id);
        return t.Id;
    });

    public Task<bool> EstadoTransporte(Guid id, bool activo) => unidad.Ejecutar(async () =>
    {
        var t = await db.Transportes.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Transporte no encontrado.", 404);
        if (!activo && await db.Rutas.AnyAsync(x => x.TransporteId == id && x.Estado != EstadoRuta.FINALIZADA))
            throw new ReglaException("TRANSPORTE_EN_USO", "El transporte tiene rutas pendientes.");
        t.Activo = activo;
        auditoria.Registrar("ESTADO_TRANSPORTE", "Transporte", id);
        return true;
    });

    private async Task Responsable(Guid id)
    {
        if (!await db.Usuarios.AnyAsync(x => x.Id == id && x.Estado == EstadoUsuario.ACTIVO &&
                x.Roles.Any(r => r.Nombre == "ENTREGA" || r.Nombre == "ADMINISTRACION")))
            throw new ReglaException("RESPONSABLE_INVALIDO", "Asigna una cuenta activa responsable de entrega.");
    }

    public Task<Guid> CrearRuta(RutaPeticion p) => unidad.Ejecutar(async () =>
    {
        await Responsable(p.ResponsableId);
        if (!await db.Transportes.AnyAsync(x => x.Id == p.TransporteId && x.Activo))
            throw new ReglaException("TRANSPORTE_INACTIVO", "Selecciona un transporte activo.");
        if (p.Fecha.Kind != DateTimeKind.Utc || p.Fecha.Year < 2020)
            throw new ReglaException("FECHA_INVALIDA", "Indica fecha UTC.", 400);
        var ruta = new Ruta
        {
            Codigo = await codigos.SiguienteAsync("RUT"),
            TransporteId = p.TransporteId,
            ResponsableId = p.ResponsableId,
            Fecha = p.Fecha,
            Zona = p.Zona.Trim(),
            Estado = EstadoRuta.PLANIFICADA
        };
        db.Rutas.Add(ruta);
        auditoria.Registrar("CREAR_RUTA", "Ruta", ruta.Id);
        return ruta.Id;
    });

    public Task<Guid> Preparar(EntregaPeticion p) => unidad.Ejecutar(async () =>
    {
        var pedido = await db.Pedidos.Include(x => x.Detalles).SingleOrDefaultAsync(x => x.Id == p.PedidoId)
            ?? throw new ReglaException("NO_ENCONTRADO", "Pedido no encontrado.", 404);
        if (pedido.EstadoComercial != EstadoComercial.CONFIRMADO)
            throw new ReglaException("PEDIDO_NO_VIGENTE", "El pedido no está confirmado.");
        await Responsable(p.ResponsableId);
        await ValidarRuta(p.RutaId, p.ResponsableId, p.Modalidad);
        if (p.FechaPrevista.Kind != DateTimeKind.Utc || p.FechaPrevista.Year < 2020 || p.OrdenEnRuta < 1)
            throw new ReglaException("ENTREGA_INVALIDA", "Indica fecha UTC y posición positiva.", 400);
        await ReservasCompletas(pedido);
        var entrega = new Entrega
        {
            PedidoId = pedido.Id,
            RutaId = p.RutaId,
            ResponsableId = p.ResponsableId,
            Modalidad = p.Modalidad,
            Estado = EstadoEntrega.PREPARADA,
            Version = 1,
            FechaPrevista = p.FechaPrevista,
            OrdenEnRuta = p.OrdenEnRuta
        };
        pedido.EstadoPreparacion = EstadoPreparacion.PREPARADO;
        foreach (var detalle in pedido.Detalles)
            detalle.Estado = EstadoDetalle.PREPARADO;
        db.Entregas.Add(entrega);
        auditoria.Registrar("PREPARAR_ENTREGA", "Entrega", entrega.Id);
        return entrega.Id;
    });

    private async Task ValidarRuta(Guid? rutaId, Guid responsable, ModalidadEntrega modalidad)
    {
        if (modalidad == ModalidadEntrega.DOMICILIO && rutaId is null)
            throw new ReglaException("RUTA_REQUERIDA", "La entrega a domicilio requiere ruta.");
        if (rutaId is not null)
        {
            var ruta = await db.Rutas.FindAsync(rutaId);
            if (ruta is null || ruta.ResponsableId != responsable || ruta.Estado != EstadoRuta.PLANIFICADA)
                throw new ReglaException("RUTA_INVALIDA", "La ruta debe estar planificada y corresponder al responsable.");
        }
    }

    private async Task<List<Reserva>> ReservasCompletas(Pedido pedido)
    {
        var ids = pedido.Detalles.Select(x => x.Id).ToArray();
        var reservas = await db.Reservas.Where(x => ids.Contains(x.DetallePedidoId) && x.Estado == EstadoReserva.ACTIVA).ToListAsync();
        foreach (var detalle in pedido.Detalles)
            if (reservas.Where(x => x.DetallePedidoId == detalle.Id).Sum(x => x.Cantidad) != detalle.CantidadConfirmada)
                throw new ReglaException("PEDIDO_INCOMPLETO", "Reserva toda la cantidad confirmada de cada producto antes de preparar o despachar.");
        foreach (var r in reservas)
        {
            var existencia = (await db.Existencias.FindAsync(r.ExistenciaId))!;
            await inventario.LoteApto(existencia.LoteId);
            if (existencia.CantidadFisica < r.Cantidad)
                throw new ReglaException("SIN_EXISTENCIA_DISPONIBLE", "Stock físico insuficiente.");
        }
        return reservas;
    }

    public Task<bool> Despachar(Guid id) => unidad.Ejecutar(async () =>
    {
        var entrega = await Obtener(id);
        VerAsignacion(entrega);
        if (entrega.Estado != EstadoEntrega.PREPARADA)
            throw new ReglaException("ENTREGA_NO_PREPARADA", "Solo se despacha una entrega preparada.");
        var pedido = await db.Pedidos.Include(x => x.Detalles).SingleAsync(x => x.Id == entrega.PedidoId);
        if (pedido.EstadoComercial != EstadoComercial.CONFIRMADO)
            throw new ReglaException("PEDIDO_NO_VIGENTE", "El pedido ya no está vigente.");
        var reservas = await ReservasCompletas(pedido);
        await finanzas.AutorizarDespacho(pedido.Id);
        foreach (var r in reservas)
        {
            var existencia = (await db.Existencias.FindAsync(r.ExistenciaId))!;
            inventario.Movimiento(existencia, -r.Cantidad, "DESPACHO", r.Id);
            r.Estado = EstadoReserva.CONSUMIDA;
        }
        entrega.Estado = EstadoEntrega.DESPACHADA;
        entrega.Version++;
        if (entrega.RutaId is Guid rutaId)
        {
            var ruta = (await db.Rutas.FindAsync(rutaId))!;
            if (!await db.Transportes.AnyAsync(x => x.Id == ruta.TransporteId && x.Activo))
                throw new ReglaException("TRANSPORTE_INACTIVO", "El transporte fue desactivado.");
            ruta.Estado = EstadoRuta.EN_REPARTO;
            ruta.SalidaReal ??= DateTime.UtcNow;
        }
        auditoria.Registrar("DESPACHAR", "Entrega", id);
        return true;
    });

    public Task<bool> Reasignar(Guid id, ReasignarPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(p.Motivo);
        await Responsable(p.ResponsableId);
        var entrega = await Obtener(id);
        if (entrega.Version != p.VersionActual || entrega.Estado is EstadoEntrega.ENTREGADA or EstadoEntrega.OBSERVADA or EstadoEntrega.CANCELADA)
            throw new ReglaException("VERSION_CONFLICTO", "Actualiza la entrega; no se reasigna una recepción finalizada.");
        await ValidarRuta(p.RutaId, p.ResponsableId, entrega.Modalidad);
        entrega.ResponsableId = p.ResponsableId;
        entrega.RutaId = p.RutaId;
        entrega.Version++;
        auditoria.Registrar("REASIGNAR: " + p.Motivo.Trim(), "Entrega", id);
        return true;
    });

    public Task<List<ResultadoSincronizacion>> Sincronizar(SincronizarPeticion peticion) => unidad.Ejecutar(async () =>
    {
        var resultados = new List<ResultadoSincronizacion>();
        foreach (var p in peticion.Recepciones)
        {
            if (p.ClaveUnica == Guid.Empty || p.AutorId != actor.Id)
                throw new ReglaException("AUTOR_INVALIDO", "La captura debe pertenecer al usuario autenticado y tener una clave única.", 403);
            var huella = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(p)));
            var anterior = db.Recepciones.Local.FirstOrDefault(x => x.ClaveUnica == p.ClaveUnica)
                ?? await db.Recepciones.SingleOrDefaultAsync(x => x.ClaveUnica == p.ClaveUnica);
            if (anterior is not null)
            {
                if (anterior.AutorId != actor.Id)
                    throw new ReglaException("SIN_PERMISO", "La captura pertenece a otra cuenta.", 403);
                if (anterior.HuellaContenido != huella)
                    throw new ReglaException("CLAVE_REUTILIZADA", "La clave ya corresponde a otro contenido; conserva la captura original.");
                resultados.Add(new(p.ClaveUnica, anterior.Id, anterior.Estado, true, anterior.FechaSincronizacion));
                continue;
            }
            Validar.FechaReal(p.FechaReal);
            Validar.Cantidad(p.PesoRecibido);
            if (p.Unidad is not "kg" and not "g")
                throw new ReglaException("UNIDAD_PESO_INVALIDA", "El peso de recepción se registra en kg o g.", 400);
            if (!p.Conforme)
                Validar.Motivo(p.Observacion);
            var entrega = await Obtener(p.EntregaId);
            VerAsignacion(entrega, estricto: true);
            var conflicto = entrega.Version != p.VersionBase || entrega.Estado != EstadoEntrega.DESPACHADA;
            var r = new Recepcion
            {
                EntregaId = entrega.Id,
                ClaveUnica = p.ClaveUnica,
                VersionBase = p.VersionBase,
                AutorId = actor.Id,
                FechaReal = p.FechaReal,
                PesoRecibido = p.PesoRecibido,
                Unidad = p.Unidad,
                FechaSincronizacion = DateTime.UtcNow,
                Conforme = p.Conforme,
                Receptor = p.Receptor.Trim(),
                Observacion = p.Observacion?.Trim() ?? "",
                HuellaContenido = huella,
                Estado = conflicto ? EstadoRecepcion.CONFLICTO : EstadoRecepcion.CONFIRMADA,
                MotivoConflicto = conflicto ? "La versión o el estado del servidor cambiaron. Requiere revisión humana." : ""
            };
            foreach (var e in p.Evidencias)
            {
                byte[] contenido;
                try
                {
                    contenido = Convert.FromBase64String(e.ContenidoBase64);
                }
                catch (FormatException) { throw new ReglaException("EVIDENCIA_INVALIDA", "La imagen no está codificada en base64 válido.", 400); }
                var png = contenido.Length > 8 && contenido.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
                var jpg = contenido.Length > 4 && contenido[0] == 255 && contenido[1] == 216 && contenido[2] == 255;
                if (contenido.Length > 524288 || !((e.Tipo == "image/png" && png) || (e.Tipo == "image/jpeg" && jpg)))
                    throw new ReglaException("EVIDENCIA_INVALIDA", "Adjunta imágenes JPEG o PNG de hasta 512 KiB.", 400);
                try
                {
                    using var archivo = new MemoryStream(contenido);
                    using var imagen = PdfSharp.Drawing.XImage.FromStream(archivo);
                    if (imagen.PixelWidth > 8000 || imagen.PixelHeight > 8000)
                        throw new ReglaException("EVIDENCIA_INVALIDA", "Reduce la imagen a un máximo de 8000 píxeles por lado.", 400);
                }
                catch (ReglaException) { throw; }
                catch (Exception) { throw new ReglaException("EVIDENCIA_INVALIDA", "La imagen está dañada o no es compatible.", 400); }
                db.EvidenciasRecepcion.Add(new EvidenciaRecepcion
                {
                    RecepcionId = r.Id,
                    Tipo = e.Tipo,
                    Contenido = contenido,
                    Sha256 = Convert.ToHexString(SHA256.HashData(contenido))
                });
            }
            db.Recepciones.Add(r);
            if (!conflicto)
                await Aplicar(entrega, r);
            auditoria.Registrar(conflicto ? "RECEPCION_EN_CONFLICTO" : "CONFIRMAR_RECEPCION", "Recepcion", r.Id);
            resultados.Add(new(p.ClaveUnica, r.Id, r.Estado, false, r.FechaSincronizacion));
        }
        return resultados;
    });

    private async Task Aplicar(Entrega entrega, Recepcion recepcion)
    {
        recepcion.SaldoPendienteAlConfirmar = (await finanzas.Saldos(pedidoId: entrega.PedidoId)).Sum(x => x.Saldo);
        entrega.Estado = recepcion.Conforme ? EstadoEntrega.ENTREGADA : EstadoEntrega.OBSERVADA;
        entrega.FechaReal = recepcion.FechaReal;
        entrega.FechaConfirmacion = DateTime.UtcNow;
        entrega.Receptor = recepcion.Receptor;
        entrega.ObservacionRecepcion = recepcion.Observacion;
        entrega.Version++;
    }

    public Task<bool> Revisar(Guid id, RevisarRecepcionPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(p.Motivo);
        var r = await db.Recepciones.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Recepción no encontrada.", 404);
        if (r.Estado != EstadoRecepcion.CONFLICTO)
            throw new ReglaException("RECEPCION_NO_PENDIENTE", "Solo se revisan capturas en conflicto.");
        var entrega = await Obtener(r.EntregaId);
        if (entrega.Version != p.VersionActual)
            throw new ReglaException("VERSION_CONFLICTO", "La entrega cambió; vuelve a revisarla.");
        if (p.Aplicar)
        {
            if (entrega.Estado != EstadoEntrega.DESPACHADA || await db.Recepciones.AnyAsync(x => x.EntregaId == entrega.Id && x.Estado == EstadoRecepcion.CONFIRMADA))
                throw new ReglaException("RECEPCION_NO_APLICABLE", "La entrega ya tiene recepción o no está despachada.");
            await Aplicar(entrega, r);
            r.Estado = EstadoRecepcion.CONFIRMADA;
        }
        else
            r.Estado = EstadoRecepcion.DESCARTADA;
        r.RevisadaPorId = actor.Id;
        r.RevisadaEn = DateTime.UtcNow;
        r.Resolucion = p.Motivo.Trim();
        auditoria.Registrar("REVISAR_RECEPCION", "Recepcion", id);
        return true;
    });

    public Task<bool> FinalizarRuta(Guid id) => unidad.Ejecutar(async () =>
    {
        var ruta = await db.Rutas.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Ruta no encontrada.", 404);
        if (ruta.ResponsableId != actor.Id && !actor.Principal.IsInRole("ADMINISTRACION"))
            throw new ReglaException("SIN_PERMISO", "Ruta de otro responsable.", 403);
        if (ruta.Estado == EstadoRuta.FINALIZADA || !await db.Entregas.AnyAsync(x => x.RutaId == id)
            || await db.Entregas.AnyAsync(x => x.RutaId == id && x.Estado != EstadoEntrega.ENTREGADA && x.Estado != EstadoEntrega.OBSERVADA && x.Estado != EstadoEntrega.CANCELADA)
            || await (from r in db.Recepciones join e in db.Entregas on r.EntregaId equals e.Id where e.RutaId == id && r.Estado == EstadoRecepcion.CONFLICTO select r.Id).AnyAsync())
            throw new ReglaException("RUTA_PENDIENTE", "La ruta requiere recepciones sincronizadas y conflictos resueltos.");
        ruta.Estado = EstadoRuta.FINALIZADA;
        ruta.RetornoReal = DateTime.UtcNow;
        auditoria.Registrar("FINALIZAR_RUTA", "Ruta", id);
        return true;
    });

    public async Task<Entrega> Obtener(Guid id) => await db.Entregas.FindAsync(id)
        ?? throw new ReglaException("NO_ENCONTRADO", "Entrega no encontrada.", 404);

    public void VerAsignacion(Entrega entrega, bool estricto = false)
    {
        if (entrega.ResponsableId != actor.Id && (estricto || !actor.Principal.IsInRole("ADMINISTRACION")))
            throw new ReglaException("SIN_PERMISO", "La entrega no está asignada a tu cuenta.", 403);
    }
}
