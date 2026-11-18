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
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
            ReemplazarIndicesCalculados(entidad);
    }

    /// <summary>
    /// En MySQL la unicidad condicional se resuelve con un índice único sobre una columna calculada
    /// <c>CASE WHEN condición THEN Columna ELSE NULL END</c>. SQL Server no permite filtrar por una columna calculada
    /// (error 10609); su forma nativa es el índice único filtrado sobre la columna real: <c>... WHERE condición</c>.
    /// </summary>
    private static void ReemplazarIndicesCalculados(Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entidad)
    {
        foreach (var indice in entidad.GetIndexes().Where(i => i.IsUnique && i.Properties.Any(p => p.GetComputedColumnSql() is not null)).ToList())
        {
            var columnas = new List<Microsoft.EntityFrameworkCore.Metadata.IMutableProperty>();
            var filtros = new List<string>();
            foreach (var propiedad in indice.Properties)
            {
                var sql = propiedad.GetComputedColumnSql();
                if (sql is null) { columnas.Add(propiedad); continue; }
                var m = CasoCondicional().Match(sql);
                if (!m.Success)
                    throw new InvalidOperationException($"No se puede traducir a SQL Server el índice sobre {entidad.ShortName()}.{propiedad.Name}: {sql}");
                var columna = entidad.GetProperties().Single(p => p.GetColumnName() == m.Groups["col"].Value);
                columnas.Add(columna);
                filtros.Add(m.Groups["cond"].Value.Trim());
                if (columna.IsNullable)
                    filtros.Add($"[{columna.GetColumnName()}] IS NOT NULL");
            }
            var nombre = indice.GetDatabaseName() ?? throw new InvalidOperationException("Índice sin nombre en " + entidad.ShortName());
            entidad.RemoveIndex(indice);
            var nuevo = entidad.AddIndex(columnas, nombre);
            nuevo.IsUnique = true;
            nuevo.SetDatabaseName(nombre);
            nuevo.SetFilter(string.Join(" AND ", filtros.Distinct()));
        }
    }

    [GeneratedRegex(@"^CASE WHEN (?<cond>.+) THEN \[(?<col>[^\]]+)\] ELSE NULL END$")]
    private static partial Regex CasoCondicional();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex Comillas();
}
