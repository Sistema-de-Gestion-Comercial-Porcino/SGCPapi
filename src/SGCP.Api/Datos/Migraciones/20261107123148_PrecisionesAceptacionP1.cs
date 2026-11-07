using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PrecisionesAceptacionP1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PesoRecibido",
                table: "Recepciones",
                type: "decimal(12,3)",
                precision: 12,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoPendienteAlConfirmar",
                table: "Recepciones",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unidad",
                table: "Recepciones",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenciaUnica",
                table: "InsumosProduccion",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Unidad",
                table: "ControlesFrio",
                type: "varchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AcuerdoServicio",
                table: "Cargos",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PrestadoEn",
                table: "Cargos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrevisionesProducto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProductoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CantidadEstimada = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisionesProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrevisionesProducto_OrdenesProduccion_OrdenProduccionId",
                        column: x => x.OrdenProduccionId,
                        principalTable: "OrdenesProduccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrevisionesProducto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.Sql("UPDATE `InsumosProduccion` SET `ReferenciaUnica` = `Id` WHERE `ReferenciaUnica` = '00000000-0000-0000-0000-000000000000';");
            migrationBuilder.Sql("UPDATE `ControlesFrio` SET `Unidad` = '°C' WHERE `Unidad` = '';");

            migrationBuilder.CreateIndex(
                name: "IX_InsumosProduccion_ReferenciaUnica",
                table: "InsumosProduccion",
                column: "ReferenciaUnica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionesProducto_OrdenProduccionId_ProductoId",
                table: "PrevisionesProducto",
                columns: new[] { "OrdenProduccionId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionesProducto_ProductoId",
                table: "PrevisionesProducto",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrevisionesProducto");

            migrationBuilder.DropIndex(
                name: "IX_InsumosProduccion_ReferenciaUnica",
                table: "InsumosProduccion");

            migrationBuilder.DropColumn(
                name: "PesoRecibido",
                table: "Recepciones");

            migrationBuilder.DropColumn(
                name: "SaldoPendienteAlConfirmar",
                table: "Recepciones");

            migrationBuilder.DropColumn(
                name: "Unidad",
                table: "Recepciones");

            migrationBuilder.DropColumn(
                name: "ReferenciaUnica",
                table: "InsumosProduccion");

            migrationBuilder.DropColumn(
                name: "Unidad",
                table: "ControlesFrio");

            migrationBuilder.DropColumn(
                name: "AcuerdoServicio",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "PrestadoEn",
                table: "Cargos");
        }
    }
}
