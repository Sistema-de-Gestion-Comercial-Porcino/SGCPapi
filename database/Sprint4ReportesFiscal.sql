START TRANSACTION;
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

COMMIT;

