using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Sprint3PagosEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "LoteId",
                table: "ControlesFrio",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AddColumn<Guid>(
                name: "EntregaId",
                table: "ControlesFrio",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CondicionesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Modalidad = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    ImporteAutorizado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FechaAcuerdo = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Vencimiento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Activa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PedidoActivo = table.Column<Guid>(type: "char(36)", nullable: true, computedColumnSql: "CASE WHEN `Activa` = 1 THEN `PedidoId` ELSE NULL END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CondicionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CondicionesPago_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CondicionesPago_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ReferenciaUnica = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Medio = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Anulado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Transportes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Modalidad = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Transportista = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Contacto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Placa = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    DescripcionVehiculo = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Conservacion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transportes", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Senas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PagoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AcuerdoResolucion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    ResueltaEn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Senas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Senas_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Senas_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rutas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    TransporteId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Zona = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    SalidaReal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RetornoReal = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rutas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rutas_Transportes_TransporteId",
                        column: x => x.TransporteId,
                        principalTable: "Transportes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rutas_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AplicacionesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PagoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CargoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    SenaId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AplicacionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AplicacionesPago_Cargos_CargoId",
                        column: x => x.CargoId,
                        principalTable: "Cargos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AplicacionesPago_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AplicacionesPago_Senas_SenaId",
                        column: x => x.SenaId,
                        principalTable: "Senas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Entregas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RutaId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Modalidad = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FechaPrevista = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaReal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Receptor = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    ObservacionRecepcion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    OrdenEnRuta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entregas_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entregas_Rutas_RutaId",
                        column: x => x.RutaId,
                        principalTable: "Rutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entregas_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Recepciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    EntregaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClaveUnica = table.Column<Guid>(type: "char(36)", nullable: false),
                    VersionBase = table.Column<int>(type: "int", nullable: false),
                    AutorId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FechaReal = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaSincronizacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Conforme = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Receptor = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Observacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    HuellaContenido = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    MotivoConflicto = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    RevisadaPorId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RevisadaEn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Resolucion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    EntregaConfirmada = table.Column<Guid>(type: "char(36)", nullable: true, computedColumnSql: "CASE WHEN `Estado` = 'CONFIRMADA' THEN `EntregaId` ELSE NULL END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recepciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recepciones_Entregas_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entregas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recepciones_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recepciones_Usuarios_RevisadaPorId",
                        column: x => x.RevisadaPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EvidenciasRecepcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    RecepcionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Contenido = table.Column<byte[]>(type: "mediumblob", nullable: false),
                    Sha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenciasRecepcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenciasRecepcion_Recepciones_RecepcionId",
                        column: x => x.RecepcionId,
                        principalTable: "Recepciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ControlesFrio_EntregaId",
                table: "ControlesFrio",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionesPago_CargoId",
                table: "AplicacionesPago",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionesPago_PagoId_CargoId",
                table: "AplicacionesPago",
                columns: new[] { "PagoId", "CargoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionesPago_SenaId",
                table: "AplicacionesPago",
                column: "SenaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesPago_PedidoActivo",
                table: "CondicionesPago",
                column: "PedidoActivo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesPago_PedidoId",
                table: "CondicionesPago",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesPago_ResponsableId",
                table: "CondicionesPago",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_PedidoId",
                table: "Entregas",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_ResponsableId_Estado",
                table: "Entregas",
                columns: new[] { "ResponsableId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_RutaId",
                table: "Entregas",
                column: "RutaId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciasRecepcion_RecepcionId",
                table: "EvidenciasRecepcion",
                column: "RecepcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ClienteId_Fecha",
                table: "Pagos",
                columns: new[] { "ClienteId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ReferenciaUnica",
                table: "Pagos",
                column: "ReferenciaUnica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ResponsableId",
                table: "Pagos",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Recepciones_AutorId",
                table: "Recepciones",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Recepciones_ClaveUnica",
                table: "Recepciones",
                column: "ClaveUnica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recepciones_EntregaConfirmada",
                table: "Recepciones",
                column: "EntregaConfirmada",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recepciones_EntregaId",
                table: "Recepciones",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recepciones_RevisadaPorId",
                table: "Recepciones",
                column: "RevisadaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_Codigo",
                table: "Rutas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_ResponsableId",
                table: "Rutas",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_TransporteId",
                table: "Rutas",
                column: "TransporteId");

            migrationBuilder.CreateIndex(
                name: "IX_Senas_PagoId",
                table: "Senas",
                column: "PagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Senas_PedidoId",
                table: "Senas",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transportes_Codigo",
                table: "Transportes",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ControlesFrio_Entregas_EntregaId",
                table: "ControlesFrio",
                column: "EntregaId",
                principalTable: "Entregas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ControlesFrio_Entregas_EntregaId",
                table: "ControlesFrio");

            migrationBuilder.DropTable(
                name: "AplicacionesPago");

            migrationBuilder.DropTable(
                name: "CondicionesPago");

            migrationBuilder.DropTable(
                name: "EvidenciasRecepcion");

            migrationBuilder.DropTable(
                name: "Senas");

            migrationBuilder.DropTable(
                name: "Recepciones");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Entregas");

            migrationBuilder.DropTable(
                name: "Rutas");

            migrationBuilder.DropTable(
                name: "Transportes");

            migrationBuilder.DropIndex(
                name: "IX_ControlesFrio_EntregaId",
                table: "ControlesFrio");

            migrationBuilder.DropColumn(
                name: "EntregaId",
                table: "ControlesFrio");

            migrationBuilder.AlterColumn<Guid>(
                name: "LoteId",
                table: "ControlesFrio",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);
        }
    }
}
