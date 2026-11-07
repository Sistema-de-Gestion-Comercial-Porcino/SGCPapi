using SGCP.Api.Comun;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Patrones.Produccion;

public abstract class FabricaResultado
{
    public abstract TipoOrdenProduccion Tipo
    {
        get;
    }
    public abstract (Lote Lote, ResultadoProduccion Resultado) Crear(
        OrdenProduccion orden, Producto producto, string codigo, DateTime vence, string origen);

    protected static (Lote Lote, ResultadoProduccion Resultado) CrearResultado(
        OrdenProduccion orden, Producto producto, string codigo, DateTime vence, string procedencia, TipoProducto familia)
    {
        if (producto.ClasificacionProducto.Familia != familia)
            throw new ReglaException("PRODUCTO_INCOMPATIBLE", "El tipo de producto no corresponde a esta producción.");
        var lote = new Lote
        {
            Codigo = codigo.Trim(),
            ProductoId = producto.Id,
            OrdenProduccionId = orden.Id,
            Unidad = producto.UnidadVenta,
            FechaProduccion = DateTime.UtcNow,
            Vencimiento = vence,
            Estado = EstadoLote.APTO,
            Procedencia = procedencia
        };
        return (lote, new ResultadoProduccion
        {
            OrdenProduccionId = orden.Id,
            ProductoId = producto.Id,
            LoteId = lote.Id,
            Estado = EstadoResultado.PENDIENTE_PESAJE,
            FechaRegistro = DateTime.UtcNow
        });
    }
}

public class FabricaFaeneo : FabricaResultado
{
    public override TipoOrdenProduccion Tipo => TipoOrdenProduccion.FAENEO;
    public override (Lote Lote, ResultadoProduccion Resultado) Crear(
        OrdenProduccion orden, Producto producto, string codigo, DateTime vence, string origen) =>
        CrearResultado(orden, producto, codigo, vence, origen, TipoProducto.CARNE_FRESCA);
}

public class FabricaDerivado : FabricaResultado
{
    public override TipoOrdenProduccion Tipo => TipoOrdenProduccion.FABRICACION;
    public override (Lote Lote, ResultadoProduccion Resultado) Crear(
        OrdenProduccion orden, Producto producto, string codigo, DateTime vence, string origen) =>
        CrearResultado(orden, producto, codigo, vence,
            $"Insumos consumidos de la orden {orden.Codigo}", TipoProducto.DERIVADO);
}

public class SelectorFabrica(IEnumerable<FabricaResultado> fabricas)
{
    private readonly IReadOnlyDictionary<TipoOrdenProduccion, FabricaResultado> disponibles = fabricas.ToDictionary(x => x.Tipo);
    public FabricaResultado Para(TipoOrdenProduccion tipo) => disponibles.TryGetValue(tipo, out var fabrica)
        ? fabrica : throw new ReglaException("TIPO_NO_IMPLEMENTADO", "Tipo de producción no disponible.", 400);
}
