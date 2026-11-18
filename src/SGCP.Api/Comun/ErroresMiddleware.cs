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
        catch (Exception e) when (EsInterbloqueo(e))
        {
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

    /// <summary>
    /// Interbloqueo o espera de bloqueo agotada, en cualquier nivel de la cadena de excepciones.
    /// SQL Server lo entrega envuelto (InvalidOperationException → DbUpdateException → SqlException); MySQL, directo.
    /// MySQL: 1213 interbloqueo, 1205 espera agotada. SQL Server: 1205 interbloqueo, 1222 espera agotada.
    /// </summary>
    private static bool EsInterbloqueo(Exception? e)
    {
        for (; e is not null; e = e.InnerException)
            if (e is MySqlException { Number: 1213 or 1205 } || e is Microsoft.Data.SqlClient.SqlException { Number: 1205 or 1222 })
                return true;
        return false;
    }
}
