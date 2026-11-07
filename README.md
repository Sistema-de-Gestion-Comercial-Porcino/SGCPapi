# SGCPapi

API del Sistema de Gestión Comercial Porcino. ASP.NET Core 10 con controladores, Entity Framework Core 10 y **MySQL 8**. La web MVC y Android consumen esta API; no acceden directamente a la base de datos.

## Equipo

- Dario Haziel Tambo Parra
- Angel Sebastian Eguez Mur
- Sofia Cuellar Roca
- Victor Manuel Manrique Arancivia

## Sprint 1

Acceso, registro público de comprador, recuperación de contraseña, usuarios y permisos, clientes, clasificaciones, productos, vigencias de costo/precio, cotizaciones versionadas y pedidos con margen histórico. CU01–CU07. Builder construye el pedido. Los módulos de producción, inventario, cobranza, entregas y reportes se agregan en la implementación P1 de los sprints 2–4 descrita más abajo.

Crear producto no crea stock. Una cotización rechazada o vencida no crea pedido. El precio del pedido procede de la cotización aceptada y el costo vigente se conserva al confirmar. Los cambios posteriores no recalculan su margen.

## Requisitos

- SDK .NET 10.
- Una instancia MySQL 8.0.13 o posterior accesible desde el equipo. Puede estar en otra computadora, o en un contenedor.
- Permisos para crear/aplicar el esquema de la base `sgcp`. Para las pruebas aisladas se requiere CREATE DATABASE.
- Red habilitada en el servidor cuando sea remoto. No expongas el puerto de MySQL directamente a internet.
- MariaDB no sirve: el proveedor oficial de Oracle habla el protocolo de MySQL y el esquema usa columnas generadas de MySQL 8.

## Preparación

Desde la raíz de este repositorio:

```bash
dotnet restore
dotnet tool restore
```

Configura los secretos del proyecto `src/SGCP.Api` con `dotnet user-secrets` o mediante variables de entorno. Las claves requeridas son:

| Clave | Uso |
|---|---|
| `ConnectionStrings:Sgcp` | Conexión real de MySQL. **Obligatoria**: la API no arranca sin ella |
| `Jwt:Clave` | Clave aleatoria de al menos 32 bytes |
| `DatosIniciales:ClaveAdministrador` | Contraseña inicial de 10 caracteres o más |
| `DatosIniciales:Ejemplos` | `true` para sembrar cortes/derivados ilustrativos |
| `DatosIniciales:CorreoAdministrador` | Correo de la primera cuenta |

Ejemplos de formato de conexión, sin credenciales reales:

```text
Local:   Server=localhost;Port=3306;Database=sgcp;User ID=sgcp;Password=CONTRASEÑA;SslMode=Preferred
Remoto:  Server=192.168.1.50;Port=3306;Database=sgcp;User ID=sgcp;Password=CONTRASEÑA;SslMode=Required
```

MySQL no tiene autenticación integrada de Windows: siempre hay usuario y contraseña. `appsettings.json` deja `ConnectionStrings:Sgcp` vacío a propósito, para que no haya credenciales en el repositorio; si falta, la API se detiene al arrancar con un mensaje que indica el formato, igual que con `Jwt:Clave`. Contra un servidor remoto usa `SslMode=Required`.

Si el usuario SQL no puede crear bases, crea previamente `sgcp`; la migración construye las tablas. En PowerShell 7, `./scripts/Preparar-Desarrollo.ps1` pide los valores sin mostrarlos, guarda secretos y ejecuta la migración. Si usas Visual Studio/PowerShell anterior, configura las claves con el administrador de secretos del proyecto.

```bash
dotnet ef database update --project src/SGCP.Api
dotnet run --project src/SGCP.Api
```

La primera ejecución siembra permisos, responsabilidades y la cuenta `admin` con la contraseña configurada. La operación es idempotente y no sobrescribe cuentas existentes. La migración crea el esquema; el sembrado de datos se ejecuta al arrancar la API. No se cargan precios, clientes, cotizaciones ni pedidos ficticios.

API local: `http://localhost:5081/api/v1/`. Swagger: `http://localhost:5081/swagger`. Para acceso desde otro equipo, configura la escucha en la IP de red y permite solo la red del equipo en el firewall. Los clientes deben usar esa dirección en lugar de `localhost`.

## Migraciones y pruebas

La migración inicial fue generada con **MySql.EntityFrameworkCore**, el proveedor oficial de Oracle, no convertida desde otro motor. `database/InicialSprint1.sql` es el script MySQL para revisión.

No uses `--idempotent` con MySQL: EF genera `IF NOT EXISTS(...) BEGIN ... END`, que es sintaxis de T-SQL y MySQL rechaza. El script del repositorio es el plano, sin esa envoltura; se aplica una sola vez sobre una base vacía. Para reaplicar sin riesgo usá `dotnet ef database update`, que sí consulta `__EFMigrationsHistory`.

```bash
dotnet build --warnaserror
dotnet ef migrations has-pending-model-changes --project src/SGCP.Api
dotnet ef migrations script --project src/SGCP.Api --output artifacts/EsquemaCompleto.sql
dotnet run --project tests/SGCP.Verificacion
```

Sin `SGCP_MYSQL_TEST`, las pruebas usan SQLite en un archivo temporal solo para verificar comportamiento: **esto no certifica MySQL**. La aplicación principal únicamente referencia el proveedor MySQL; SQLite pertenece al proyecto de pruebas.

Para verificar con MySQL, configura `SGCP_MYSQL_TEST` con una conexión de pruebas o ejecuta `scripts/Verificar-MySql.ps1`. El verificador crea una base `sgcp_pruebas_<uuid>`, aplica la migración dos veces, comprueba que no queden migraciones pendientes, ejecuta el recorrido HTTP y elimina únicamente esa base aislada al terminar. El usuario necesita CREATE DATABASE; concedelo acotado al prefijo con ``GRANT ALL ON `sgcp%`.* TO 'sgcp'@'%';`` en lugar de dar permiso sobre todo el servidor.

El flujo `.github/workflows/sprint1-mysql.yml` ejecuta esa verificación con un MySQL 8.4 temporal en GitHub Actions. Su presencia no significa que se haya ejecutado: comprueba el resultado de la ejecución en GitHub antes de afirmar que pasó.

## Seguridad y reglas

- JWT con firma, emisor, audiencia, vencimiento e identificador de sesión. Cada solicitud verifica la sesión y recupera permisos efectivos del servidor.
- Contraseñas con PasswordHasher; bloqueo tras un umbral configurable. Administración habilita la cuenta bloqueada. Conviene disponer de una segunda cuenta administradora para la demostración del bloqueo.
- Se protege el último administrador habilitado al desactivar o quitar su responsabilidad. No existen borrados físicos en los endpoints.
- El registro público crea una ficha nueva vinculada al comprador. No permite apropiarse de una ficha previa por conocer su teléfono o identificación.
- El comprador solo consulta sus cotizaciones/pedidos. Nunca recibe costos ni márgenes internos.
- Cotizaciones sustituidas se conservan y no pueden generar operaciones nuevas; cada versión tiene código propio y enlace a su antecesora.
- Vigencias expresadas como intervalos `[desde, hasta)`. Las fechas del contrato API son UTC; la web las presenta en hora de Bolivia.
- Unicidad en base de datos y transacciones serializables protegen pedidos, versiones, códigos y vigencias. Un conflicto se comunica al cliente para actualizar y reintentar.
- MySQL no tiene índices filtrados. Las tres reglas que los usaban se sostienen con columnas generadas que valen `NULL` cuando la fila no participa, más un índice único sobre ellas: `PreciosProducto.ProductoConVigenciaAbierta` deja una sola vigencia abierta por producto, `Clientes.IdentificacionFiscalUnica` exige NIT único solo entre clientes activos con NIT, y el índice único de `Cotizaciones.CotizacionOrigenId` admite varias raíces porque MySQL considera distintos los `NULL`. Son columnas de la base, no propiedades del modelo, y la garantía es la misma.
- El margen es estimado por productos, no utilidad real por animal. Cantidades de hasta tres decimales; precios e importes monetarios con dos.

## Recuperación de acceso

En desarrollo, los mensajes se escriben fuera del repositorio, en la carpeta local de datos del usuario `SGCP/recuperaciones` (Linux: `~/.local/share/SGCP/recuperaciones`). Solo el operador del equipo puede leerlos. No se devuelve el token en el endpoint público. Esta es una bandeja local de demostración, no una verificación por correo de producción.

En producción configura `Correo:Servidor`, `Correo:Puerto`, `Correo:Usuario`, `Correo:Clave`, `Correo:Remitente` y `Recuperacion:UrlWeb` HTTPS. El envío SMTP requiere un servicio real. El token dura 30 minutos, se usa una vez y al restablecer se revocan las sesiones anteriores.

## Rutas principales

Todas bajo `/api/v1`, JSON y autenticación Bearer salvo acceso/registro/recuperación:

| Módulo | Rutas |
|---|---|
| Acceso | `POST auth/login`, `auth/registro-comprador`, `auth/recuperar`, `auth/restablecer`, `auth/cerrar-sesion`; `GET auth/yo` |
| Usuarios | `GET/POST usuarios`, `GET/PUT usuarios/{id}`, `POST usuarios/{id}/estado`; `GET roles`, `permisos`, `auditoria` |
| Clientes | `GET/POST clientes`, `GET/PUT clientes/{id}`, `POST clientes/{id}/activar`, `desactivar` |
| Clasificaciones | `GET/POST clasificaciones`, `GET/PUT clasificaciones/{id}`, `POST clasificaciones/{id}/estado` |
| Productos | `GET/POST productos`, `GET/PUT productos/{id}`, `POST productos/{id}/estado` |
| Costos | `GET/POST productos/{id}/precios`, `PUT precios/{id}`, `POST precios/{id}/finalizar` |
| Cotizaciones | `GET/POST cotizaciones`, `GET/PUT cotizaciones/{id}`, `POST .../presentar`, `aceptar`, `rechazar`, `nueva-version` |
| Pedidos | `GET/POST pedidos`, `GET pedidos/{id}`, `POST pedidos/{id}/cancelar` |
| Resumen | `GET resumen`, filtrado según permisos |

Swagger documenta cuerpos y enumeraciones. Los listados aceptan `pagina`, `tamanoPagina` (10/20/50/100), `busqueda` y filtros de módulo. Los errores contienen `codigo`, `mensaje` y `detalles`.

## Ejecución paso a paso

Para levantar el sistema completo (API, web y Android) en orden, hay una guía del equipo en `SGCPdocumentacion/00_Definicion/Como_ejecutar_el_sistema.md`, con la configuración de MySQL local, el recorrido de comprobación y una tabla de errores frecuentes.

El motor pasó de SQL Server a MySQL el 6 de noviembre de 2026. Qué se cambió, por qué el proveedor de Oracle y no Pomelo, y las tres trampas de MySQL: `SGCPdocumentacion/00_Definicion/Cambio_a_MySQL.md`.

## Control de versiones

Formato `vMAYOR.MENOR.PARCHE`, donde **MAYOR es el número de sprint**:

| Posición | Significado | Ejemplo |
|---|---|---|
| MAYOR | número de sprint. `0` es el trabajo anterior a la primera entrega | `v2.4.0` pertenece al Sprint 2 |
| MENOR | número de parte del reparto: P1, P2, P3 o P4 | `v2.1.0` es la parte 1 del Sprint 2 |
| PARCHE | corrección sobre esa misma parte, sin agregar funcionalidad | `v2.1.1` |

Los cuatro repositorios comparten MAYOR y MENOR, porque el sprint y la parte son los mismos para todos; el PARCHE avanza por separado en cada repositorio.

**La etiqueta de cierre de un sprint es `vN.4.0`**, la versión del commit de P4, que es siempre el último. Desde el Sprint 2 no se crea `vN.0.0`: quedaría por detrás de versiones ya entregadas. El Sprint 1 conserva su `v1.0.0` porque se entregó en un solo bloque.

Cada versión de la tabla tiene su etiqueta en git, así que `git show v1.0.0` muestra el código exacto de esa fila. **La etiqueta se crea después del commit**, nunca antes, o apunta al estado anterior:

```bash
git commit -m "v1.0.0 - Sprint 1"
git tag -a v1.0.0 -m "Sprint 1 entregado"
git push origin main --follow-tags
```

En la columna *Cambio*, los prefijos son fijos para que la tabla se pueda leer de un tirón: `Corrección:`, `Documentación:`, `Infraestructura:`, o nada cuando es funcionalidad nueva. Si el cambio llegó por una rama, la fila termina en `(PR #n)`; la rama en sí no se anota, porque git ya la guarda en el commit de fusión.

Desde `v1.0.0` el prefijo es `v` sin punto, que es la convención y lo que admite una etiqueta de git. Las filas anteriores se conservan como se escribieron.

### Historial

Más reciente arriba.

| Versión | Fecha | Sprint | Cambio | Autor |
|---|---|---|---|---|
| v4.1.0 | 2026-11-07 | 2, 3 y 4 | Parte de API de los tres sprints (S2-P1, S3-P1, S4-P1): producción, sanidad, lotes, pesajes, inventario, reservas y frío; pagos, señas, saldos, transportes, entregas y sincronización con evidencias; reportes por perfil con exportación PDF y Excel, constancia y Adapter fiscal sin proveedor. Patrones Factory Method, Strategy, Facade y Adapter | Dario Haziel Tambo Parra |
| v1.0.1 | 2026-11-07 | 1 | Corrección: el esquema de versiones aclara que MENOR es el número de parte y que la etiqueta de cierre del sprint es `vN.4.0` | Dario Haziel Tambo Parra |
| v1.0.0 | 2026-11-06 | 1 | Corrección: la fábrica de contexto de diseño ya no exige credenciales, de modo que el flujo de CI genera y revisa migraciones sin configurarlas | Dario Haziel Tambo Parra |
| v.0.1.0 | 2026-11-06 | 1 | CU01–CU07, permisos, catálogo, vigencias de precio, cotizaciones versionadas, pedidos con margen histórico y Builder, sobre MySQL 8 con tres índices filtrados reemplazados por columnas generadas | Dario Haziel Tambo Parra |

En SGCPapi hay dos versiones distintas que no hay que confundir. La de esta tabla es la del repositorio. El `v1` de `/api/v1/` es la versión del **contrato** que consumen la web y Android, y solo sube si se rompe la compatibilidad con ellos: en el Sprint 2 habrá `v2.x.x` sirviendo en `/api/v1/` y eso es correcto.

## Contrato P1: sprints 2, 3 y 4

Decisiones de Dario confirmadas el 7 de noviembre de 2026: se conservan los seis actores actuales; ventas incluye cobranza (no se agrega «finanzas»). RF-11 no tiene proveedor confirmado: el Adapter registra la operación SIN_PROVEEDOR y no emite comprobantes fiscales. Esta implementación es la base de API que P2 integrará; no implica que las pantallas, Android, diagramas o validación de P3/P4 estén terminados.

### Sincronización de recepción para Android

`POST /api/v1/recepciones/sincronizar`, JWT de ENTREGA. Cuerpo `{ "recepciones": [...] }`, de 1 a 20 elementos. Cada elemento:

```json
{
  "claveUnica": "UUID generado por el teléfono, conservado en reintentos",
  "entregaId": "UUID descargado de mis-entregas",
  "versionBase": 2,
  "autorId": "UUID del usuario autenticado",
  "fechaReal": "2026-11-07T18:00:00Z",
  "pesoRecibido": 12.375,
  "unidad": "kg",
  "conforme": true,
  "receptor": "Nombre de quien recibe",
  "observacion": "",
  "evidencias": [{ "tipo": "image/jpeg", "contenidoBase64": "..." }]
}
```

La evidencia viaja en el mismo cuerpo para guardar recepción y evidencia atómicamente: 1–3 imágenes JPEG/PNG, hasta 512 KiB cada una; el móvil debe reducirlas antes de enviar. El lote completo admite hasta 12 MiB de JSON; si excede, dividir en lotes menores. No enviar rutas locales ni URLs públicas. El servidor valida formato, tamaño, autor y asignación. Guarda los archivos en la base, con hash SHA-256 y acceso autenticado.

Respuesta `{ "resultados": [{ "claveUnica", "recepcionId", "estado", "duplicada", "fechaSincronizacion" }] }`. `estado` es CONFIRMADA o CONFLICTO (y posteriormente DESCARTADA si se resuelve así). Un reintento devuelve la recepción original y no crea filas, incluso tras reasignaciones posteriores; solo su autor o administración puede consultarla. Reutilizar una clave con otro contenido devuelve CLAVE_REUTILIZADA. La clave única tiene índice en MySQL.

Una versión distinta o una entrega que ya no esté DESPACHADA produce una recepción CONFLICTO: queda para revisión y no cambia la entrega. Los demás errores de validación rechazan todo el lote sin escrituras parciales. Un fallo de concurrencia devuelve 409; el cliente reintenta el lote con las mismas claves. La revisión requiere motivo y versión actual; permite aplicar o descartar, conservando la captura original. Conforme -> ENTREGADA; observada -> OBSERVADA. Fecha real y fecha de sincronización se conservan separadas. La recepción registra peso positivo con hasta tres decimales y unidad kg/g, sin modificar stock. Ninguna recepción modifica pagos ni saldos.

`GET /api/v1/mis-entregas` descarga solo entregas del responsable autenticado, previamente despachadas/autorizadas a domicilio. Los retiros locales no se descargan a Android; la web registra su recepción online mediante el mismo contrato de recepción y el responsable asignado. Nunca recibe costos, precios, márgenes ni saldos. Las fechas JSON son UTC. La captura offline queda pendiente en Android hasta recibir confirmación; esta API no implementa la cola local de P2.

### Matriz de reportes propuesta y autorizada para implementar

| Perfil | Clientes | Cotizaciones | Pedidos | Producción | Inventario | Pagos | Entregas |
|---|---|---|---|---|---|---|---|
| ADMINISTRACION | Todos | Todas | Todos, costo y margen históricos | Toda | Todo | Todos | Todas |
| VENTAS | Todos | Todas | Todos, costo y margen históricos | No | No | Todos | Todas |
| PRODUCCION | No | No | No | Toda, sin importes | Todo, sin importes | No | No |
| ALMACEN | No | No | No | Toda, sin importes | Todo, sin importes | No | No |
| ENTREGA | No | No | No | No | No | No | Solo asignadas, sin importes |
| COMPRADOR | No | Propias | Propios, sin costos ni márgenes | No | No | Propios | Propias |

Misma matriz para consulta, PDF y Excel. Filtros admitidos: desde/hasta (UTC), productoId, clasificacionId (tipo de corte/derivado), clienteId y estado según el reporte; los filtros incompatibles se rechazan, no se ignoran. Comprador solicitando cliente ajeno recibe 403. Roles múltiples acumulan permisos operativos, pero una cuenta con COMPRADOR mantiene siempre el aislamiento por cliente y no ve costos/márgenes. P3 puede revisar esta matriz con el equipo.


## Rutas P1 de sprints 2–4

Todas requieren JWT. Swagger muestra los DTO, tipos y enumeraciones. Los listados usan `pagina` y `tamanoPagina` (10/20/50/100), salvo consultas de operación acotadas o reportes (máximo 10000 filas, sin truncar: se pide acotar filtros).

| Módulo | Contrato bajo `/api/v1` |
|---|---|
| Animales y sanidad | `GET/POST animales`, `PUT animales/{id}`, `POST animales/{id}/estado`, `GET/POST animales/{id}/controles-sanitarios` |
| Producción | `GET/POST ordenes-produccion`, `GET ordenes-produccion/{id}`, `POST .../iniciar`, `consumos`, `confirmar`, `anular` |
| Catálogo de resultados | La confirmación recibe una lista de productos y códigos de lote; la consulta muestra producto, familia y tipo de corte/derivado |
| Lotes | `GET lotes`, `POST lotes/externos`, `POST lotes/{id}/rechazar` |
| Almacén | `GET/POST pesajes`, `GET/POST entradas-inventario`, `POST entradas-inventario/{id}/anular`, `GET existencias`, `GET movimientos-stock` |
| Reservas | `GET/POST reservas`, `POST reservas/{id}/liberar`, `GET pedidos-operativos` (sin precios) |
| Frío e incidencias | `GET/POST controles-frio`, `GET incidencias`. Frío referencia un lote o una entrega y usa °C con evaluación humana |
| Cargos | `GET/POST cargos`, `POST cargos/{id}/anular`. El POST exige acuerdoServicio y prestadoEn para servicios ya prestados; los cargos de producto nacen al confirmar el pedido |
| Condición acordada | `GET/POST pedidos/{id}/condiciones-pago` |
| Cobranza | `GET/POST pagos`, `POST pagos/{id}/anular`, `GET saldos?clienteId=...&pedidoId=...` |
| Señas | `GET/POST senas`, `POST senas/{id}/aplicar`, `resolver` |
| Transporte | `GET/POST transportes`, `PUT transportes/{id}`, `POST transportes/{id}/estado`, `GET responsables-entrega` |
| Rutas | `GET/POST rutas`, `POST rutas/{id}/finalizar` |
| Entregas | `GET/POST entregas`, `POST entregas/{id}/despachar`, `reasignar`, `GET mis-entregas` |
| Recepción | `POST recepciones/sincronizar`, `GET recepciones/{id}`, `GET recepciones/conflictos`, `POST recepciones/{id}/revisar`, `GET recepciones/{id}/evidencias/{evidenciaId}` |
| Reportes | `GET reportes` devuelve tipos permitidos; `GET reportes/{tipo}`, `GET reportes/{tipo}/exportar/pdf`, `.../xlsx` |
| Constancia | `GET entregas/{id}/constancia`: PDF con receptor, peso/unidad, fechas UTC, saldo histórico, hashes y evidencia gráfica; solo recepción conforme confirmada y acceso de administración/ventas o comprador propio |
| Fiscal condicional | `GET/POST pedidos/{pedidoId}/registro-fiscal`: referencia estable, estado SIN_PROVEEDOR, sin emisión fiscal real |

### Reglas de operación y migración

- Las órdenes admiten previsiones con producto, cantidad estimada y unidad; no generan disponibilidad ni rendimientos garantizados. El catálogo nunca genera stock. Un faeneo requiere animal aprobado; una fabricación consume existencias aptas no reservadas. La confirmación crea varios lotes con producto y clasificación, todavía sin cantidad confirmada. Pesaje confirma bruto/tara/neto; entrada física incrementa stock una sola vez por lote y producto.
- La recepción externa documentada permite abastecer sin obligar a faenar un animal. Debe pasar igualmente por pesaje y entrada física. Cada consumo envía referenciaUnica UUID, existenciaId y cantidad: reintentar con el mismo contenido devuelve el original; cambiar el contenido con la misma referencia se rechaza. Una existencia tiene una ubicación textual; no hay transferencias entre cámaras ni reposiciones parciales sobre el mismo lote en este alcance.
- Una reserva se puede repartir entre lotes: una abierta por detalle/existencia; la suma no puede exceder el detalle. El despacho exige reserva completa, lote apto y condición comercial satisfecha. Descuenta stock una sola vez y conserva el movimiento. Cancelar un pedido exige acuerdo de resolución para sus señas, libera reservas y marca CANCELADA su entrega preparada; después de despachar requiere resolución comercial.
- El rechazo bloquea los lotes afectados y sus derivados, libera reservas activas y registra incidencias. Nunca suma de vuelta insumos ya consumidos. Las anulaciones conservan fila, motivo y auditoría; no existe DELETE físico.
- Los cargos de productos se crean con los importes históricos del pedido. Se usa redondeo acumulado por detalle para que sumen exactamente el total acordado, incluso con fracciones de kilo; no se recalcula el margen histórico. La migración de Sprint 2 también crea cargos para los pedidos confirmados anteriores, sin recalcular precios. Los servicios se registran por separado.
- Saldo = cargos vigentes − aplicaciones de pagos no anulados. Si todavía no existe acuerdo registrado, la consulta indica SIN_ACUERDO; no presume contado ni inventa vencimiento. La seña registra dinero recibido antes de aplicarlo, se aplica completa a un cargo del mismo pedido una sola vez, o se conserva un acuerdo de resolución sin devolución/retención automática. No se aplican pagos entre clientes distintos ni por encima del saldo.
- El acuerdo de crédito exige importe y vencimiento explícitos. Sustituirlo conserva el anterior; después del despacho se conserva el acuerdo histórico. Contado exige saldo cero para despachar. La recepción nunca escribe pagos ni saldos.
- Las entregas son completas, una por pedido; no se implementa entrega parcial. Retiros locales tienen responsable sin ruta obligatoria; domicilio requiere ruta. Una reasignación cambia la versión y las capturas antiguas requieren revisión. Ruta terminada requiere entregas confirmadas, observadas o canceladas y conflictos resueltos. La API no puede detectar capturas que nunca salieron del teléfono: P2 debe impedir el cierre mientras su cola local tenga envíos pendientes.
- Tras actualizar una base existente, ejecuta la API con `DatosIniciales:Habilitar=true` una vez: incorpora permisos y actualiza las seis responsabilidades estándar sin reemplazar cuentas. Las responsabilidades estándar se mantienen desde el código; las adicionales no se reescriben.
- Se conservan las migraciones InicialSprint1, Sprint2Produccion, Sprint3PagosEntregas, Sprint4ReportesFiscal y PrecisionesAceptacionP1. La última incorpora detalles explícitos del Excel omitidos en el reparto resumido: peso/unidad de recepción, saldo histórico de constancia, referencia del consumo, previsiones y evidencia de prestación del servicio. `database/Sprint2Produccion.sql`, `Sprint3PagosEntregas.sql` y `Sprint4ReportesFiscal.sql` son incrementales desde el sprint anterior; después debe aplicarse `database/PrecisionesAceptacionP1.sql`. `database/EsquemaCompleto.sql` es para base vacía. No ejecutar un script incremental sobre un esquema distinto; preferir `dotnet ef database update`.

### Exportaciones y patrones

[ClosedXML 0.105.1](https://www.nuget.org/packages/ClosedXML/0.105.1) genera XLSX tipados sin requerir Excel instalado. [PDFsharp 6.2.4](https://www.nuget.org/packages/PDFsharp/6.2.4) genera los PDF en .NET; la fuente DejaVu Sans está incluida con su licencia para evitar depender de fuentes del equipo. No se usa Office, Chromium ni un servicio externo. Los valores textuales del XLSX se escriben como texto, no como fórmulas.

- Builder: pedido histórico desde cotización aceptada (Sprint 1).
- Factory Method: resultados de faeneo/fabricación; selector por fábricas registradas en DI (Sprint 2).
- Strategy: contado y crédito acordado antes de despacho (Sprint 3).
- Facade: autorización y consulta única para JSON, PDF y XLSX (Sprint 4).
- Adapter: contrato fiscal con implementación sin proveedor; no simula emisión fiscal (Sprint 4).
- Command y State del Android corresponden a P2 y no se declaran implementados por esta API.

Las fechas de inventario filtran por fecha de producción del lote y muestran existencias actuales, no reconstruyen un inventario histórico. Un filtro por producto en pedidos/cotizaciones selecciona documentos que lo incluyen y conserva su total completo. Reportes de clientes y pagos no admiten producto; producción e inventario no admiten cliente. No se modifica el estado de una cotización vencida al reportarla. La constancia conserva el saldo al confirmar la recepción; no lo reemplaza con pagos posteriores. Como contiene saldo, se restringe a administración/ventas y comprador propio, no al repartidor.

### Comprobación P1

Con `SGCP_MYSQL_TEST` configurada, `dotnet run --project tests/SGCP.Verificacion` ejecuta Sprint 1 y S2–S4 en una base nueva y la elimina al terminar. Repetir con `LANG=es_BO.UTF-8 LC_ALL=es_BO.UTF-8` y con `LANG=C LC_ALL=C`. `--solo-sprint1` limita el recorrido al anterior. Sin la variable se conserva la prueba SQLite del Sprint 1; la certificación de producción/inventario y reportes se hace sobre MySQL real.

El ensayo de volumen siembra 2000 pedidos adicionales exclusivamente en esa base temporal y mide la consulta HTTP del reporte. Es una medición técnica local, no sustituye RNF de usabilidad, hardware del docente ni pruebas Android en teléfono.
