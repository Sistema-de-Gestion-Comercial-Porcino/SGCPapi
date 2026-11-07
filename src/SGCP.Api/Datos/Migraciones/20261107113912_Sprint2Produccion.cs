using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Sprint2Produccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Animales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Identificacion = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Procedencia = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Proveedor = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    PesoIngreso = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animales", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cargos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Tipo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Concepto = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Anulado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cargos_DetallesPedido_DetallePedidoId",
                        column: x => x.DetallePedidoId,
                        principalTable: "DetallesPedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ControlesSanitarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    AnimalId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Resultado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Observacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlesSanitarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlesSanitarios_Animales_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ControlesSanitarios_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdenesProduccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    AnimalId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InicioPrevisto = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FinPrevisto = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InicioReal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FinReal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Observacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    AnimalEnOrden = table.Column<Guid>(type: "char(36)", nullable: true, computedColumnSql: "CASE WHEN `Estado` <> 'ANULADA' THEN `AnimalId` ELSE NULL END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesProduccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesProduccion_Animales_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesProduccion_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    ProductoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "char(36)", nullable: true),
                    FechaProduccion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Vencimiento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Unidad = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Procedencia = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    ReferenciaExterna = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_OrdenesProduccion_OrdenProduccionId",
                        column: x => x.OrdenProduccionId,
                        principalTable: "OrdenesProduccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ControlesFrio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoteId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Temperatura = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Ubicacion = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    Evaluacion = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Observacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlesFrio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlesFrio_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ControlesFrio_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Existencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoteId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProductoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Ubicacion = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: false),
                    CantidadFisica = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Existencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Existencias_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Existencias_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Incidencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoteId = table.Column<Guid>(type: "char(36)", nullable: true),
                    AnimalId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Motivo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Resuelta = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Resolucion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    ResueltaEn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidencias_Animales_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pesajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoteId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProductoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PesoBruto = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Tara = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Neto = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pesajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pesajes_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pesajes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pesajes_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ResultadosProduccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProductoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoteId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CantidadReal = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: true),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosProduccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadosProduccion_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultadosProduccion_OrdenesProduccion_OrdenProduccionId",
                        column: x => x.OrdenProduccionId,
                        principalTable: "OrdenesProduccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultadosProduccion_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InsumosProduccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsumosProduccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsumosProduccion_Existencias_ExistenciaId",
                        column: x => x.ExistenciaId,
                        principalTable: "Existencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InsumosProduccion_OrdenesProduccion_OrdenProduccionId",
                        column: x => x.OrdenProduccionId,
                        principalTable: "OrdenesProduccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InsumosProduccion_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MovimientosStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Motivo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Existencias_ExistenciaId",
                        column: x => x.ExistenciaId,
                        principalTable: "Existencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Estado = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MotivoLiberacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    DetalleActivo = table.Column<Guid>(type: "char(36)", nullable: true, computedColumnSql: "CASE WHEN `Estado` = 'ACTIVA' THEN `DetallePedidoId` ELSE NULL END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_DetallesPedido_DetallePedidoId",
                        column: x => x.DetallePedidoId,
                        principalTable: "DetallesPedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Existencias_ExistenciaId",
                        column: x => x.ExistenciaId,
                        principalTable: "Existencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EntradasInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoteId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProductoId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PesajeId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Anulada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AnuladaPorId = table.Column<Guid>(type: "char(36)", nullable: true),
                    AnuladaEn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntradasInventario_Existencias_ExistenciaId",
                        column: x => x.ExistenciaId,
                        principalTable: "Existencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasInventario_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasInventario_Pesajes_PesajeId",
                        column: x => x.PesajeId,
                        principalTable: "Pesajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasInventario_Usuarios_AnuladaPorId",
                        column: x => x.AnuladaPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasInventario_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Animales_Identificacion",
                table: "Animales",
                column: "Identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_DetallePedidoId",
                table: "Cargos",
                column: "DetallePedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_PedidoId",
                table: "Cargos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlesFrio_LoteId",
                table: "ControlesFrio",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlesFrio_ResponsableId",
                table: "ControlesFrio",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlesSanitarios_AnimalId_Fecha",
                table: "ControlesSanitarios",
                columns: new[] { "AnimalId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_ControlesSanitarios_ResponsableId",
                table: "ControlesSanitarios",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasInventario_AnuladaPorId",
                table: "EntradasInventario",
                column: "AnuladaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasInventario_ExistenciaId",
                table: "EntradasInventario",
                column: "ExistenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasInventario_LoteId_ProductoId",
                table: "EntradasInventario",
                columns: new[] { "LoteId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntradasInventario_PesajeId",
                table: "EntradasInventario",
                column: "PesajeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasInventario_ProductoId",
                table: "EntradasInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasInventario_ResponsableId",
                table: "EntradasInventario",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Existencias_LoteId_ProductoId",
                table: "Existencias",
                columns: new[] { "LoteId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Existencias_ProductoId",
                table: "Existencias",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_AnimalId",
                table: "Incidencias",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_LoteId",
                table: "Incidencias",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_ResponsableId",
                table: "Incidencias",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_InsumosProduccion_ExistenciaId",
                table: "InsumosProduccion",
                column: "ExistenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_InsumosProduccion_OrdenProduccionId",
                table: "InsumosProduccion",
                column: "OrdenProduccionId");

            migrationBuilder.CreateIndex(
                name: "IX_InsumosProduccion_ResponsableId",
                table: "InsumosProduccion",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_Codigo",
                table: "Lotes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_OrdenProduccionId",
                table: "Lotes",
                column: "OrdenProduccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProductoId",
                table: "Lotes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_ExistenciaId",
                table: "MovimientosStock",
                column: "ExistenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_ResponsableId",
                table: "MovimientosStock",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_Tipo_ReferenciaId",
                table: "MovimientosStock",
                columns: new[] { "Tipo", "ReferenciaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesProduccion_AnimalEnOrden",
                table: "OrdenesProduccion",
                column: "AnimalEnOrden",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesProduccion_AnimalId",
                table: "OrdenesProduccion",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesProduccion_Codigo",
                table: "OrdenesProduccion",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesProduccion_ResponsableId",
                table: "OrdenesProduccion",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Pesajes_LoteId",
                table: "Pesajes",
                column: "LoteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pesajes_ProductoId",
                table: "Pesajes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pesajes_ResponsableId",
                table: "Pesajes",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_DetalleActivo_ExistenciaId",
                table: "Reservas",
                columns: new[] { "DetalleActivo", "ExistenciaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_DetallePedidoId",
                table: "Reservas",
                column: "DetallePedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ExistenciaId",
                table: "Reservas",
                column: "ExistenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ResponsableId",
                table: "Reservas",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosProduccion_LoteId",
                table: "ResultadosProduccion",
                column: "LoteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosProduccion_OrdenProduccionId",
                table: "ResultadosProduccion",
                column: "OrdenProduccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosProduccion_ProductoId",
                table: "ResultadosProduccion",
                column: "ProductoId");
            // Los pedidos ya confirmados en Sprint 1 conservan sus importes históricos.
            migrationBuilder.Sql(@"
INSERT INTO `Cargos` (`Id`, `PedidoId`, `DetallePedidoId`, `Tipo`, `Concepto`, `Importe`, `Fecha`, `Anulado`, `MotivoAnulacion`)
SELECT UUID(), d.`PedidoId`, d.`Id`, 'PRODUCTO', CONCAT('Producto del pedido ', p.`Codigo`),
       ROUND(SUM(d.`CantidadConfirmada` * d.`PrecioAcordado`) OVER
           (PARTITION BY d.`PedidoId` ORDER BY d.`Id` ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 2)
       - ROUND(COALESCE(SUM(d.`CantidadConfirmada` * d.`PrecioAcordado`) OVER
           (PARTITION BY d.`PedidoId` ORDER BY d.`Id` ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0), 2), p.`Fecha`, 0, ''
FROM `DetallesPedido` d INNER JOIN `Pedidos` p ON p.`Id` = d.`PedidoId`
WHERE p.`EstadoComercial` = 'CONFIRMADO';");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cargos");

            migrationBuilder.DropTable(
                name: "ControlesFrio");

            migrationBuilder.DropTable(
                name: "ControlesSanitarios");

            migrationBuilder.DropTable(
                name: "EntradasInventario");

            migrationBuilder.DropTable(
                name: "Incidencias");

            migrationBuilder.DropTable(
                name: "InsumosProduccion");

            migrationBuilder.DropTable(
                name: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "ResultadosProduccion");

            migrationBuilder.DropTable(
                name: "Pesajes");

            migrationBuilder.DropTable(
                name: "Existencias");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "OrdenesProduccion");

            migrationBuilder.DropTable(
                name: "Animales");
        }
    }
}
