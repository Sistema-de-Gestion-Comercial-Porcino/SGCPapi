using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Patrones.Produccion;
using SGCP.Api.Seguridad;

namespace SGCP.Api.Servicios;

public class ServicioProduccion(SgcpContext db, UnidadTrabajo unidad, ActorActual actor,
    ServicioAuditoria auditoria, ServicioInventario inventario, SelectorFabrica fabricas, GeneradorCodigo codigos)
{
    public Task<Guid> Animal(Guid? id, AnimalPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Cantidad(p.PesoIngreso);
        var x = id is null ? new Animal { Estado = EstadoAnimal.PENDIENTE, FechaIngreso = DateTime.UtcNow, Activo = true }
            : await db.Animales.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Animal no encontrado.", 404);
        if (id is not null && await db.ControlesSanitarios.AnyAsync(c => c.AnimalId == id))
            throw new ReglaException("ANIMAL_CON_HISTORIAL", "La identificación y procedencia se conservan tras el control sanitario.");
        x.Identificacion = p.Identificacion.Trim();
        x.Procedencia = p.Procedencia.Trim();
        x.Proveedor = p.Proveedor.Trim();
        x.PesoIngreso = p.PesoIngreso;
        if (id is null)
            db.Animales.Add(x);
        auditoria.Registrar(id is null ? "ALTA" : "EDITAR", "Animal", x.Id);
        return x.Id;
    });

    public Task<bool> CambiarEstadoAnimal(Guid id, bool activo) => unidad.Ejecutar(async () =>
    {
        var animal = await db.Animales.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Animal no encontrado.", 404);
        if (!activo && await db.OrdenesProduccion.AnyAsync(x => x.AnimalId == id &&
            (x.Estado == EstadoOrdenProduccion.PLANIFICADA || x.Estado == EstadoOrdenProduccion.EN_PROCESO)))
            throw new ReglaException("ANIMAL_EN_PRODUCCION", "Anula o concluye primero la orden pendiente.");
        animal.Activo = activo;
        auditoria.Registrar(activo ? "ACTIVAR" : "DESACTIVAR", "Animal", id);
        return true;
    });

    public Task<Guid> Sanidad(Guid id, SanidadPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.FechaReal(p.Fecha);
        var animal = await db.Animales.FindAsync(id) ?? throw new ReglaException("NO_ENCONTRADO", "Animal no encontrado.", 404);
        // Un animal rechazado o faenado ya no admite controles: ni un «Aprobado» que lo rehabilite ni un segundo rechazo
        // que duplique la incidencia.
        if (animal.Estado == EstadoAnimal.BLOQUEADO)
            throw new ReglaException("ANIMAL_NO_REHABILITABLE", p.Resultado == ResultadoSanitario.APROBADO
                ? "No se revierte un bloqueo ni un faeneo mediante otro control."
                : "El animal ya está rechazado: su incidencia está registrada y no se rechaza otra vez.");
        if (animal.Estado == EstadoAnimal.FAENADO)
            throw new ReglaException("ANIMAL_NO_REHABILITABLE", "No se revierte un bloqueo ni un faeneo mediante otro control.");
        if (await db.ControlesSanitarios.AnyAsync(x => x.AnimalId == id && x.Fecha >= p.Fecha))
            throw new ReglaException("CONTROL_FUERA_DE_ORDEN", "La fecha debe ser posterior al último control.");
        var observacion = p.Observacion?.Trim() ?? "";
        if (p.Resultado == ResultadoSanitario.RECHAZADO && observacion.Length < 3)
            throw new ReglaException("MOTIVO_REQUERIDO", animal.Estado == EstadoAnimal.APTO
                ? "El animal estaba apto: explica en la observación qué le pasó para rechazarlo ahora."
                : "Explica en la observación el motivo del rechazo.", 400);
        var estadoAnterior = animal.Estado;
        var c = new ControlSanitario { AnimalId = id, Resultado = p.Resultado, Fecha = p.Fecha, Observacion = observacion, ResponsableId = actor.Id };
        db.ControlesSanitarios.Add(c);
        animal.Estado = p.Resultado == ResultadoSanitario.APROBADO ? EstadoAnimal.APTO : EstadoAnimal.BLOQUEADO;
        if (p.Resultado == ResultadoSanitario.RECHAZADO)
        {
            var motivo = estadoAnterior == EstadoAnimal.APTO ? "Pasó de apto a rechazado: " + observacion : observacion;
            db.Incidencias.Add(new Incidencia { AnimalId = id, Motivo = motivo, Fecha = DateTime.UtcNow, ResponsableId = actor.Id });
            var ordenes = await db.OrdenesProduccion.Where(x => x.AnimalId == id).ToListAsync();
            foreach (var orden in ordenes)
            {
                if (orden.Estado is not (EstadoOrdenProduccion.CONFIRMADA or EstadoOrdenProduccion.ANULADA))
                    orden.Estado = EstadoOrdenProduccion.BLOQUEADA;
                foreach (var lote in await db.Lotes.Where(x => x.OrdenProduccionId == orden.Id).Select(x => x.Id).ToListAsync())
                    await inventario.BloquearLote(lote, observacion);
            }
        }
        auditoria.Registrar("CONTROL_SANITARIO", "Animal", id);
        return c.Id;
    });

    private async Task AnimalApto(Guid? id)
    {
        var animal = id is null ? null : await db.Animales.FindAsync(id);
        if (animal is null || !animal.Activo || animal.Estado != EstadoAnimal.APTO)
            throw new ReglaException("ANIMAL_RECHAZADO", "Se requiere un animal activo con control sanitario aprobado.");
    }

    public Task<Guid> Crear(OrdenPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Previstas(p.InicioPrevisto, p.FinPrevisto);
        if (p.Tipo == TipoOrdenProduccion.FAENEO)
            await AnimalApto(p.AnimalId);
        else if (p.AnimalId is not null)
            throw new ReglaException("ORDEN_INVALIDA", "La fabricación utiliza insumos, no un animal.", 400);
        var orden = new OrdenProduccion
        {
            Codigo = await codigos.SiguienteAsync("OP"),
            Tipo = p.Tipo,
            AnimalId = p.AnimalId,
            Estado = EstadoOrdenProduccion.PLANIFICADA,
            InicioPrevisto = p.InicioPrevisto,
            FinPrevisto = p.FinPrevisto,
            FechaCreacion = DateTime.UtcNow,
            ResponsableId = actor.Id,
            Observacion = (p.Observacion?.Trim() ?? "")
        };
        db.OrdenesProduccion.Add(orden);
        foreach (var prevision in p.Previsiones ?? [])
        {
            Validar.Cantidad(prevision.CantidadEstimada);
            var producto = await db.Productos.Include(x => x.ClasificacionProducto).SingleOrDefaultAsync(x => x.Id == prevision.ProductoId);
            var familia = p.Tipo == TipoOrdenProduccion.FAENEO ? TipoProducto.CARNE_FRESCA : TipoProducto.DERIVADO;
            if (producto is null || !producto.Activo || !producto.ClasificacionProducto.Activo ||
                producto.UnidadVenta != prevision.Unidad || producto.ClasificacionProducto.Familia != familia)
                throw new ReglaException("PREVISION_INVALIDA", "La previsión requiere producto activo, familia y unidad compatibles.");
            db.PrevisionesProducto.Add(new PrevisionProducto
            {
                OrdenProduccionId = orden.Id,
                ProductoId = producto.Id,
                CantidadEstimada = prevision.CantidadEstimada,
                Unidad = prevision.Unidad
            });
        }
        auditoria.Registrar("PROGRAMAR", "OrdenProduccion", orden.Id);
        return orden.Id;
    });

    public Task<bool> Iniciar(Guid id) => unidad.Ejecutar(async () =>
    {
        var orden = await Orden(id);
        if (orden.Estado != EstadoOrdenProduccion.PLANIFICADA)
            throw new ReglaException("TRANSICION_INVALIDA", "La orden debe estar planificada.");
        if (orden.Tipo == TipoOrdenProduccion.FAENEO)
            await AnimalApto(orden.AnimalId);
        orden.InicioReal = DateTime.UtcNow;
        orden.Estado = EstadoOrdenProduccion.EN_PROCESO;
        auditoria.Registrar("INICIAR", "OrdenProduccion", id);
        return true;
    });

    public Task<Guid> Consumir(Guid id, InsumoPeticion p) => unidad.Ejecutar(async () =>
    {
        Validar.Cantidad(p.Cantidad);
        if (p.ReferenciaUnica == Guid.Empty)
            throw new ReglaException("REFERENCIA_REQUERIDA", "Envía una referencia UUID del consumo y consérvala al reintentar.", 400);
        var anterior = await db.InsumosProduccion.SingleOrDefaultAsync(x => x.ReferenciaUnica == p.ReferenciaUnica);
        if (anterior is not null)
        {
            if (anterior.OrdenProduccionId != id || anterior.ExistenciaId != p.ExistenciaId || anterior.Cantidad != p.Cantidad)
                throw new ReglaException("CLAVE_REUTILIZADA", "La referencia pertenece a otro consumo.");
            return anterior.Id;
        }
        var orden = await Orden(id);
        if (orden.Tipo != TipoOrdenProduccion.FABRICACION || orden.Estado != EstadoOrdenProduccion.EN_PROCESO)
            throw new ReglaException("ORDEN_NO_CONSUMIBLE", "Solo una fabricación en proceso registra consumo real.");
        var existencia = await db.Existencias.FindAsync(p.ExistenciaId) ?? throw new ReglaException("SIN_EXISTENCIA_DISPONIBLE", "Insumo sin existencia física.");
        if (await inventario.Disponible(existencia) < p.Cantidad)
            throw new ReglaException("SIN_EXISTENCIA_DISPONIBLE", "Stock apto insuficiente para consumir.");
        var lote = (await db.Lotes.FindAsync(existencia.LoteId))!;
        var insumo = new InsumoProduccion { ReferenciaUnica = p.ReferenciaUnica, OrdenProduccionId = id, ExistenciaId = existencia.Id, Cantidad = p.Cantidad, Unidad = lote.Unidad, Fecha = DateTime.UtcNow, ResponsableId = actor.Id };
        db.InsumosProduccion.Add(insumo);
        inventario.Movimiento(existencia, -p.Cantidad, "CONSUMO", insumo.Id);
        auditoria.Registrar("CONSUMO_REAL", "InsumoProduccion", insumo.Id);
        return insumo.Id;
    });

    public Task<Guid[]> Confirmar(Guid id, ConfirmarProduccionPeticion p) => unidad.Ejecutar(async () =>
    {
        var orden = await Orden(id);
        if (orden.Estado != EstadoOrdenProduccion.EN_PROCESO)
            throw new ReglaException("ORDEN_NO_CONFIRMABLE", "La orden debe estar en proceso y sin bloqueos.");
        string origen = "";
        if (orden.Tipo == TipoOrdenProduccion.FAENEO)
        {
            await AnimalApto(orden.AnimalId);
            var animal = (await db.Animales.FindAsync(orden.AnimalId))!;
            origen = animal.Identificacion + " · " + animal.Procedencia;
            animal.Estado = EstadoAnimal.FAENADO;
        }
        else
        {
            var consumos = await db.InsumosProduccion.Where(x => x.OrdenProduccionId == id).ToListAsync();
            if (consumos.Count == 0)
                throw new ReglaException("SIN_CONSUMOS", "Registra los insumos realmente consumidos.");
            foreach (var consumo in consumos)
            {
                var existencia = (await db.Existencias.FindAsync(consumo.ExistenciaId))!;
                await inventario.LoteApto(existencia.LoteId);
            }
        }
        var ids = new List<Guid>();
        foreach (var r in p.Resultados)
        {
            Validar.Previstas(DateTime.UtcNow, r.Vencimiento);
            var producto = await db.Productos.Include(x => x.ClasificacionProducto).SingleOrDefaultAsync(x => x.Id == r.ProductoId && x.Activo && x.ClasificacionProducto.Activo)
                ?? throw new ReglaException("PRODUCTO_NO_ACTIVO", "Selecciona productos activos del catálogo.");
            var (lote, resultado) = fabricas.Para(orden.Tipo).Crear(orden, producto, r.CodigoLote, r.Vencimiento, origen);
            db.Lotes.Add(lote);
            db.ResultadosProduccion.Add(resultado);
            ids.Add(lote.Id);
        }
        orden.FinReal = DateTime.UtcNow;
        orden.Estado = EstadoOrdenProduccion.CONFIRMADA;
        auditoria.Registrar("CONFIRMAR_RESULTADOS_SIN_STOCK", "OrdenProduccion", id);
        return ids.ToArray();
    });

    public Task<bool> Anular(Guid id, string motivo) => unidad.Ejecutar(async () =>
    {
        Validar.Motivo(motivo);
        var orden = await Orden(id);
        if (orden.Estado == EstadoOrdenProduccion.CONFIRMADA || orden.Estado == EstadoOrdenProduccion.ANULADA)
            throw new ReglaException("TRANSICION_INVALIDA", "No se anulan órdenes confirmadas o ya anuladas.");
        // El consumo mezcla los insumos en el producto fabricado: no se separan ni se devuelven a su existencia.
        if (await db.InsumosProduccion.AnyAsync(x => x.OrdenProduccionId == id))
            throw new ReglaException("ORDEN_CON_CONSUMOS", "La orden ya consumió insumos: el consumo es irreversible y la orden no se anula. Confirma el resultado con lo realmente obtenido y pésalo en almacén.");
        orden.Estado = EstadoOrdenProduccion.ANULADA;
        orden.Observacion = motivo.Trim();
        auditoria.Registrar("ANULAR", "OrdenProduccion", id);
        return true;
    });

    private async Task<OrdenProduccion> Orden(Guid id) => await db.OrdenesProduccion.FindAsync(id)
        ?? throw new ReglaException("NO_ENCONTRADO", "Orden no encontrada.", 404);
}
