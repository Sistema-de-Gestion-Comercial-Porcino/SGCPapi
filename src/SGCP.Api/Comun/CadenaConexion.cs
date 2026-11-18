namespace SGCP.Api.Comun;

public static class CadenaConexion
{
    private const string EjemploMySql = "Server=localhost;Port=3306;Database=sgcp;User ID=sgcp;Password=TU_CLAVE;SslMode=Preferred";
    private const string EjemploSqlServer = "Server=localhost;Database=sgcp;Trusted_Connection=True;TrustServerCertificate=True";
    private static string Ejemplo(IConfiguration config) =>
        ConfiguracionBaseDatos.Proveedor(config) == ProveedorBaseDatos.SqlServer ? EjemploSqlServer : EjemploMySql;

    public static string Exigir(IConfiguration config)
    {
        var cadena = config.GetConnectionString("Sgcp");
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException("Configura ConnectionStrings:Sgcp mediante user-secrets o variable de entorno "
                + $"(ejemplo: {Ejemplo(config)}).");
        return cadena;
    }

    public static string ParaDisenio(IConfiguration config, string marcador = "Server=localhost;Port=3306;Database=sgcp;User ID=sgcp;Password=")
    {
        var cadena = config.GetConnectionString("Sgcp");
        if (!string.IsNullOrWhiteSpace(cadena))
            return cadena;
        Console.Error.WriteLine("Sin ConnectionStrings:Sgcp configurada. Se usa una conexión de marcador para construir el modelo: "
            + "alcanza para generar y revisar migraciones, no para aplicarlas. Para 'database update' configura la cadena real "
            + $"(ejemplo: {Ejemplo(config)}).");
        return marcador;
    }
}
