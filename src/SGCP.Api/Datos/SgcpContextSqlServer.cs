using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
namespace SGCP.Api.Datos;

/// <summary>
/// El mismo modelo que <see cref="SgcpContext"/>, para SQL Server. Es un tipo aparte para que EF Core le asocie
/// sus propias migraciones (<c>Datos/MigracionesSqlServer</c>) sin tocar las de MySQL.
/// </summary>
public partial class SgcpContextSqlServer(DbContextOptions<SgcpContextSqlServer> opciones) : SgcpContext(opciones)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
            foreach (var propiedad in entidad.GetProperties())
            {
                // Las columnas calculadas se escribieron con comillas de MySQL (`Columna`); SQL Server usa [Columna].
                var sql = propiedad.GetComputedColumnSql();
                if (sql is not null)
                    propiedad.SetComputedColumnSql(Comillas().Replace(sql, "[$1]"));
                // mediumblob (hasta 16 MB) no existe en SQL Server; varbinary(max) cubre el mismo contenido.
                if (string.Equals(propiedad.GetColumnType(), "mediumblob", StringComparison.OrdinalIgnoreCase))
                    propiedad.SetColumnType("varbinary(max)");
            }
    }

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex Comillas();
}
