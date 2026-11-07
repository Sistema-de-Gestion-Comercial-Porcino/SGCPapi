START TRANSACTION;
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

COMMIT;

