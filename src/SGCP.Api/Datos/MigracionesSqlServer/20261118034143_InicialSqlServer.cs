using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGCP.Api.Datos.MigracionesSqlServer
{
    /// <inheritdoc />
    public partial class InicialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Animales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Procedencia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    PesoIngreso = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClasificacionesProducto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Familia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasificacionesProducto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IdentificacionFiscal = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CondicionHabitual = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IdentificacionFiscalUnica = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true, computedColumnSql: "CASE WHEN [Activo] = 1 AND [IdentificacionFiscal] <> '' THEN [IdentificacionFiscal] ELSE NULL END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transportes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Modalidad = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Transportista = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DescripcionVehiculo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Conservacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transportes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Presentacion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    UnidadVenta = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClasificacionProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_ClasificacionesProducto_ClasificacionProductoId",
                        column: x => x.ClasificacionProductoId,
                        principalTable: "ClasificacionesProducto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotivoRespuesta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Cotizaciones_CotizacionOrigenId",
                        column: x => x.CotizacionOrigenId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    HashClave = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolesPermisos",
                columns: table => new
                {
                    PermisoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermisos", x => new { x.PermisoId, x.RolId });
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreciosProducto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostoReferencia = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoConVigenciaAbierta = table.Column<Guid>(type: "uniqueidentifier", nullable: true, computedColumnSql: "CASE WHEN [VigenteHasta] IS NULL THEN [ProductoId] ELSE NULL END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreciosProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreciosProducto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesCotizacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCotizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCotizacion_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCotizacion_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaComprometida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoComercial = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoPreparacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CondicionPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CostoEstimadoConfirmado = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    MargenEstimadoConfirmado = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ControlesSanitarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "OrdenesProduccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AnimalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InicioPrevisto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinPrevisto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InicioReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AnimalEnOrden = table.Column<Guid>(type: "uniqueidentifier", nullable: true, computedColumnSql: "CASE WHEN [Estado] <> 'ANULADA' THEN [AnimalId] ELSE NULL END", stored: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenciaUnica = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Medio = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "RecuperacionesAcceso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SolicitadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtilizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Invalidado = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecuperacionesAcceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecuperacionesAcceso_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Operacion = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoria_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rutas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TransporteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SalidaReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetornoReal = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Sesiones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expira = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revocada = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sesiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sesiones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                columns: table => new
                {
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => new { x.RolId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesFiscales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenciaUnica = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NumeroFiscal = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RespuestaProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "CondicionesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Modalidad = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ImporteAutorizado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FechaAcuerdo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Vencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    PedidoActivo = table.Column<Guid>(type: "uniqueidentifier", nullable: true, computedColumnSql: "CASE WHEN [Activa] = 1 THEN [PedidoId] ELSE NULL END", stored: true)
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
                });

            migrationBuilder.CreateTable(
                name: "DetallesPedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadSolicitada = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    CantidadConfirmada = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PrecioAcordado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CostoReferenciaAcordado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaProduccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Vencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Procedencia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenciaExterna = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "PrevisionesProducto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadEstimada = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Senas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcuerdoResolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResueltaEn = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Entregas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RutaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Modalidad = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FechaPrevista = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Receptor = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    ObservacionRecepcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Cargos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    AcuerdoServicio = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PrestadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Existencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Incidencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnimalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resuelta = table.Column<bool>(type: "bit", nullable: false),
                    Resolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResueltaEn = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Pesajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PesoBruto = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Tara = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Neto = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "ResultadosProduccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadReal = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "ControlesFrio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntregaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Temperatura = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Evaluacion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlesFrio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlesFrio_Entregas_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entregas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                });

            migrationBuilder.CreateTable(
                name: "Recepciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntregaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaveUnica = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionBase = table.Column<int>(type: "int", nullable: false),
                    AutorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaReal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaSincronizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PesoRecibido = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SaldoPendienteAlConfirmar = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Conforme = table.Column<bool>(type: "bit", nullable: false),
                    Receptor = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    HuellaContenido = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MotivoConflicto = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RevisadaPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevisadaEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EntregaConfirmada = table.Column<Guid>(type: "uniqueidentifier", nullable: true, computedColumnSql: "CASE WHEN [Estado] = 'CONFIRMADA' THEN [EntregaId] ELSE NULL END", stored: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AplicacionesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CargoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Importe = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "InsumosProduccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenciaUnica = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenProduccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "MovimientosStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivoLiberacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DetalleActivo = table.Column<Guid>(type: "uniqueidentifier", nullable: true, computedColumnSql: "CASE WHEN [Estado] = 'ACTIVA' THEN [DetallePedidoId] ELSE NULL END", stored: true)
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
                });

            migrationBuilder.CreateTable(
                name: "EntradasInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PesajeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExistenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Anulada = table.Column<bool>(type: "bit", nullable: false),
                    AnuladaPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnuladaEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "EvidenciasRecepcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecepcionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Contenido = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_Animales_Identificacion",
                table: "Animales",
                column: "Identificacion",
                unique: true);

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
                unique: true,
                filter: "[SenaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_DetallePedidoId",
                table: "Cargos",
                column: "DetallePedidoId",
                unique: true,
                filter: "[DetallePedidoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_PedidoId",
                table: "Cargos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasificacionesProducto_Codigo",
                table: "ClasificacionesProducto",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdentificacionFiscalUnica",
                table: "Clientes",
                column: "IdentificacionFiscalUnica",
                unique: true,
                filter: "[IdentificacionFiscalUnica] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesPago_PedidoActivo",
                table: "CondicionesPago",
                column: "PedidoActivo",
                unique: true,
                filter: "[PedidoActivo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesPago_PedidoId",
                table: "CondicionesPago",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesPago_ResponsableId",
                table: "CondicionesPago",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlesFrio_EntregaId",
                table: "ControlesFrio",
                column: "EntregaId");

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
                name: "IX_Cotizaciones_ClienteId",
                table: "Cotizaciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_Codigo",
                table: "Cotizaciones",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_CotizacionOrigenId",
                table: "Cotizaciones",
                column: "CotizacionOrigenId",
                unique: true,
                filter: "[CotizacionOrigenId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCotizacion_CotizacionId",
                table: "DetallesCotizacion",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCotizacion_ProductoId",
                table: "DetallesCotizacion",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_PedidoId",
                table: "DetallesPedido",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_ProductoId",
                table: "DetallesPedido",
                column: "ProductoId");

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
                name: "IX_InsumosProduccion_ReferenciaUnica",
                table: "InsumosProduccion",
                column: "ReferenciaUnica",
                unique: true);

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
                unique: true,
                filter: "[AnimalEnOrden] IS NOT NULL");

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
                name: "IX_Pedidos_ClienteId",
                table: "Pedidos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Codigo",
                table: "Pedidos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_CotizacionId",
                table: "Pedidos",
                column: "CotizacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Codigo",
                table: "Permisos",
                column: "Codigo",
                unique: true);

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
                name: "IX_PreciosProducto_ProductoConVigenciaAbierta",
                table: "PreciosProducto",
                column: "ProductoConVigenciaAbierta",
                unique: true,
                filter: "[ProductoConVigenciaAbierta] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PreciosProducto_ProductoId_VigenteDesde",
                table: "PreciosProducto",
                columns: new[] { "ProductoId", "VigenteDesde" },
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

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ClasificacionProductoId",
                table: "Productos",
                column: "ClasificacionProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

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
                unique: true,
                filter: "[EntregaConfirmada] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Recepciones_EntregaId",
                table: "Recepciones",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recepciones_RevisadaPorId",
                table: "Recepciones",
                column: "RevisadaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecuperacionesAcceso_TokenHash",
                table: "RecuperacionesAcceso",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecuperacionesAcceso_UsuarioId",
                table: "RecuperacionesAcceso",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_UsuarioId",
                table: "RegistrosAuditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_DetalleActivo_ExistenciaId",
                table: "Reservas",
                columns: new[] { "DetalleActivo", "ExistenciaId" },
                unique: true,
                filter: "[DetalleActivo] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_RolId",
                table: "RolesPermisos",
                column: "RolId");

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
                name: "IX_Sesiones_UsuarioId_Revocada",
                table: "Sesiones",
                columns: new[] { "UsuarioId", "Revocada" });

            migrationBuilder.CreateIndex(
                name: "IX_Transportes_Codigo",
                table: "Transportes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ClienteId",
                table: "Usuarios",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_UsuarioId",
                table: "UsuariosRoles",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AplicacionesPago");

            migrationBuilder.DropTable(
                name: "ComprobantesFiscales");

            migrationBuilder.DropTable(
                name: "CondicionesPago");

            migrationBuilder.DropTable(
                name: "ControlesFrio");

            migrationBuilder.DropTable(
                name: "ControlesSanitarios");

            migrationBuilder.DropTable(
                name: "DetallesCotizacion");

            migrationBuilder.DropTable(
                name: "EntradasInventario");

            migrationBuilder.DropTable(
                name: "EvidenciasRecepcion");

            migrationBuilder.DropTable(
                name: "Incidencias");

            migrationBuilder.DropTable(
                name: "InsumosProduccion");

            migrationBuilder.DropTable(
                name: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "PreciosProducto");

            migrationBuilder.DropTable(
                name: "PrevisionesProducto");

            migrationBuilder.DropTable(
                name: "RecuperacionesAcceso");

            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "ResultadosProduccion");

            migrationBuilder.DropTable(
                name: "RolesPermisos");

            migrationBuilder.DropTable(
                name: "Sesiones");

            migrationBuilder.DropTable(
                name: "UsuariosRoles");

            migrationBuilder.DropTable(
                name: "Cargos");

            migrationBuilder.DropTable(
                name: "Senas");

            migrationBuilder.DropTable(
                name: "Pesajes");

            migrationBuilder.DropTable(
                name: "Recepciones");

            migrationBuilder.DropTable(
                name: "Existencias");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "DetallesPedido");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Entregas");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Rutas");

            migrationBuilder.DropTable(
                name: "OrdenesProduccion");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "Transportes");

            migrationBuilder.DropTable(
                name: "Animales");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "ClasificacionesProducto");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
