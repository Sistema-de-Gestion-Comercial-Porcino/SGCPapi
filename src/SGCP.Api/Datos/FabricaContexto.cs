using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SGCP.Api.Comun;
namespace SGCP.Api.Datos;

public class FabricaContexto : IDesignTimeDbContextFactory<SgcpContext>
{
    public SgcpContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<SgcpContext>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        return new(new DbContextOptionsBuilder<SgcpContext>().UseMySQL(CadenaConexion.ParaDisenio(config)).Options);
    }
}

/// <summary>Fábrica de diseño para las migraciones de SQL Server: <c>dotnet ef ... --context SgcpContextSqlServer</c>.</summary>
public class FabricaContextoSqlServer : IDesignTimeDbContextFactory<SgcpContextSqlServer>
{
    public SgcpContextSqlServer CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<SgcpContext>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        // Solo se usa la cadena configurada si apunta a SQL Server; si el equipo tiene la de MySQL, se usa el marcador.
        const string marcador = "Server=localhost;Database=sgcp;Trusted_Connection=True;TrustServerCertificate=True";
        var cadena = ConfiguracionBaseDatos.Proveedor(config) == ProveedorBaseDatos.SqlServer ? CadenaConexion.ParaDisenio(config, marcador) : marcador;
        return new(new DbContextOptionsBuilder<SgcpContextSqlServer>().UseSqlServer(cadena).Options);
    }
}
