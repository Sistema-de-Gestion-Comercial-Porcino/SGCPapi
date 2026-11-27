using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class InicioFallaFrio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InicioFalla",
                table: "ControlesFrio",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InicioFalla",
                table: "ControlesFrio");
        }
    }
}
