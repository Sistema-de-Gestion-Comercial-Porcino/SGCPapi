using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.MigracionesSqlServer
{
    /// <inheritdoc />
    public partial class PerfilesYDocumentoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_IdentificacionFiscalUnica",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "SIN_ESPECIFICAR");

            migrationBuilder.AlterColumn<string>(
                name: "IdentificacionFiscalUnica",
                table: "Clientes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                computedColumnSql: "CASE WHEN [IdentificacionFiscal] <> '' THEN [IdentificacionFiscal] ELSE NULL END",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN [Activo] = 1 AND [IdentificacionFiscal] <> '' THEN [IdentificacionFiscal] ELSE NULL END",
                oldStored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdentificacionFiscalUnica",
                table: "Clientes",
                column: "IdentificacionFiscal",
                unique: true,
                filter: "[IdentificacionFiscal] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_IdentificacionFiscalUnica",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "IdentificacionFiscalUnica",
                table: "Clientes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                computedColumnSql: "CASE WHEN [Activo] = 1 AND [IdentificacionFiscal] <> '' THEN [IdentificacionFiscal] ELSE NULL END",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN [IdentificacionFiscal] <> '' THEN [IdentificacionFiscal] ELSE NULL END",
                oldStored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdentificacionFiscalUnica",
                table: "Clientes",
                column: "IdentificacionFiscal",
                unique: true,
                filter: "[Activo] = 1 AND [IdentificacionFiscal] <> ''");
        }
    }
}
