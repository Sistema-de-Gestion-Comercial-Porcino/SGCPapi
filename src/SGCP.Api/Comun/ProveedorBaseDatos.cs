using Microsoft.EntityFrameworkCore;
using SGCP.Api.Datos;
namespace SGCP.Api.Comun;

/// <summary>Motor de base de datos elegido con <c>BaseDatos:Proveedor</c>: <c>MySql</c> (por defecto) o <c>SqlServer</c>.</summary>
public enum ProveedorBaseDatos { MySql, SqlServer }

public static class ConfiguracionBaseDatos
{
    public static ProveedorBaseDatos Proveedor(IConfiguration config)
    {
        var valor = config["BaseDatos:Proveedor"];
        if (string.IsNullOrWhiteSpace(valor))
            return ProveedorBaseDatos.MySql;
        return Enum.TryParse<ProveedorBaseDatos>(valor.Trim(), ignoreCase: true, out var p) && Enum.IsDefined(p) ? p
            : throw new InvalidOperationException($"BaseDatos:Proveedor = '{valor}' no es válido. Usa MySql o SqlServer.");
    }

    /// <summary>Registra <see cref="SgcpContext"/> con el motor configurado. Con SQL Server se usa
    /// <see cref="SgcpContextSqlServer"/>, que tiene sus propias migraciones.</summary>
    public static void AgregarContexto(IServiceCollection servicios, IConfiguration config)
    {
        // La cadena se exige al crear el contexto, no al registrarlo: así las pruebas pueden reemplazar el registro.
        if (Proveedor(config) == ProveedorBaseDatos.SqlServer)
            servicios.AddDbContext<SgcpContext, SgcpContextSqlServer>(o => o.UseSqlServer(CadenaConexion.Exigir(config)));
        else
            servicios.AddDbContext<SgcpContext>(o => o.UseMySQL(CadenaConexion.Exigir(config)));
    }
}
