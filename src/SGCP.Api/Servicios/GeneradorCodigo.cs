using Microsoft.EntityFrameworkCore;
using SGCP.Api.Datos;
namespace SGCP.Api.Servicios;

public class GeneradorCodigo(SgcpContext db)
{
    public async Task<string> SiguienteAsync(string prefijo)
    {
        var consulta = prefijo switch
        {
            "COT" => db.Cotizaciones.Select(x => x.Codigo),
            "PED" => db.Pedidos.Select(x => x.Codigo),
            "OP" => db.OrdenesProduccion.Select(x => x.Codigo),
            "RUT" => db.Rutas.Select(x => x.Codigo),
            _ => throw new ArgumentException("Prefijo no implementado.", nameof(prefijo))
        };
        var codigos = await consulta.ToListAsync();
        var siguiente = codigos.Select(x => int.TryParse(x.Split('-').Last(), out var n) ? n : 0).DefaultIfEmpty().Max() + 1;
        return $"{prefijo}-{siguiente:D3}";
    }
}
