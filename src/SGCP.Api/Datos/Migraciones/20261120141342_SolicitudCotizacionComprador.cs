using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.Migraciones
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
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // El modelo anterior no representa solicitudes sin vigencia. Al revertir,
            // conservamos el borrador con fecha vencida, sin convertirlo en una oferta.
            migrationBuilder.Sql("UPDATE `Cotizaciones` SET `VigenteHasta` = `Fecha` WHERE `VigenteHasta` IS NULL");
            migrationBuilder.AlterColumn<DateTime>(
                name: "VigenteHasta",
                table: "Cotizaciones",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);
        }
    }
}
