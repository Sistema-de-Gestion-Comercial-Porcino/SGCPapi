using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.MigracionesSqlServer
{
    /// <inheritdoc />
    public partial class SolicitudCotizacionComprador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "VigenteHasta",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Cotizaciones] SET [VigenteHasta] = [Fecha] WHERE [VigenteHasta] IS NULL");
            migrationBuilder.AlterColumn<DateTime>(
                name: "VigenteHasta",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
