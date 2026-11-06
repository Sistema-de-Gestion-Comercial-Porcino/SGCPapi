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
