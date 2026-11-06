namespace SGCP.Api.Comun;

public static class CadenaConexion
{
    public static string Exigir(IConfiguration config)
    {
        var cadena = config.GetConnectionString("Sgcp");
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException("Configura ConnectionStrings:Sgcp mediante user-secrets o variable de entorno "
                + "(ejemplo: Server=localhost;Port=3306;Database=sgcp;User ID=sgcp;Password=TU_CLAVE;SslMode=Preferred).");
        return cadena;
    }
}
