START TRANSACTION;
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

