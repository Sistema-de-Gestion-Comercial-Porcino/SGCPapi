using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Sprint4ReportesFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComprobantesFiscales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ReferenciaUnica = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    NumeroFiscal = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    RespuestaProveedor = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesFiscales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesFiscales_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprobantesFiscales_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesFiscales_PedidoId",
                table: "ComprobantesFiscales",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesFiscales_ReferenciaUnica",
                table: "ComprobantesFiscales",
                column: "ReferenciaUnica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesFiscales_ResponsableId",
                table: "ComprobantesFiscales",
                column: "ResponsableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprobantesFiscales");
        }
    }
}
