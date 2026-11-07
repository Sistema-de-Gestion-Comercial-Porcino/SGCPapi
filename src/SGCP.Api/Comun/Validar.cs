namespace SGCP.Api.Comun;

public static class Validar
{
    public static void Cantidad(decimal valor)
    {
        if (valor <= 0 || valor > 999999999m || decimal.Round(valor, 3) != valor)
            throw new ReglaException("CANTIDAD_INVALIDA", "Indica una cantidad positiva con hasta tres decimales.", 400);
    }
    public static void Dinero(decimal valor)
    {
        if (valor <= 0 || valor > 9999999999.99m || decimal.Round(valor, 2) != valor)
            throw new ReglaException("IMPORTE_INVALIDO", "Indica un importe positivo con hasta dos decimales.", 400);
    }
    public static DateTime FechaReal(DateTime fecha)
    {
        if (fecha.Kind != DateTimeKind.Utc || fecha < new DateTime(2020, 1, 1) || fecha > DateTime.UtcNow.AddMinutes(5))
            throw new ReglaException("FECHA_INVALIDA", "Envía la fecha real UTC, sin fechas futuras.", 400);
        return fecha;
    }
    public static void Previstas(DateTime inicio, DateTime fin)
    {
        if (inicio.Kind != DateTimeKind.Utc || fin.Kind != DateTimeKind.Utc || inicio.Year < 2020 || fin <= inicio)
            throw new ReglaException("FECHAS_INVALIDAS", "Indica inicio y fin UTC; el fin debe ser posterior.", 400);
    }
    public static void Motivo(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 1000)
            throw new ReglaException("MOTIVO_REQUERIDO", "Escribe el motivo (hasta 1000 caracteres).", 400);
    }
}
