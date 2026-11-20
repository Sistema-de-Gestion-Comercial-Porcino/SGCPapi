using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.MigracionesSqlServer
{
    /// <inheritdoc />
    public partial class SeguimientoCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeguimientosCobro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CondicionPagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaGestion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProximaFecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientosCobro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeguimientosCobro_CondicionesPago_CondicionPagoId",
                        column: x => x.CondicionPagoId,
                        principalTable: "CondicionesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientosCobro_CondicionPagoId",
                table: "SeguimientosCobro",
                column: "CondicionPagoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeguimientosCobro");
        }
    }
}
