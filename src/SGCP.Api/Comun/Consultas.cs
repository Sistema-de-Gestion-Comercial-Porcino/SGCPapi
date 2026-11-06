using Microsoft.EntityFrameworkCore;
using SGCP.Api.Contratos.Respuestas;
namespace SGCP.Api.Comun;

public static class Consultas
{
    public static async Task<ResultadoPaginado<T>> Paginar<T>(this IQueryable<T> consulta, int pagina, int tamano)
    {
        if (pagina < 1 || pagina > 100000 || !new[] { 10, 20, 50, 100 }.Contains(tamano))
            throw new ReglaException("PAGINACION_INVALIDA", "Usa una página positiva y tamaño 10, 20, 50 o 100.", 400);
        var total = await consulta.CountAsync();
        return new(await consulta.Skip((pagina - 1) * tamano).Take(tamano).ToListAsync(),
                pagina,
                tamano,
                total,
                (int)Math.Ceiling((double)total / tamano));
    }
    public static decimal Dinero(decimal valor) => decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    public static DateTime Utc(DateTime fecha) => fecha.Kind == DateTimeKind.Utc ? fecha : fecha.ToUniversalTime();
}
