CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `ClasificacionesProducto` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(20) NOT NULL,
    `Nombre` varchar(100) NOT NULL,
    `Familia` varchar(20) NOT NULL,
    `Descripcion` varchar(400) NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Clientes` (
    `Id` char(36) NOT NULL,
    `Nombre` varchar(120) NOT NULL,
    `Tipo` varchar(60) NOT NULL,
    `Telefono` varchar(30) NOT NULL,
    `Direccion` varchar(250) NOT NULL,
    `IdentificacionFiscal` varchar(40) NOT NULL,
    `CondicionHabitual` varchar(20) NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    `FechaAlta` datetime(6) NOT NULL,
    `Observaciones` varchar(500) NOT NULL,
    `IdentificacionFiscalUnica` varchar(40) AS (CASE WHEN `Activo` = 1 AND `IdentificacionFiscal` <> '' THEN `IdentificacionFiscal` ELSE NULL END) NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Permisos` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(80) NOT NULL,
    `Descripcion` varchar(200) NOT NULL,
    `Modulo` varchar(40) NOT NULL,
    `Accion` varchar(40) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Roles` (
    `Id` char(36) NOT NULL,
    `Nombre` varchar(40) NOT NULL,
    `Descripcion` varchar(200) NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Productos` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(20) NOT NULL,
    `Nombre` varchar(120) NOT NULL,
    `Presentacion` varchar(60) NOT NULL,
    `UnidadVenta` varchar(10) NOT NULL,
    `Descripcion` varchar(400) NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    `FechaAlta` datetime(6) NOT NULL,
    `ClasificacionProductoId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Productos_ClasificacionesProducto_ClasificacionProductoId` FOREIGN KEY (`ClasificacionProductoId`) REFERENCES `ClasificacionesProducto` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Cotizaciones` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(20) NOT NULL,
    `Version` int NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `VigenteHasta` datetime(6) NOT NULL,
    `Canal` varchar(20) NOT NULL,
    `Estado` varchar(20) NOT NULL,
    `MotivoRespuesta` varchar(500) NOT NULL,
    `ClienteId` char(36) NOT NULL,
    `CotizacionOrigenId` char(36) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Cotizaciones_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Cotizaciones_Cotizaciones_CotizacionOrigenId` FOREIGN KEY (`CotizacionOrigenId`) REFERENCES `Cotizaciones` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Usuarios` (
    `Id` char(36) NOT NULL,
    `Nombre` varchar(120) NOT NULL,
    `Login` varchar(80) NOT NULL,
    `HashClave` varchar(500) NOT NULL,
    `Estado` varchar(20) NOT NULL,
    `IntentosFallidos` int NOT NULL,
    `Correo` varchar(180) NOT NULL,
    `Telefono` varchar(30) NULL,
    `CreadoEn` datetime(6) NOT NULL,
    `UltimoAcceso` datetime(6) NULL,
    `ClienteId` char(36) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Usuarios_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `RolesPermisos` (
    `PermisoId` char(36) NOT NULL,
    `RolId` char(36) NOT NULL,
    PRIMARY KEY (`PermisoId`, `RolId`),
    CONSTRAINT `FK_RolesPermisos_Permisos_PermisoId` FOREIGN KEY (`PermisoId`) REFERENCES `Permisos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_RolesPermisos_Roles_RolId` FOREIGN KEY (`RolId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PreciosProducto` (
    `Id` char(36) NOT NULL,
    `CostoReferencia` decimal(12,2) NOT NULL,
    `PrecioVenta` decimal(12,2) NOT NULL,
    `VigenteDesde` datetime(6) NOT NULL,
    `VigenteHasta` datetime(6) NULL,
    `ProductoId` char(36) NOT NULL,
    `ProductoConVigenciaAbierta` char(36) AS (CASE WHEN `VigenteHasta` IS NULL THEN `ProductoId` ELSE NULL END) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PreciosProducto_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `DetallesCotizacion` (
    `Id` char(36) NOT NULL,
    `Cantidad` decimal(12,3) NOT NULL,
    `Unidad` varchar(10) NOT NULL,
    `PrecioUnitario` decimal(12,2) NOT NULL,
    `Subtotal` decimal(14,2) NOT NULL,
    `CotizacionId` char(36) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DetallesCotizacion_Cotizaciones_CotizacionId` FOREIGN KEY (`CotizacionId`) REFERENCES `Cotizaciones` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_DetallesCotizacion_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Pedidos` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(20) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `FechaComprometida` datetime(6) NOT NULL,
    `EstadoComercial` varchar(20) NOT NULL,
    `EstadoPreparacion` varchar(20) NOT NULL,
    `CondicionPago` varchar(20) NOT NULL,
    `CostoEstimadoConfirmado` decimal(14,2) NOT NULL,
    `MargenEstimadoConfirmado` decimal(14,2) NOT NULL,
    `MotivoCancelacion` varchar(500) NOT NULL,
    `ClienteId` char(36) NOT NULL,
    `CotizacionId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Pedidos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Pedidos_Cotizaciones_CotizacionId` FOREIGN KEY (`CotizacionId`) REFERENCES `Cotizaciones` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `RecuperacionesAcceso` (
    `Id` char(36) NOT NULL,
    `TokenHash` varchar(64) NOT NULL,
    `SolicitadoEn` datetime(6) NOT NULL,
    `ExpiraEn` datetime(6) NOT NULL,
    `UtilizadoEn` datetime(6) NULL,
    `Invalidado` tinyint(1) NOT NULL,
    `UsuarioId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RecuperacionesAcceso_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `RegistrosAuditoria` (
    `Id` char(36) NOT NULL,
    `Operacion` varchar(1200) NOT NULL,
    `Entidad` varchar(80) NOT NULL,
    `EntidadId` char(36) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `UsuarioId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RegistrosAuditoria_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Sesiones` (
    `Id` char(36) NOT NULL,
    `Inicio` datetime(6) NOT NULL,
    `Expira` datetime(6) NOT NULL,
    `Revocada` tinyint(1) NOT NULL,
    `UsuarioId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Sesiones_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `UsuariosRoles` (
    `RolId` char(36) NOT NULL,
    `UsuarioId` char(36) NOT NULL,
    PRIMARY KEY (`RolId`, `UsuarioId`),
    CONSTRAINT `FK_UsuariosRoles_Roles_RolId` FOREIGN KEY (`RolId`) REFERENCES `Roles` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_UsuariosRoles_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `DetallesPedido` (
    `Id` char(36) NOT NULL,
    `CantidadSolicitada` decimal(12,3) NOT NULL,
    `CantidadConfirmada` decimal(12,3) NOT NULL,
    `Unidad` varchar(10) NOT NULL,
    `PrecioAcordado` decimal(12,2) NOT NULL,
    `CostoReferenciaAcordado` decimal(12,2) NOT NULL,
    `Estado` varchar(20) NOT NULL,
    `PedidoId` char(36) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DetallesPedido_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_DetallesPedido_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX `IX_ClasificacionesProducto_Codigo` ON `ClasificacionesProducto` (`Codigo`);

CREATE UNIQUE INDEX `IX_Clientes_IdentificacionFiscalUnica` ON `Clientes` (`IdentificacionFiscalUnica`);

CREATE INDEX `IX_Cotizaciones_ClienteId` ON `Cotizaciones` (`ClienteId`);

CREATE UNIQUE INDEX `IX_Cotizaciones_Codigo` ON `Cotizaciones` (`Codigo`);

CREATE UNIQUE INDEX `IX_Cotizaciones_CotizacionOrigenId` ON `Cotizaciones` (`CotizacionOrigenId`);

CREATE INDEX `IX_DetallesCotizacion_CotizacionId` ON `DetallesCotizacion` (`CotizacionId`);

CREATE INDEX `IX_DetallesCotizacion_ProductoId` ON `DetallesCotizacion` (`ProductoId`);

CREATE INDEX `IX_DetallesPedido_PedidoId` ON `DetallesPedido` (`PedidoId`);

CREATE INDEX `IX_DetallesPedido_ProductoId` ON `DetallesPedido` (`ProductoId`);

CREATE INDEX `IX_Pedidos_ClienteId` ON `Pedidos` (`ClienteId`);

CREATE UNIQUE INDEX `IX_Pedidos_Codigo` ON `Pedidos` (`Codigo`);

CREATE UNIQUE INDEX `IX_Pedidos_CotizacionId` ON `Pedidos` (`CotizacionId`);

CREATE UNIQUE INDEX `IX_Permisos_Codigo` ON `Permisos` (`Codigo`);

CREATE UNIQUE INDEX `IX_PreciosProducto_ProductoConVigenciaAbierta` ON `PreciosProducto` (`ProductoConVigenciaAbierta`);

CREATE UNIQUE INDEX `IX_PreciosProducto_ProductoId_VigenteDesde` ON `PreciosProducto` (`ProductoId`, `VigenteDesde`);

CREATE INDEX `IX_Productos_ClasificacionProductoId` ON `Productos` (`ClasificacionProductoId`);

CREATE UNIQUE INDEX `IX_Productos_Codigo` ON `Productos` (`Codigo`);

CREATE UNIQUE INDEX `IX_RecuperacionesAcceso_TokenHash` ON `RecuperacionesAcceso` (`TokenHash`);

CREATE INDEX `IX_RecuperacionesAcceso_UsuarioId` ON `RecuperacionesAcceso` (`UsuarioId`);

CREATE INDEX `IX_RegistrosAuditoria_UsuarioId` ON `RegistrosAuditoria` (`UsuarioId`);

CREATE UNIQUE INDEX `IX_Roles_Nombre` ON `Roles` (`Nombre`);

CREATE INDEX `IX_RolesPermisos_RolId` ON `RolesPermisos` (`RolId`);

CREATE INDEX `IX_Sesiones_UsuarioId_Revocada` ON `Sesiones` (`UsuarioId`, `Revocada`);

CREATE INDEX `IX_Usuarios_ClienteId` ON `Usuarios` (`ClienteId`);

CREATE UNIQUE INDEX `IX_Usuarios_Correo` ON `Usuarios` (`Correo`);

CREATE UNIQUE INDEX `IX_Usuarios_Login` ON `Usuarios` (`Login`);

CREATE INDEX `IX_UsuariosRoles_UsuarioId` ON `UsuariosRoles` (`UsuarioId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261106130825_InicialSprint1', '10.0.12');

CREATE TABLE `Animales` (
    `Id` char(36) NOT NULL,
    `Identificacion` varchar(80) NOT NULL,
    `Procedencia` varchar(300) NOT NULL,
    `Proveedor` varchar(180) NOT NULL,
    `PesoIngreso` decimal(12,3) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    `FechaIngreso` datetime(6) NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Cargos` (
    `Id` char(36) NOT NULL,
    `PedidoId` char(36) NOT NULL,
    `DetallePedidoId` char(36) NULL,
    `Tipo` varchar(40) NOT NULL,
    `Concepto` varchar(300) NOT NULL,
    `Importe` decimal(12,2) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `Anulado` tinyint(1) NOT NULL,
    `MotivoAnulacion` varchar(1000) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Cargos_DetallesPedido_DetallePedidoId` FOREIGN KEY (`DetallePedidoId`) REFERENCES `DetallesPedido` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Cargos_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `ControlesSanitarios` (
    `Id` char(36) NOT NULL,
    `AnimalId` char(36) NOT NULL,
    `Resultado` varchar(40) NOT NULL,
    `Observacion` varchar(1000) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ControlesSanitarios_Animales_AnimalId` FOREIGN KEY (`AnimalId`) REFERENCES `Animales` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ControlesSanitarios_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `OrdenesProduccion` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(80) NOT NULL,
    `Tipo` varchar(40) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    `AnimalId` char(36) NULL,
    `ResponsableId` char(36) NOT NULL,
    `FechaCreacion` datetime(6) NOT NULL,
    `InicioPrevisto` datetime(6) NOT NULL,
    `FinPrevisto` datetime(6) NOT NULL,
    `InicioReal` datetime(6) NULL,
    `FinReal` datetime(6) NULL,
    `Observacion` varchar(1000) NOT NULL,
    `AnimalEnOrden` char(36) AS (CASE WHEN `Estado` <> 'ANULADA' THEN `AnimalId` ELSE NULL END) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OrdenesProduccion_Animales_AnimalId` FOREIGN KEY (`AnimalId`) REFERENCES `Animales` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_OrdenesProduccion_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Lotes` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(80) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    `OrdenProduccionId` char(36) NULL,
    `FechaProduccion` datetime(6) NOT NULL,
    `Vencimiento` datetime(6) NOT NULL,
    `Unidad` varchar(20) NOT NULL,
    `Procedencia` varchar(500) NOT NULL,
    `ReferenciaExterna` varchar(180) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Lotes_OrdenesProduccion_OrdenProduccionId` FOREIGN KEY (`OrdenProduccionId`) REFERENCES `OrdenesProduccion` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Lotes_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `ControlesFrio` (
    `Id` char(36) NOT NULL,
    `LoteId` char(36) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `Temperatura` decimal(12,2) NOT NULL,
    `Ubicacion` varchar(180) NOT NULL,
    `Evaluacion` varchar(40) NOT NULL,
    `Observacion` varchar(1000) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ControlesFrio_Lotes_LoteId` FOREIGN KEY (`LoteId`) REFERENCES `Lotes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ControlesFrio_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Existencias` (
    `Id` char(36) NOT NULL,
    `LoteId` char(36) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    `Ubicacion` varchar(180) NOT NULL,
    `CantidadFisica` decimal(12,3) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Existencias_Lotes_LoteId` FOREIGN KEY (`LoteId`) REFERENCES `Lotes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Existencias_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Incidencias` (
    `Id` char(36) NOT NULL,
    `LoteId` char(36) NULL,
    `AnimalId` char(36) NULL,
    `ResponsableId` char(36) NOT NULL,
    `Motivo` varchar(1000) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `Resuelta` tinyint(1) NOT NULL,
    `Resolucion` varchar(1000) NOT NULL,
    `ResueltaEn` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Incidencias_Animales_AnimalId` FOREIGN KEY (`AnimalId`) REFERENCES `Animales` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Incidencias_Lotes_LoteId` FOREIGN KEY (`LoteId`) REFERENCES `Lotes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Incidencias_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Pesajes` (
    `Id` char(36) NOT NULL,
    `LoteId` char(36) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `PesoBruto` decimal(12,3) NOT NULL,
    `Tara` decimal(12,3) NOT NULL,
    `Neto` decimal(12,3) NOT NULL,
    `Unidad` varchar(20) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `Observacion` varchar(1000) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Pesajes_Lotes_LoteId` FOREIGN KEY (`LoteId`) REFERENCES `Lotes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Pesajes_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Pesajes_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `ResultadosProduccion` (
    `Id` char(36) NOT NULL,
    `OrdenProduccionId` char(36) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    `LoteId` char(36) NOT NULL,
    `CantidadReal` decimal(12,3) NULL,
    `Estado` varchar(40) NOT NULL,
    `FechaRegistro` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ResultadosProduccion_Lotes_LoteId` FOREIGN KEY (`LoteId`) REFERENCES `Lotes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ResultadosProduccion_OrdenesProduccion_OrdenProduccionId` FOREIGN KEY (`OrdenProduccionId`) REFERENCES `OrdenesProduccion` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ResultadosProduccion_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `InsumosProduccion` (
    `Id` char(36) NOT NULL,
    `OrdenProduccionId` char(36) NOT NULL,
    `ExistenciaId` char(36) NOT NULL,
    `Cantidad` decimal(12,3) NOT NULL,
    `Unidad` varchar(20) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_InsumosProduccion_Existencias_ExistenciaId` FOREIGN KEY (`ExistenciaId`) REFERENCES `Existencias` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_InsumosProduccion_OrdenesProduccion_OrdenProduccionId` FOREIGN KEY (`OrdenProduccionId`) REFERENCES `OrdenesProduccion` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_InsumosProduccion_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `MovimientosStock` (
    `Id` char(36) NOT NULL,
    `ExistenciaId` char(36) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `ReferenciaId` char(36) NOT NULL,
    `Tipo` varchar(40) NOT NULL,
    `Cantidad` decimal(12,3) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `Motivo` varchar(1000) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_MovimientosStock_Existencias_ExistenciaId` FOREIGN KEY (`ExistenciaId`) REFERENCES `Existencias` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_MovimientosStock_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Reservas` (
    `Id` char(36) NOT NULL,
    `DetallePedidoId` char(36) NOT NULL,
    `ExistenciaId` char(36) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `Cantidad` decimal(12,3) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `MotivoLiberacion` varchar(1000) NOT NULL,
    `DetalleActivo` char(36) AS (CASE WHEN `Estado` = 'ACTIVA' THEN `DetallePedidoId` ELSE NULL END) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Reservas_DetallesPedido_DetallePedidoId` FOREIGN KEY (`DetallePedidoId`) REFERENCES `DetallesPedido` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservas_Existencias_ExistenciaId` FOREIGN KEY (`ExistenciaId`) REFERENCES `Existencias` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservas_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `EntradasInventario` (
    `Id` char(36) NOT NULL,
    `LoteId` char(36) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    `PesajeId` char(36) NOT NULL,
    `ExistenciaId` char(36) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `Cantidad` decimal(12,3) NOT NULL,
    `Anulada` tinyint(1) NOT NULL,
    `AnuladaPorId` char(36) NULL,
    `AnuladaEn` datetime(6) NULL,
    `MotivoAnulacion` varchar(1000) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EntradasInventario_Existencias_ExistenciaId` FOREIGN KEY (`ExistenciaId`) REFERENCES `Existencias` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_EntradasInventario_Lotes_LoteId` FOREIGN KEY (`LoteId`) REFERENCES `Lotes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_EntradasInventario_Pesajes_PesajeId` FOREIGN KEY (`PesajeId`) REFERENCES `Pesajes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_EntradasInventario_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_EntradasInventario_Usuarios_AnuladaPorId` FOREIGN KEY (`AnuladaPorId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_EntradasInventario_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX `IX_Animales_Identificacion` ON `Animales` (`Identificacion`);

CREATE UNIQUE INDEX `IX_Cargos_DetallePedidoId` ON `Cargos` (`DetallePedidoId`);

CREATE INDEX `IX_Cargos_PedidoId` ON `Cargos` (`PedidoId`);

CREATE INDEX `IX_ControlesFrio_LoteId` ON `ControlesFrio` (`LoteId`);

CREATE INDEX `IX_ControlesFrio_ResponsableId` ON `ControlesFrio` (`ResponsableId`);

CREATE INDEX `IX_ControlesSanitarios_AnimalId_Fecha` ON `ControlesSanitarios` (`AnimalId`, `Fecha`);

CREATE INDEX `IX_ControlesSanitarios_ResponsableId` ON `ControlesSanitarios` (`ResponsableId`);

CREATE INDEX `IX_EntradasInventario_AnuladaPorId` ON `EntradasInventario` (`AnuladaPorId`);

CREATE INDEX `IX_EntradasInventario_ExistenciaId` ON `EntradasInventario` (`ExistenciaId`);

CREATE UNIQUE INDEX `IX_EntradasInventario_LoteId_ProductoId` ON `EntradasInventario` (`LoteId`, `ProductoId`);

CREATE INDEX `IX_EntradasInventario_PesajeId` ON `EntradasInventario` (`PesajeId`);

CREATE INDEX `IX_EntradasInventario_ProductoId` ON `EntradasInventario` (`ProductoId`);

CREATE INDEX `IX_EntradasInventario_ResponsableId` ON `EntradasInventario` (`ResponsableId`);

CREATE UNIQUE INDEX `IX_Existencias_LoteId_ProductoId` ON `Existencias` (`LoteId`, `ProductoId`);

CREATE INDEX `IX_Existencias_ProductoId` ON `Existencias` (`ProductoId`);

CREATE INDEX `IX_Incidencias_AnimalId` ON `Incidencias` (`AnimalId`);

CREATE INDEX `IX_Incidencias_LoteId` ON `Incidencias` (`LoteId`);

CREATE INDEX `IX_Incidencias_ResponsableId` ON `Incidencias` (`ResponsableId`);

CREATE INDEX `IX_InsumosProduccion_ExistenciaId` ON `InsumosProduccion` (`ExistenciaId`);

CREATE INDEX `IX_InsumosProduccion_OrdenProduccionId` ON `InsumosProduccion` (`OrdenProduccionId`);

CREATE INDEX `IX_InsumosProduccion_ResponsableId` ON `InsumosProduccion` (`ResponsableId`);

CREATE UNIQUE INDEX `IX_Lotes_Codigo` ON `Lotes` (`Codigo`);

CREATE INDEX `IX_Lotes_OrdenProduccionId` ON `Lotes` (`OrdenProduccionId`);

CREATE INDEX `IX_Lotes_ProductoId` ON `Lotes` (`ProductoId`);

CREATE INDEX `IX_MovimientosStock_ExistenciaId` ON `MovimientosStock` (`ExistenciaId`);

CREATE INDEX `IX_MovimientosStock_ResponsableId` ON `MovimientosStock` (`ResponsableId`);

CREATE UNIQUE INDEX `IX_MovimientosStock_Tipo_ReferenciaId` ON `MovimientosStock` (`Tipo`, `ReferenciaId`);

CREATE UNIQUE INDEX `IX_OrdenesProduccion_AnimalEnOrden` ON `OrdenesProduccion` (`AnimalEnOrden`);

CREATE INDEX `IX_OrdenesProduccion_AnimalId` ON `OrdenesProduccion` (`AnimalId`);

CREATE UNIQUE INDEX `IX_OrdenesProduccion_Codigo` ON `OrdenesProduccion` (`Codigo`);

CREATE INDEX `IX_OrdenesProduccion_ResponsableId` ON `OrdenesProduccion` (`ResponsableId`);

CREATE UNIQUE INDEX `IX_Pesajes_LoteId` ON `Pesajes` (`LoteId`);

CREATE INDEX `IX_Pesajes_ProductoId` ON `Pesajes` (`ProductoId`);

CREATE INDEX `IX_Pesajes_ResponsableId` ON `Pesajes` (`ResponsableId`);

CREATE UNIQUE INDEX `IX_Reservas_DetalleActivo_ExistenciaId` ON `Reservas` (`DetalleActivo`, `ExistenciaId`);

CREATE INDEX `IX_Reservas_DetallePedidoId` ON `Reservas` (`DetallePedidoId`);

CREATE INDEX `IX_Reservas_ExistenciaId` ON `Reservas` (`ExistenciaId`);

CREATE INDEX `IX_Reservas_ResponsableId` ON `Reservas` (`ResponsableId`);

CREATE UNIQUE INDEX `IX_ResultadosProduccion_LoteId` ON `ResultadosProduccion` (`LoteId`);

CREATE INDEX `IX_ResultadosProduccion_OrdenProduccionId` ON `ResultadosProduccion` (`OrdenProduccionId`);

CREATE INDEX `IX_ResultadosProduccion_ProductoId` ON `ResultadosProduccion` (`ProductoId`);


INSERT INTO `Cargos` (`Id`, `PedidoId`, `DetallePedidoId`, `Tipo`, `Concepto`, `Importe`, `Fecha`, `Anulado`, `MotivoAnulacion`)
SELECT UUID(), d.`PedidoId`, d.`Id`, 'PRODUCTO', CONCAT('Producto del pedido ', p.`Codigo`),
       ROUND(SUM(d.`CantidadConfirmada` * d.`PrecioAcordado`) OVER
           (PARTITION BY d.`PedidoId` ORDER BY d.`Id` ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 2)
       - ROUND(COALESCE(SUM(d.`CantidadConfirmada` * d.`PrecioAcordado`) OVER
           (PARTITION BY d.`PedidoId` ORDER BY d.`Id` ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0), 2), p.`Fecha`, 0, ''
FROM `DetallesPedido` d INNER JOIN `Pedidos` p ON p.`Id` = d.`PedidoId`
WHERE p.`EstadoComercial` = 'CONFIRMADO';

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261107113912_Sprint2Produccion', '10.0.12');

ALTER TABLE `ControlesFrio` MODIFY `LoteId` char(36) NULL;

ALTER TABLE `ControlesFrio` ADD `EntregaId` char(36) NULL;

CREATE TABLE `CondicionesPago` (
    `Id` char(36) NOT NULL,
    `PedidoId` char(36) NOT NULL,
    `Modalidad` varchar(40) NOT NULL,
    `ImporteAutorizado` decimal(12,2) NOT NULL,
    `FechaAcuerdo` datetime(6) NOT NULL,
    `Vencimiento` datetime(6) NULL,
    `ResponsableId` char(36) NOT NULL,
    `Activa` tinyint(1) NOT NULL,
    `PedidoActivo` char(36) AS (CASE WHEN `Activa` = 1 THEN `PedidoId` ELSE NULL END) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_CondicionesPago_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_CondicionesPago_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Pagos` (
    `Id` char(36) NOT NULL,
    `ClienteId` char(36) NOT NULL,
    `ReferenciaUnica` varchar(80) NOT NULL,
    `Importe` decimal(12,2) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `Medio` varchar(80) NOT NULL,
    `Tipo` varchar(40) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `Anulado` tinyint(1) NOT NULL,
    `MotivoAnulacion` varchar(1000) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Pagos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Pagos_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Transportes` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(80) NOT NULL,
    `Modalidad` varchar(40) NOT NULL,
    `Transportista` varchar(180) NOT NULL,
    `Contacto` varchar(100) NOT NULL,
    `Placa` varchar(40) NOT NULL,
    `DescripcionVehiculo` varchar(300) NOT NULL,
    `Conservacion` varchar(300) NOT NULL,
    `Activo` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Senas` (
    `Id` char(36) NOT NULL,
    `PagoId` char(36) NOT NULL,
    `PedidoId` char(36) NOT NULL,
    `AcuerdoResolucion` varchar(1000) NOT NULL,
    `ResueltaEn` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Senas_Pagos_PagoId` FOREIGN KEY (`PagoId`) REFERENCES `Pagos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Senas_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Rutas` (
    `Id` char(36) NOT NULL,
    `Codigo` varchar(80) NOT NULL,
    `TransporteId` char(36) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `Zona` varchar(180) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    `SalidaReal` datetime(6) NULL,
    `RetornoReal` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Rutas_Transportes_TransporteId` FOREIGN KEY (`TransporteId`) REFERENCES `Transportes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Rutas_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `AplicacionesPago` (
    `Id` char(36) NOT NULL,
    `PagoId` char(36) NOT NULL,
    `CargoId` char(36) NOT NULL,
    `SenaId` char(36) NULL,
    `Importe` decimal(12,2) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AplicacionesPago_Cargos_CargoId` FOREIGN KEY (`CargoId`) REFERENCES `Cargos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_AplicacionesPago_Pagos_PagoId` FOREIGN KEY (`PagoId`) REFERENCES `Pagos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_AplicacionesPago_Senas_SenaId` FOREIGN KEY (`SenaId`) REFERENCES `Senas` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Entregas` (
    `Id` char(36) NOT NULL,
    `PedidoId` char(36) NOT NULL,
    `RutaId` char(36) NULL,
    `ResponsableId` char(36) NOT NULL,
    `Modalidad` varchar(40) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    `Version` int NOT NULL,
    `FechaPrevista` datetime(6) NOT NULL,
    `FechaReal` datetime(6) NULL,
    `FechaConfirmacion` datetime(6) NULL,
    `Receptor` varchar(180) NOT NULL,
    `ObservacionRecepcion` varchar(1000) NOT NULL,
    `OrdenEnRuta` int NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Entregas_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Entregas_Rutas_RutaId` FOREIGN KEY (`RutaId`) REFERENCES `Rutas` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Entregas_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Recepciones` (
    `Id` char(36) NOT NULL,
    `EntregaId` char(36) NOT NULL,
    `ClaveUnica` char(36) NOT NULL,
    `VersionBase` int NOT NULL,
    `AutorId` char(36) NOT NULL,
    `FechaReal` datetime(6) NOT NULL,
    `FechaSincronizacion` datetime(6) NOT NULL,
    `Conforme` tinyint(1) NOT NULL,
    `Receptor` varchar(180) NOT NULL,
    `Observacion` varchar(1000) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    `HuellaContenido` varchar(64) NOT NULL,
    `MotivoConflicto` varchar(1000) NOT NULL,
    `RevisadaPorId` char(36) NULL,
    `RevisadaEn` datetime(6) NULL,
    `Resolucion` varchar(1000) NOT NULL,
    `EntregaConfirmada` char(36) AS (CASE WHEN `Estado` = 'CONFIRMADA' THEN `EntregaId` ELSE NULL END) NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Recepciones_Entregas_EntregaId` FOREIGN KEY (`EntregaId`) REFERENCES `Entregas` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Recepciones_Usuarios_AutorId` FOREIGN KEY (`AutorId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Recepciones_Usuarios_RevisadaPorId` FOREIGN KEY (`RevisadaPorId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `EvidenciasRecepcion` (
    `Id` char(36) NOT NULL,
    `RecepcionId` char(36) NOT NULL,
    `Tipo` varchar(40) NOT NULL,
    `Contenido` mediumblob NOT NULL,
    `Sha256` varchar(64) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EvidenciasRecepcion_Recepciones_RecepcionId` FOREIGN KEY (`RecepcionId`) REFERENCES `Recepciones` (`Id`) ON DELETE RESTRICT
);

CREATE INDEX `IX_ControlesFrio_EntregaId` ON `ControlesFrio` (`EntregaId`);

CREATE INDEX `IX_AplicacionesPago_CargoId` ON `AplicacionesPago` (`CargoId`);

CREATE UNIQUE INDEX `IX_AplicacionesPago_PagoId_CargoId` ON `AplicacionesPago` (`PagoId`, `CargoId`);

CREATE UNIQUE INDEX `IX_AplicacionesPago_SenaId` ON `AplicacionesPago` (`SenaId`);

CREATE UNIQUE INDEX `IX_CondicionesPago_PedidoActivo` ON `CondicionesPago` (`PedidoActivo`);

CREATE INDEX `IX_CondicionesPago_PedidoId` ON `CondicionesPago` (`PedidoId`);

CREATE INDEX `IX_CondicionesPago_ResponsableId` ON `CondicionesPago` (`ResponsableId`);

CREATE UNIQUE INDEX `IX_Entregas_PedidoId` ON `Entregas` (`PedidoId`);

CREATE INDEX `IX_Entregas_ResponsableId_Estado` ON `Entregas` (`ResponsableId`, `Estado`);

CREATE INDEX `IX_Entregas_RutaId` ON `Entregas` (`RutaId`);

CREATE INDEX `IX_EvidenciasRecepcion_RecepcionId` ON `EvidenciasRecepcion` (`RecepcionId`);

CREATE INDEX `IX_Pagos_ClienteId_Fecha` ON `Pagos` (`ClienteId`, `Fecha`);

CREATE UNIQUE INDEX `IX_Pagos_ReferenciaUnica` ON `Pagos` (`ReferenciaUnica`);

CREATE INDEX `IX_Pagos_ResponsableId` ON `Pagos` (`ResponsableId`);

CREATE INDEX `IX_Recepciones_AutorId` ON `Recepciones` (`AutorId`);

CREATE UNIQUE INDEX `IX_Recepciones_ClaveUnica` ON `Recepciones` (`ClaveUnica`);

CREATE UNIQUE INDEX `IX_Recepciones_EntregaConfirmada` ON `Recepciones` (`EntregaConfirmada`);

CREATE INDEX `IX_Recepciones_EntregaId` ON `Recepciones` (`EntregaId`);

CREATE INDEX `IX_Recepciones_RevisadaPorId` ON `Recepciones` (`RevisadaPorId`);

CREATE UNIQUE INDEX `IX_Rutas_Codigo` ON `Rutas` (`Codigo`);

CREATE INDEX `IX_Rutas_ResponsableId` ON `Rutas` (`ResponsableId`);

CREATE INDEX `IX_Rutas_TransporteId` ON `Rutas` (`TransporteId`);

CREATE UNIQUE INDEX `IX_Senas_PagoId` ON `Senas` (`PagoId`);

CREATE INDEX `IX_Senas_PedidoId` ON `Senas` (`PedidoId`);

CREATE UNIQUE INDEX `IX_Transportes_Codigo` ON `Transportes` (`Codigo`);

ALTER TABLE `ControlesFrio` ADD CONSTRAINT `FK_ControlesFrio_Entregas_EntregaId` FOREIGN KEY (`EntregaId`) REFERENCES `Entregas` (`Id`) ON DELETE RESTRICT;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261107114529_Sprint3PagosEntregas', '10.0.12');

CREATE TABLE `ComprobantesFiscales` (
    `Id` char(36) NOT NULL,
    `PedidoId` char(36) NOT NULL,
    `ReferenciaUnica` varchar(80) NOT NULL,
    `Estado` varchar(40) NOT NULL,
    `NumeroFiscal` varchar(80) NOT NULL,
    `RespuestaProveedor` varchar(1000) NOT NULL,
    `Fecha` datetime(6) NOT NULL,
    `ResponsableId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ComprobantesFiscales_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ComprobantesFiscales_Usuarios_ResponsableId` FOREIGN KEY (`ResponsableId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX `IX_ComprobantesFiscales_PedidoId` ON `ComprobantesFiscales` (`PedidoId`);

CREATE UNIQUE INDEX `IX_ComprobantesFiscales_ReferenciaUnica` ON `ComprobantesFiscales` (`ReferenciaUnica`);

CREATE INDEX `IX_ComprobantesFiscales_ResponsableId` ON `ComprobantesFiscales` (`ResponsableId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261107115147_Sprint4ReportesFiscal', '10.0.12');

ALTER TABLE `Recepciones` ADD `PesoRecibido` decimal(12,3) NOT NULL DEFAULT 0.0;

ALTER TABLE `Recepciones` ADD `SaldoPendienteAlConfirmar` decimal(12,2) NULL;

ALTER TABLE `Recepciones` ADD `Unidad` varchar(20) NOT NULL DEFAULT '';

ALTER TABLE `InsumosProduccion` ADD `ReferenciaUnica` char(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE `ControlesFrio` ADD `Unidad` varchar(8) NOT NULL DEFAULT '';

ALTER TABLE `Cargos` ADD `AcuerdoServicio` varchar(300) NOT NULL DEFAULT '';

ALTER TABLE `Cargos` ADD `PrestadoEn` datetime(6) NULL;

CREATE TABLE `PrevisionesProducto` (
    `Id` char(36) NOT NULL,
    `OrdenProduccionId` char(36) NOT NULL,
    `ProductoId` char(36) NOT NULL,
    `CantidadEstimada` decimal(12,3) NOT NULL,
    `Unidad` varchar(20) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PrevisionesProducto_OrdenesProduccion_OrdenProduccionId` FOREIGN KEY (`OrdenProduccionId`) REFERENCES `OrdenesProduccion` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PrevisionesProducto_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
);

UPDATE `InsumosProduccion` SET `ReferenciaUnica` = `Id` WHERE `ReferenciaUnica` = '00000000-0000-0000-0000-000000000000';

UPDATE `ControlesFrio` SET `Unidad` = '°C' WHERE `Unidad` = '';

CREATE UNIQUE INDEX `IX_InsumosProduccion_ReferenciaUnica` ON `InsumosProduccion` (`ReferenciaUnica`);

CREATE UNIQUE INDEX `IX_PrevisionesProducto_OrdenProduccionId_ProductoId` ON `PrevisionesProducto` (`OrdenProduccionId`, `ProductoId`);

CREATE INDEX `IX_PrevisionesProducto_ProductoId` ON `PrevisionesProducto` (`ProductoId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261107123148_PrecisionesAceptacionP1', '10.0.12');

COMMIT;

