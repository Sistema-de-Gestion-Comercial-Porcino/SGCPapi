using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PerfilesYDocumentoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "Clientes",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "SIN_ESPECIFICAR");

            migrationBuilder.AlterColumn<string>(
                name: "IdentificacionFiscalUnica",
                table: "Clientes",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true,
                computedColumnSql: "CASE WHEN `IdentificacionFiscal` <> '' THEN `IdentificacionFiscal` ELSE NULL END",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40,
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN `Activo` = 1 AND `IdentificacionFiscal` <> '' THEN `IdentificacionFiscal` ELSE NULL END",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "IdentificacionFiscalUnica",
                table: "Clientes",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true,
                computedColumnSql: "CASE WHEN `Activo` = 1 AND `IdentificacionFiscal` <> '' THEN `IdentificacionFiscal` ELSE NULL END",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40,
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN `IdentificacionFiscal` <> '' THEN `IdentificacionFiscal` ELSE NULL END",
                oldStored: true);
        }
    }
}
