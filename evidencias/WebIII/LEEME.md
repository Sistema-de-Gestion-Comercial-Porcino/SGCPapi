# Evidencias Web III · W3-P4

Víctor Manuel Manrique Arancivia · 18/11/2026.

15 capturas reales de Swagger: vista general, los 12 casos de [API_Integrador.http](../../pruebas/API_Integrador.http), consulta posterior al DELETE y comparación antes/después del reinicio.

API verificada: commit `356c9dc`, rama `sprint4/p4-api-swagger-evidencias`. Entorno local con SQL Server Express y datos sintéticos en una base aislada. Las capturas se tomaron antes de integrar la rama a `main`. La API y la base temporales se eliminaron al finalizar; los tokens visibles pertenecen a esas sesiones de prueba y ya no son utilizables.

| Caso | Resultado comprobado | Captura |
|---|---|---|
| Swagger | Título, descripción, contacto y autenticación JWT | [00](00_Swagger_general.jpg) |
| 1 · Login | 200, token y vencimiento | [01](01_POST_login_200.jpg) |
| 2 · Listar clientes | 200, respuesta paginada | [02](02_GET_clientes_200.jpg) |
| 3 · Crear cliente | 201, DTO y cabecera Location | [03](03_POST_clientes_201.jpg) |
| 4 · Consultar Location | 200, mismo cliente creado | [04](04_GET_cliente_Location_200.jpg) |
| 5 · Cliente sin nombre | 400, DATOS_INVALIDOS y detalle del campo | [05](05_POST_cliente_sin_nombre_400.jpg) |
| 6 · Cotización con cliente inexistente | 400, CLIENTE_NO_ACTIVO | [06](06_POST_cotizacion_cliente_inexistente_400.jpg) |
| 7 · Editar cliente | 200, datos actualizados | [07](07_PUT_cliente_200.jpg) |
| 8 · Pedidos del cliente | 200, pedido confirmado con total calculado | [08](08_GET_pedidos_cliente_200.jpg) |
| 9 · Cliente inexistente | 404, NO_ENCONTRADO | [09](09_GET_cliente_inexistente_404.jpg) |
| 10 · Borrar cliente con historial | 409, CLIENTE_CON_HISTORIAL | [10](10_DELETE_cliente_con_historial_409.jpg) |
| 11 · Borrar cliente sin historial | 204 sin cuerpo; consulta posterior 404 | [11](11_DELETE_cliente_sin_historial_204.jpg), [11b](11b_GET_cliente_eliminado_404.jpg) |
| 12 · Persistencia | 200 antes y después de detener/iniciar la API; mismos datos | [12a](12a_GET_cliente_antes_reinicio_200.jpg), [12b](12b_GET_cliente_despues_reinicio_200.jpg) |

El caso 8 usa una cotización presentada y aceptada, con pedido confirmado, como indican las preparaciones del archivo HTTP. El caso 11 usa un segundo cliente sin historial.

En el caso 12 se mantuvo la misma base: se detuvo el proceso `9464` y se inició el `7684`. Coincidieron todos los campos del cliente. [Registro breve del reinicio](12_reinicio_API.txt). Las fechas de las respuestas HTTP están en UTC.

Para el PDF de entrega, usar las capturas en este orden y las descripciones de la tabla.
