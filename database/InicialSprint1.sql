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

COMMIT;

