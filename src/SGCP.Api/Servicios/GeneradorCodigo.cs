using Microsoft.EntityFrameworkCore;
using SGCP.Api.Datos;
namespace SGCP.Api.Servicios;

public class GeneradorCodigo(SgcpContext db)
{
    public async Task<string> SiguienteAsync(string prefijo)
    {
        var codigos = prefijo == "COT" ? await db.Cotizaciones.Select(x => x.Codigo)
            .ToListAsync() : await db.Pedidos.Select(x => x.Codigo)
            .ToListAsync();
        var siguiente = codigos.Select(x => int.TryParse(x.Split('-').Last(), out var n) ? n : 0).DefaultIfEmpty().Max() + 1;
        return $"{prefijo}-{siguiente:D3}";
    }
}
