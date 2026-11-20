# CU05 · Solicitud del comprador

Implementa «Solicitar o crear cotización» de CU05 (RF01/RF02), sin cambiar los responsables de CU06. Usa `Cotizacion` y `DetalleCotizacion` existentes.

## Contrato

- `POST /api/v1/cotizaciones/solicitudes`: comprador autenticado, permiso `cotizaciones.solicitar`. Crea su solicitud y devuelve 201, `id` y `Location`.
- `PUT /api/v1/cotizaciones/{id}/solicitud`: modifica una solicitud propia que siga pendiente de revisión. Devuelve 200 e `id`.
- Cuerpo: `{ "detalles": [{ "productoId": "UUID", "cantidad": 1.5 }] }`, entre 1 y 100 líneas. Cantidades positivas, hasta tres decimales; productos activos.
- Cliente y canal se obtienen de la sesión (`PORTAL`). Unidad se obtiene del producto. El comprador no elige cliente, precio, costo, vigencia ni estado.
- 400: datos inválidos, cliente inactivo o producto no disponible. 403: cuenta no compradora o solicitud ajena. 404: solicitud inexistente. 409: solicitud ya preparada/presentada o versión superada.

La solicitud queda `BORRADOR`, con vigencia nula hasta que Ventas la prepare. Los importes internos iniciales son cero y no representan una oferta: la API los devuelve pendientes. La web muestra por separado precio de referencia, subtotales y total estimados con el catálogo vigente; los productos sin precio figuran «Por cotizar». No se fija una duración de oferta automáticamente, ni se crea pedido, reserva, pago o movimiento de stock.

Ventas abre el borrador, revisa productos, cantidades, precios y vigencia mediante el contrato existente y lo presenta. Solo entonces el comprador puede aceptar/rechazar. La confirmación del pedido sigue reservada a Ventas y exige aceptación vigente.

## Verificación

Cubrir creación y edición propias, aislamiento entre compradores, datos inválidos, rechazo de campos internos manipulados, revisión por Ventas, bloqueo de presentación/aceptación prematuras y consulta del pedido propio después de la confirmación. Comprobar que un comprador no administra cotizaciones ni confirma pedidos.

Verificado con 33 comprobaciones nuevas dentro del verificador completo: 209 correctas en SQL Server y 209 en MySQL. Las migraciones se aplicaron dos veces en bases temporales; los datos anteriores mantienen su vigencia. En la web se recorrió solicitud, modificación, preparación/presentación, aceptación y consulta del pedido confirmado con cuentas sintéticas.

## Actualización

Actualizar API y web juntas: `vigenteHasta`, `total`, `precioUnitario` y `subtotal` admiten `null` mientras la solicitud no está preparada. Aplicar la migración `SolicitudCotizacionComprador` del motor configurado y reiniciar la API. El sembrado agrega el permiso específico al comprador; volver a iniciar sesión en la web para renovar sus permisos. Los contratos internos de preparación y confirmación siguen usando importes y fechas obligatorios.
