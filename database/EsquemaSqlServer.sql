IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Animales] (
        [Id] uniqueidentifier NOT NULL,
        [Identificacion] nvarchar(80) NOT NULL,
        [Procedencia] nvarchar(300) NOT NULL,
        [Proveedor] nvarchar(180) NOT NULL,
        [PesoIngreso] decimal(12,3) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        [FechaIngreso] datetime2 NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Animales] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [ClasificacionesProducto] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Familia] nvarchar(20) NOT NULL,
        [Descripcion] nvarchar(400) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ClasificacionesProducto] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Tipo] nvarchar(60) NOT NULL,
        [Telefono] nvarchar(30) NOT NULL,
        [Direccion] nvarchar(250) NOT NULL,
        [IdentificacionFiscal] nvarchar(40) NOT NULL,
        [CondicionHabitual] nvarchar(20) NOT NULL,
        [Activo] bit NOT NULL,
        [FechaAlta] datetime2 NOT NULL,
        [Observaciones] nvarchar(500) NOT NULL,
        [IdentificacionFiscalUnica] AS CASE WHEN [Activo] = 1 AND [IdentificacionFiscal] <> '' THEN [IdentificacionFiscal] ELSE NULL END PERSISTED,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Permisos] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [Descripcion] nvarchar(200) NOT NULL,
        [Modulo] nvarchar(40) NOT NULL,
        [Accion] nvarchar(40) NOT NULL,
        CONSTRAINT [PK_Permisos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(40) NOT NULL,
        [Descripcion] nvarchar(200) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Transportes] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [Modalidad] nvarchar(40) NOT NULL,
        [Transportista] nvarchar(180) NOT NULL,
        [Contacto] nvarchar(100) NOT NULL,
        [Placa] nvarchar(40) NOT NULL,
        [DescripcionVehiculo] nvarchar(300) NOT NULL,
        [Conservacion] nvarchar(300) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Transportes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Presentacion] nvarchar(60) NOT NULL,
        [UnidadVenta] nvarchar(10) NOT NULL,
        [Descripcion] nvarchar(400) NOT NULL,
        [Activo] bit NOT NULL,
        [FechaAlta] datetime2 NOT NULL,
        [ClasificacionProductoId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Productos_ClasificacionesProducto_ClasificacionProductoId] FOREIGN KEY ([ClasificacionProductoId]) REFERENCES [ClasificacionesProducto] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Cotizaciones] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [Version] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [VigenteHasta] datetime2 NOT NULL,
        [Canal] nvarchar(20) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [MotivoRespuesta] nvarchar(500) NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [CotizacionOrigenId] uniqueidentifier NULL,
        CONSTRAINT [PK_Cotizaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cotizaciones_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Cotizaciones_Cotizaciones_CotizacionOrigenId] FOREIGN KEY ([CotizacionOrigenId]) REFERENCES [Cotizaciones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Login] nvarchar(80) NOT NULL,
        [HashClave] nvarchar(500) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [IntentosFallidos] int NOT NULL,
        [Correo] nvarchar(180) NOT NULL,
        [Telefono] nvarchar(30) NULL,
        [CreadoEn] datetime2 NOT NULL,
        [UltimoAcceso] datetime2 NULL,
        [ClienteId] uniqueidentifier NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [RolesPermisos] (
        [PermisoId] uniqueidentifier NOT NULL,
        [RolId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RolesPermisos] PRIMARY KEY ([PermisoId], [RolId]),
        CONSTRAINT [FK_RolesPermisos_Permisos_PermisoId] FOREIGN KEY ([PermisoId]) REFERENCES [Permisos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RolesPermisos_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [PreciosProducto] (
        [Id] uniqueidentifier NOT NULL,
        [CostoReferencia] decimal(12,2) NOT NULL,
        [PrecioVenta] decimal(12,2) NOT NULL,
        [VigenteDesde] datetime2 NOT NULL,
        [VigenteHasta] datetime2 NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [ProductoConVigenciaAbierta] AS CASE WHEN [VigenteHasta] IS NULL THEN [ProductoId] ELSE NULL END PERSISTED,
        CONSTRAINT [PK_PreciosProducto] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PreciosProducto_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [DetallesCotizacion] (
        [Id] uniqueidentifier NOT NULL,
        [Cantidad] decimal(12,3) NOT NULL,
        [Unidad] nvarchar(10) NOT NULL,
        [PrecioUnitario] decimal(12,2) NOT NULL,
        [Subtotal] decimal(14,2) NOT NULL,
        [CotizacionId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_DetallesCotizacion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesCotizacion_Cotizaciones_CotizacionId] FOREIGN KEY ([CotizacionId]) REFERENCES [Cotizaciones] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesCotizacion_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Pedidos] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [FechaComprometida] datetime2 NOT NULL,
        [EstadoComercial] nvarchar(20) NOT NULL,
        [EstadoPreparacion] nvarchar(20) NOT NULL,
        [CondicionPago] nvarchar(20) NOT NULL,
        [CostoEstimadoConfirmado] decimal(14,2) NOT NULL,
        [MargenEstimadoConfirmado] decimal(14,2) NOT NULL,
        [MotivoCancelacion] nvarchar(500) NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [CotizacionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Pedidos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pedidos_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pedidos_Cotizaciones_CotizacionId] FOREIGN KEY ([CotizacionId]) REFERENCES [Cotizaciones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [ControlesSanitarios] (
        [Id] uniqueidentifier NOT NULL,
        [AnimalId] uniqueidentifier NOT NULL,
        [Resultado] nvarchar(40) NOT NULL,
        [Observacion] nvarchar(1000) NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_ControlesSanitarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ControlesSanitarios_Animales_AnimalId] FOREIGN KEY ([AnimalId]) REFERENCES [Animales] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ControlesSanitarios_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [OrdenesProduccion] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [Tipo] nvarchar(40) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        [AnimalId] uniqueidentifier NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [InicioPrevisto] datetime2 NOT NULL,
        [FinPrevisto] datetime2 NOT NULL,
        [InicioReal] datetime2 NULL,
        [FinReal] datetime2 NULL,
        [Observacion] nvarchar(1000) NOT NULL,
        [AnimalEnOrden] AS CASE WHEN [Estado] <> 'ANULADA' THEN [AnimalId] ELSE NULL END PERSISTED,
        CONSTRAINT [PK_OrdenesProduccion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrdenesProduccion_Animales_AnimalId] FOREIGN KEY ([AnimalId]) REFERENCES [Animales] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenesProduccion_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Pagos] (
        [Id] uniqueidentifier NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [ReferenciaUnica] nvarchar(80) NOT NULL,
        [Importe] decimal(12,2) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Medio] nvarchar(80) NOT NULL,
        [Tipo] nvarchar(40) NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Anulado] bit NOT NULL,
        [MotivoAnulacion] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_Pagos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pagos_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pagos_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [RecuperacionesAcceso] (
        [Id] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(64) NOT NULL,
        [SolicitadoEn] datetime2 NOT NULL,
        [ExpiraEn] datetime2 NOT NULL,
        [UtilizadoEn] datetime2 NULL,
        [Invalidado] bit NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RecuperacionesAcceso] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecuperacionesAcceso_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [RegistrosAuditoria] (
        [Id] uniqueidentifier NOT NULL,
        [Operacion] nvarchar(1200) NOT NULL,
        [Entidad] nvarchar(80) NOT NULL,
        [EntidadId] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RegistrosAuditoria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RegistrosAuditoria_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Rutas] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [TransporteId] uniqueidentifier NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Zona] nvarchar(180) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        [SalidaReal] datetime2 NULL,
        [RetornoReal] datetime2 NULL,
        CONSTRAINT [PK_Rutas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Rutas_Transportes_TransporteId] FOREIGN KEY ([TransporteId]) REFERENCES [Transportes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Rutas_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Sesiones] (
        [Id] uniqueidentifier NOT NULL,
        [Inicio] datetime2 NOT NULL,
        [Expira] datetime2 NOT NULL,
        [Revocada] bit NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Sesiones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sesiones_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [UsuariosRoles] (
        [RolId] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UsuariosRoles] PRIMARY KEY ([RolId], [UsuarioId]),
        CONSTRAINT [FK_UsuariosRoles_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UsuariosRoles_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [ComprobantesFiscales] (
        [Id] uniqueidentifier NOT NULL,
        [PedidoId] uniqueidentifier NOT NULL,
        [ReferenciaUnica] nvarchar(80) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        [NumeroFiscal] nvarchar(80) NOT NULL,
        [RespuestaProveedor] nvarchar(1000) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ComprobantesFiscales] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprobantesFiscales_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ComprobantesFiscales_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [CondicionesPago] (
        [Id] uniqueidentifier NOT NULL,
        [PedidoId] uniqueidentifier NOT NULL,
        [Modalidad] nvarchar(40) NOT NULL,
        [ImporteAutorizado] decimal(12,2) NOT NULL,
        [FechaAcuerdo] datetime2 NOT NULL,
        [Vencimiento] datetime2 NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Activa] bit NOT NULL,
        [PedidoActivo] AS CASE WHEN [Activa] = 1 THEN [PedidoId] ELSE NULL END PERSISTED,
        CONSTRAINT [PK_CondicionesPago] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CondicionesPago_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CondicionesPago_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [DetallesPedido] (
        [Id] uniqueidentifier NOT NULL,
        [CantidadSolicitada] decimal(12,3) NOT NULL,
        [CantidadConfirmada] decimal(12,3) NOT NULL,
        [Unidad] nvarchar(10) NOT NULL,
        [PrecioAcordado] decimal(12,2) NOT NULL,
        [CostoReferenciaAcordado] decimal(12,2) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [PedidoId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_DetallesPedido] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesPedido_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesPedido_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Lotes] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(80) NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [OrdenProduccionId] uniqueidentifier NULL,
        [FechaProduccion] datetime2 NOT NULL,
        [Vencimiento] datetime2 NOT NULL,
        [Unidad] nvarchar(20) NOT NULL,
        [Procedencia] nvarchar(500) NOT NULL,
        [ReferenciaExterna] nvarchar(180) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        CONSTRAINT [PK_Lotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Lotes_OrdenesProduccion_OrdenProduccionId] FOREIGN KEY ([OrdenProduccionId]) REFERENCES [OrdenesProduccion] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Lotes_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [PrevisionesProducto] (
        [Id] uniqueidentifier NOT NULL,
        [OrdenProduccionId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [CantidadEstimada] decimal(12,3) NOT NULL,
        [Unidad] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_PrevisionesProducto] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrevisionesProducto_OrdenesProduccion_OrdenProduccionId] FOREIGN KEY ([OrdenProduccionId]) REFERENCES [OrdenesProduccion] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PrevisionesProducto_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Senas] (
        [Id] uniqueidentifier NOT NULL,
        [PagoId] uniqueidentifier NOT NULL,
        [PedidoId] uniqueidentifier NOT NULL,
        [AcuerdoResolucion] nvarchar(1000) NOT NULL,
        [ResueltaEn] datetime2 NULL,
        CONSTRAINT [PK_Senas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Senas_Pagos_PagoId] FOREIGN KEY ([PagoId]) REFERENCES [Pagos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Senas_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Entregas] (
        [Id] uniqueidentifier NOT NULL,
        [PedidoId] uniqueidentifier NOT NULL,
        [RutaId] uniqueidentifier NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Modalidad] nvarchar(40) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        [Version] int NOT NULL,
        [FechaPrevista] datetime2 NOT NULL,
        [FechaReal] datetime2 NULL,
        [FechaConfirmacion] datetime2 NULL,
        [Receptor] nvarchar(180) NOT NULL,
        [ObservacionRecepcion] nvarchar(1000) NOT NULL,
        [OrdenEnRuta] int NOT NULL,
        CONSTRAINT [PK_Entregas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Entregas_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Entregas_Rutas_RutaId] FOREIGN KEY ([RutaId]) REFERENCES [Rutas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Entregas_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Cargos] (
        [Id] uniqueidentifier NOT NULL,
        [PedidoId] uniqueidentifier NOT NULL,
        [DetallePedidoId] uniqueidentifier NULL,
        [Tipo] nvarchar(40) NOT NULL,
        [Concepto] nvarchar(300) NOT NULL,
        [Importe] decimal(12,2) NOT NULL,
        [AcuerdoServicio] nvarchar(300) NOT NULL,
        [PrestadoEn] datetime2 NULL,
        [Fecha] datetime2 NOT NULL,
        [Anulado] bit NOT NULL,
        [MotivoAnulacion] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_Cargos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cargos_DetallesPedido_DetallePedidoId] FOREIGN KEY ([DetallePedidoId]) REFERENCES [DetallesPedido] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Cargos_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Existencias] (
        [Id] uniqueidentifier NOT NULL,
        [LoteId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [Ubicacion] nvarchar(180) NOT NULL,
        [CantidadFisica] decimal(12,3) NOT NULL,
        CONSTRAINT [PK_Existencias] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Existencias_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Existencias_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Incidencias] (
        [Id] uniqueidentifier NOT NULL,
        [LoteId] uniqueidentifier NULL,
        [AnimalId] uniqueidentifier NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Motivo] nvarchar(1000) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Resuelta] bit NOT NULL,
        [Resolucion] nvarchar(1000) NOT NULL,
        [ResueltaEn] datetime2 NULL,
        CONSTRAINT [PK_Incidencias] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Incidencias_Animales_AnimalId] FOREIGN KEY ([AnimalId]) REFERENCES [Animales] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Incidencias_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Incidencias_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Pesajes] (
        [Id] uniqueidentifier NOT NULL,
        [LoteId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [PesoBruto] decimal(12,3) NOT NULL,
        [Tara] decimal(12,3) NOT NULL,
        [Neto] decimal(12,3) NOT NULL,
        [Unidad] nvarchar(20) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Observacion] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_Pesajes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pesajes_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pesajes_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pesajes_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [ResultadosProduccion] (
        [Id] uniqueidentifier NOT NULL,
        [OrdenProduccionId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [LoteId] uniqueidentifier NOT NULL,
        [CantidadReal] decimal(12,3) NULL,
        [Estado] nvarchar(40) NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        CONSTRAINT [PK_ResultadosProduccion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResultadosProduccion_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ResultadosProduccion_OrdenesProduccion_OrdenProduccionId] FOREIGN KEY ([OrdenProduccionId]) REFERENCES [OrdenesProduccion] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ResultadosProduccion_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [ControlesFrio] (
        [Id] uniqueidentifier NOT NULL,
        [EntregaId] uniqueidentifier NULL,
        [LoteId] uniqueidentifier NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Unidad] nvarchar(8) NOT NULL,
        [Temperatura] decimal(12,2) NOT NULL,
        [Ubicacion] nvarchar(180) NOT NULL,
        [Evaluacion] nvarchar(40) NOT NULL,
        [Observacion] nvarchar(1000) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_ControlesFrio] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ControlesFrio_Entregas_EntregaId] FOREIGN KEY ([EntregaId]) REFERENCES [Entregas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ControlesFrio_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ControlesFrio_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Recepciones] (
        [Id] uniqueidentifier NOT NULL,
        [EntregaId] uniqueidentifier NOT NULL,
        [ClaveUnica] uniqueidentifier NOT NULL,
        [VersionBase] int NOT NULL,
        [AutorId] uniqueidentifier NOT NULL,
        [FechaReal] datetime2 NOT NULL,
        [FechaSincronizacion] datetime2 NOT NULL,
        [PesoRecibido] decimal(12,3) NOT NULL,
        [Unidad] nvarchar(20) NOT NULL,
        [SaldoPendienteAlConfirmar] decimal(12,2) NULL,
        [Conforme] bit NOT NULL,
        [Receptor] nvarchar(180) NOT NULL,
        [Observacion] nvarchar(1000) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        [HuellaContenido] nvarchar(64) NOT NULL,
        [MotivoConflicto] nvarchar(1000) NOT NULL,
        [RevisadaPorId] uniqueidentifier NULL,
        [RevisadaEn] datetime2 NULL,
        [Resolucion] nvarchar(1000) NOT NULL,
        [EntregaConfirmada] AS CASE WHEN [Estado] = 'CONFIRMADA' THEN [EntregaId] ELSE NULL END PERSISTED,
        CONSTRAINT [PK_Recepciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Recepciones_Entregas_EntregaId] FOREIGN KEY ([EntregaId]) REFERENCES [Entregas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Recepciones_Usuarios_AutorId] FOREIGN KEY ([AutorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Recepciones_Usuarios_RevisadaPorId] FOREIGN KEY ([RevisadaPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [AplicacionesPago] (
        [Id] uniqueidentifier NOT NULL,
        [PagoId] uniqueidentifier NOT NULL,
        [CargoId] uniqueidentifier NOT NULL,
        [SenaId] uniqueidentifier NULL,
        [Importe] decimal(12,2) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_AplicacionesPago] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AplicacionesPago_Cargos_CargoId] FOREIGN KEY ([CargoId]) REFERENCES [Cargos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AplicacionesPago_Pagos_PagoId] FOREIGN KEY ([PagoId]) REFERENCES [Pagos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AplicacionesPago_Senas_SenaId] FOREIGN KEY ([SenaId]) REFERENCES [Senas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [InsumosProduccion] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenciaUnica] uniqueidentifier NOT NULL,
        [OrdenProduccionId] uniqueidentifier NOT NULL,
        [ExistenciaId] uniqueidentifier NOT NULL,
        [Cantidad] decimal(12,3) NOT NULL,
        [Unidad] nvarchar(20) NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_InsumosProduccion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InsumosProduccion_Existencias_ExistenciaId] FOREIGN KEY ([ExistenciaId]) REFERENCES [Existencias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InsumosProduccion_OrdenesProduccion_OrdenProduccionId] FOREIGN KEY ([OrdenProduccionId]) REFERENCES [OrdenesProduccion] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InsumosProduccion_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [MovimientosStock] (
        [Id] uniqueidentifier NOT NULL,
        [ExistenciaId] uniqueidentifier NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [ReferenciaId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(40) NOT NULL,
        [Cantidad] decimal(12,3) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Motivo] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_MovimientosStock] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientosStock_Existencias_ExistenciaId] FOREIGN KEY ([ExistenciaId]) REFERENCES [Existencias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosStock_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [Reservas] (
        [Id] uniqueidentifier NOT NULL,
        [DetallePedidoId] uniqueidentifier NOT NULL,
        [ExistenciaId] uniqueidentifier NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Cantidad] decimal(12,3) NOT NULL,
        [Estado] nvarchar(40) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [MotivoLiberacion] nvarchar(1000) NOT NULL,
        [DetalleActivo] AS CASE WHEN [Estado] = 'ACTIVA' THEN [DetallePedidoId] ELSE NULL END PERSISTED,
        CONSTRAINT [PK_Reservas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reservas_DetallesPedido_DetallePedidoId] FOREIGN KEY ([DetallePedidoId]) REFERENCES [DetallesPedido] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reservas_Existencias_ExistenciaId] FOREIGN KEY ([ExistenciaId]) REFERENCES [Existencias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reservas_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [EntradasInventario] (
        [Id] uniqueidentifier NOT NULL,
        [LoteId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [PesajeId] uniqueidentifier NOT NULL,
        [ExistenciaId] uniqueidentifier NOT NULL,
        [ResponsableId] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Cantidad] decimal(12,3) NOT NULL,
        [Anulada] bit NOT NULL,
        [AnuladaPorId] uniqueidentifier NULL,
        [AnuladaEn] datetime2 NULL,
        [MotivoAnulacion] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_EntradasInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EntradasInventario_Existencias_ExistenciaId] FOREIGN KEY ([ExistenciaId]) REFERENCES [Existencias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntradasInventario_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntradasInventario_Pesajes_PesajeId] FOREIGN KEY ([PesajeId]) REFERENCES [Pesajes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntradasInventario_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntradasInventario_Usuarios_AnuladaPorId] FOREIGN KEY ([AnuladaPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EntradasInventario_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE TABLE [EvidenciasRecepcion] (
        [Id] uniqueidentifier NOT NULL,
        [RecepcionId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(40) NOT NULL,
        [Contenido] varbinary(max) NOT NULL,
        [Sha256] nvarchar(64) NOT NULL,
        CONSTRAINT [PK_EvidenciasRecepcion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EvidenciasRecepcion_Recepciones_RecepcionId] FOREIGN KEY ([RecepcionId]) REFERENCES [Recepciones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Animales_Identificacion] ON [Animales] ([Identificacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_AplicacionesPago_CargoId] ON [AplicacionesPago] ([CargoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AplicacionesPago_PagoId_CargoId] ON [AplicacionesPago] ([PagoId], [CargoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AplicacionesPago_SenaId] ON [AplicacionesPago] ([SenaId]) WHERE [SenaId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Cargos_DetallePedidoId] ON [Cargos] ([DetallePedidoId]) WHERE [DetallePedidoId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Cargos_PedidoId] ON [Cargos] ([PedidoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ClasificacionesProducto_Codigo] ON [ClasificacionesProducto] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clientes_IdentificacionFiscalUnica] ON [Clientes] ([IdentificacionFiscal]) WHERE [Activo] = 1 AND [IdentificacionFiscal] <> ''''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprobantesFiscales_PedidoId] ON [ComprobantesFiscales] ([PedidoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprobantesFiscales_ReferenciaUnica] ON [ComprobantesFiscales] ([ReferenciaUnica]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ComprobantesFiscales_ResponsableId] ON [ComprobantesFiscales] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CondicionesPago_PedidoActivo] ON [CondicionesPago] ([PedidoId]) WHERE [Activa] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_CondicionesPago_ResponsableId] ON [CondicionesPago] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ControlesFrio_EntregaId] ON [ControlesFrio] ([EntregaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ControlesFrio_LoteId] ON [ControlesFrio] ([LoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ControlesFrio_ResponsableId] ON [ControlesFrio] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ControlesSanitarios_AnimalId_Fecha] ON [ControlesSanitarios] ([AnimalId], [Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ControlesSanitarios_ResponsableId] ON [ControlesSanitarios] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Cotizaciones_ClienteId] ON [Cotizaciones] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cotizaciones_Codigo] ON [Cotizaciones] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Cotizaciones_CotizacionOrigenId] ON [Cotizaciones] ([CotizacionOrigenId]) WHERE [CotizacionOrigenId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_DetallesCotizacion_CotizacionId] ON [DetallesCotizacion] ([CotizacionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_DetallesCotizacion_ProductoId] ON [DetallesCotizacion] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_DetallesPedido_PedidoId] ON [DetallesPedido] ([PedidoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_DetallesPedido_ProductoId] ON [DetallesPedido] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_EntradasInventario_AnuladaPorId] ON [EntradasInventario] ([AnuladaPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_EntradasInventario_ExistenciaId] ON [EntradasInventario] ([ExistenciaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EntradasInventario_LoteId_ProductoId] ON [EntradasInventario] ([LoteId], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_EntradasInventario_PesajeId] ON [EntradasInventario] ([PesajeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_EntradasInventario_ProductoId] ON [EntradasInventario] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_EntradasInventario_ResponsableId] ON [EntradasInventario] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Entregas_PedidoId] ON [Entregas] ([PedidoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Entregas_ResponsableId_Estado] ON [Entregas] ([ResponsableId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Entregas_RutaId] ON [Entregas] ([RutaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_EvidenciasRecepcion_RecepcionId] ON [EvidenciasRecepcion] ([RecepcionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Existencias_LoteId_ProductoId] ON [Existencias] ([LoteId], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Existencias_ProductoId] ON [Existencias] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Incidencias_AnimalId] ON [Incidencias] ([AnimalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Incidencias_LoteId] ON [Incidencias] ([LoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Incidencias_ResponsableId] ON [Incidencias] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_InsumosProduccion_ExistenciaId] ON [InsumosProduccion] ([ExistenciaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_InsumosProduccion_OrdenProduccionId] ON [InsumosProduccion] ([OrdenProduccionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InsumosProduccion_ReferenciaUnica] ON [InsumosProduccion] ([ReferenciaUnica]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_InsumosProduccion_ResponsableId] ON [InsumosProduccion] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lotes_Codigo] ON [Lotes] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Lotes_OrdenProduccionId] ON [Lotes] ([OrdenProduccionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Lotes_ProductoId] ON [Lotes] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_MovimientosStock_ExistenciaId] ON [MovimientosStock] ([ExistenciaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_MovimientosStock_ResponsableId] ON [MovimientosStock] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MovimientosStock_Tipo_ReferenciaId] ON [MovimientosStock] ([Tipo], [ReferenciaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OrdenesProduccion_AnimalEnOrden] ON [OrdenesProduccion] ([AnimalId]) WHERE [Estado] <> ''ANULADA'' AND [AnimalId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrdenesProduccion_Codigo] ON [OrdenesProduccion] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_OrdenesProduccion_ResponsableId] ON [OrdenesProduccion] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Pagos_ClienteId_Fecha] ON [Pagos] ([ClienteId], [Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pagos_ReferenciaUnica] ON [Pagos] ([ReferenciaUnica]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Pagos_ResponsableId] ON [Pagos] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Pedidos_ClienteId] ON [Pedidos] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pedidos_Codigo] ON [Pedidos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pedidos_CotizacionId] ON [Pedidos] ([CotizacionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permisos_Codigo] ON [Permisos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pesajes_LoteId] ON [Pesajes] ([LoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Pesajes_ProductoId] ON [Pesajes] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Pesajes_ResponsableId] ON [Pesajes] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PreciosProducto_ProductoConVigenciaAbierta] ON [PreciosProducto] ([ProductoId]) WHERE [VigenteHasta] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PreciosProducto_ProductoId_VigenteDesde] ON [PreciosProducto] ([ProductoId], [VigenteDesde]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PrevisionesProducto_OrdenProduccionId_ProductoId] ON [PrevisionesProducto] ([OrdenProduccionId], [ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_PrevisionesProducto_ProductoId] ON [PrevisionesProducto] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Productos_ClasificacionProductoId] ON [Productos] ([ClasificacionProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [Productos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Recepciones_AutorId] ON [Recepciones] ([AutorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Recepciones_ClaveUnica] ON [Recepciones] ([ClaveUnica]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Recepciones_EntregaConfirmada] ON [Recepciones] ([EntregaId]) WHERE [Estado] = ''CONFIRMADA''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Recepciones_RevisadaPorId] ON [Recepciones] ([RevisadaPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RecuperacionesAcceso_TokenHash] ON [RecuperacionesAcceso] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_RecuperacionesAcceso_UsuarioId] ON [RecuperacionesAcceso] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_RegistrosAuditoria_UsuarioId] ON [RegistrosAuditoria] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Reservas_DetalleActivo_ExistenciaId] ON [Reservas] ([DetallePedidoId], [ExistenciaId]) WHERE [Estado] = ''ACTIVA''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Reservas_ExistenciaId] ON [Reservas] ([ExistenciaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Reservas_ResponsableId] ON [Reservas] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ResultadosProduccion_LoteId] ON [ResultadosProduccion] ([LoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ResultadosProduccion_OrdenProduccionId] ON [ResultadosProduccion] ([OrdenProduccionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_ResultadosProduccion_ProductoId] ON [ResultadosProduccion] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Nombre] ON [Roles] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_RolesPermisos_RolId] ON [RolesPermisos] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rutas_Codigo] ON [Rutas] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Rutas_ResponsableId] ON [Rutas] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Rutas_TransporteId] ON [Rutas] ([TransporteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Senas_PagoId] ON [Senas] ([PagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Senas_PedidoId] ON [Senas] ([PedidoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Sesiones_UsuarioId_Revocada] ON [Sesiones] ([UsuarioId], [Revocada]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Transportes_Codigo] ON [Transportes] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_Usuarios_ClienteId] ON [Usuarios] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Correo] ON [Usuarios] ([Correo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Login] ON [Usuarios] ([Login]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_UsuariosRoles_UsuarioId] ON [UsuariosRoles] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118035108_InicialSqlServer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261118035108_InicialSqlServer', N'10.0.12');
END;

COMMIT;
GO

