namespace SGCP.Api.Comun;

public static class CadenaConexion
{
    private const string Ejemplo = "Server=localhost;Port=3306;Database=sgcp;User ID=sgcp;Password=TU_CLAVE;SslMode=Preferred";

    public static string Exigir(IConfiguration config)
    {
        var cadena = config.GetConnectionString("Sgcp");
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException("Configura ConnectionStrings:Sgcp mediante user-secrets o variable de entorno "
                + $"(ejemplo: {Ejemplo}).");
        return cadena;
    }

    public static string ParaDisenio(IConfiguration config)
    {
        var cadena = config.GetConnectionString("Sgcp");
        if (!string.IsNullOrWhiteSpace(cadena))
            return cadena;
        Console.Error.WriteLine("Sin ConnectionStrings:Sgcp configurada. Se usa una conexión de marcador para construir el modelo: "
            + "alcanza para generar y revisar migraciones, no para aplicarlas. Para 'database update' configura la cadena real "
            + $"(ejemplo: {Ejemplo}).");
        return "Server=localhost;Port=3306;Database=sgcp;User ID=sgcp;Password=";
    }
}
