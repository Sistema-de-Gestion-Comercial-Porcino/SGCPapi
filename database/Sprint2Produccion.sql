START TRANSACTION;
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

COMMIT;

