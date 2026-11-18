using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SGCP.Api.Contratos.Respuestas;
namespace SGCP.Api.Comun;

public class ErroresMiddleware(RequestDelegate siguiente, ILogger<ErroresMiddleware> log)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        { await siguiente(contexto); }
        catch (ReglaException e)
        {
            contexto.Response.StatusCode = e.Estado;
            await contexto.Response.WriteAsJsonAsync(new ErrorRespuesta(e.Codigo,
                e.Message,
                e.Detalles));
        }
        catch (MySqlException e) when (e.Number == 1213
                    || e.Number == 1205)
        {
            contexto.Response.StatusCode = 409;
            await contexto.Response.WriteAsJsonAsync(new ErrorRespuesta("CONFLICTO_CONCURRENTE",
                "Otra operación actualizó los mismos datos. Actualiza y vuelve a intentar.",
                []));
        }
        catch (Microsoft.Data.SqlClient.SqlException e) when (e.Number == 1205 || e.Number == 1222)
        {
            // SQL Server: 1205 = víctima de interbloqueo, 1222 = tiempo de espera de bloqueo (equivalentes a 1213/1205 de MySQL).
            contexto.Response.StatusCode = 409;
            await contexto.Response.WriteAsJsonAsync(new ErrorRespuesta("CONFLICTO_CONCURRENTE",
                "Otra operación actualizó los mismos datos. Actualiza y vuelve a intentar.",
                []));
        }
        catch (DbUpdateException e)
        {
            log.LogWarning(e, "Conflicto al persistir una operación");
            contexto.Response.StatusCode = 409;
            await contexto.Response.WriteAsJsonAsync(new ErrorRespuesta("CONFLICTO_DATOS",
                "Los datos cambiaron o existe un registro con el mismo identificador. Actualiza y vuelve a intentar.",
                []));
        }
        catch (Exception e)
        {
            log.LogError(e, "Error de operación");
            contexto.Response.StatusCode = 500;
            await contexto.Response.WriteAsJsonAsync(new ErrorRespuesta("ERROR_INTERNO",
                "No se pudo completar la operación.",
                []));
        }
    }
}
