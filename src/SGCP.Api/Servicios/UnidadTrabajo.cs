using System.Data;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Datos;
namespace SGCP.Api.Servicios;

public class UnidadTrabajo(SgcpContext db)
{
    public async Task<T> Ejecutar<T>(Func<Task<T>> accion)
    {
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var resultado = await accion();
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return resultado;
    }
}
