# SGCPapi

API del Sistema de Gestión Comercial Porcino. ASP.NET Core 10 con controladores, Entity Framework Core 10 y **SQL Server** o **MySQL 8** (elegible). La web MVC y Android consumen esta API; no acceden directamente a la base de datos.

## Entrega Web III · API Integrador

| Dato | Valor |
|---|---|
| Proyecto | **SGCPapi**: Web API del Sistema de Gestión Comercial Porcino (SGCP) |
| Versión de .NET | **.NET 10** (SDK 10.0.100, `net10.0`), ASP.NET Core 10, Entity Framework Core 10.0.12 |
| Base de datos | **SQL Server 2019 o posterior** (LocalDB, Express o Docker) con migración propia; MySQL 8 sigue disponible |
| Repositorio | https://github.com/Sistema-de-Gestion-Comercial-Porcino/SGCPapi |
| Evidencias | [`docs/Evidencias_API_Integrador.pdf`](docs/Evidencias_API_Integrador.pdf), capturas en [`evidencias/WebIII`](evidencias/WebIII) y pruebas reproducibles en [`pruebas/API_Integrador.http`](pruebas/API_Integrador.http) |

### Integrantes

| Integrante | Usuario de GitHub | Aporte en este repositorio |
|---|---|---|
| Dario Haziel Tambo Parra | [`SuperGenio56`](https://github.com/SuperGenio56) | API de los sprints 1 a 4, SQL Server como motor con su migración, verificador y entrega |
| Angel Sebastian Eguez Mur | [`sebaxtianmur89`](https://github.com/sebaxtianmur89) | Clientes como recurso REST: DTO de salida, 201 con `Location`, `DELETE` con regla de dominio, pedidos del cliente y 20 comprobaciones |
| Victor Manuel Manrique Arancivia | [`VictorManrique01`](https://github.com/VictorManrique01) | Swagger documentado, 12 pruebas HTTP, evidencias, roles jerárquicos y reportes protegidos |
| Sofia Cuellar Roca | [`sof1345`](https://github.com/sof1345) | Script de la entrega: ZIP limpio con el proyecto, las migraciones y el PDF, sin `bin`, `obj` ni `.vs` |

### Recurso evaluado

El recurso principal es **Cliente** y su recurso relacionado es **Pedido** (un cliente tiene muchos pedidos, clave foránea `Pedidos.ClienteId`). Todas las rutas van bajo `/api/v1` y requieren el token de `POST /api/v1/auth/login`.

| Operación | Ruta | Respuestas |
|---|---|---|
| Listado paginado | `GET /clientes?pagina=1&tamanoPagina=20` | 200 |
| Consulta por id | `GET /clientes/{id}` | 200, 404 |
| Alta | `POST /clientes` | 201 con `Location`, 400, 409 |
| Edición | `PUT /clientes/{id}` | 200, 400, 404, 409 |
| Baja | `DELETE /clientes/{id}` | 204, 404, 409 `CLIENTE_CON_HISTORIAL` |
| Recurso relacionado | `GET /clientes/{id}/pedidos` | 200, 404 |

Organización: entidades en `Dominio/`, DTO de entrada en `Contratos/Peticiones` y de salida en `Contratos/Respuestas`, controladores en `Controllers/`, reglas en `Servicios/` y datos (contexto, configuraciones y migraciones) en `Datos/`. El contexto se inyecta por constructor y todas las operaciones son asíncronas. Los errores se devuelven con un código (`DATOS_INVALIDOS`, `NO_ENCONTRADO`, `CLIENTE_NO_ACTIVO`, etc.).

### Pasos con SQL Server

El detalle está en [Usar SQL Server en lugar de MySQL](#usar-sql-server-en-lugar-de-mysql). En resumen, desde la raíz del repositorio:

```bash
dotnet restore
dotnet tool restore
dotnet user-secrets set "BaseDatos:Proveedor" "SqlServer" --project src/SGCP.Api
# Cadena de ejemplo, sin credenciales reales:
dotnet user-secrets set "ConnectionStrings:Sgcp" "Server=(localdb)\MSSQLLocalDB;Database=sgcp;Trusted_Connection=True;TrustServerCertificate=True" --project src/SGCP.Api
dotnet user-secrets set "Jwt:Clave" "<clave de al menos 32 bytes en Base64>" --project src/SGCP.Api
dotnet user-secrets set "DatosIniciales:ClaveAdministrador" "<contraseña inicial de admin>" --project src/SGCP.Api
dotnet user-secrets set "DatosIniciales:Ejemplos" "true" --project src/SGCP.Api
dotnet ef database update --project src/SGCP.Api --context SgcpContextSqlServer
dotnet run --project src/SGCP.Api
```

1. La migración crea las tablas en SQL Server (`Datos/MigracionesSqlServer`).
2. El primer arranque carga los datos de prueba: permisos, roles, la cuenta `admin` y el catálogo de ejemplo. La carga es idempotente.
3. Swagger queda en `http://localhost:5081/swagger`. Inicia sesión con `POST /api/v1/auth/login`, pulsa **Authorize** y pega el token.
4. Para comprobar la persistencia, detén la API, vuelve a iniciarla y repite `GET /api/v1/clientes/{id}`: los datos siguen en SQL Server.

### ZIP de entrega

Desde la raíz, en PowerShell:

```powershell
./scripts/Armar-Entrega.ps1
```

Genera `artifacts/SGCPapi-WebIII.zip`. El archivo queda fuera de Git. Incluye el proyecto completo, las migraciones de SQL Server (`Datos/MigracionesSqlServer`) y de MySQL, el README, el PDF de evidencias y las pruebas HTTP. Excluye `bin`, `obj` y `.vs`, y también `.git`, `artifacts`, `TestResults` y los archivos locales de secretos (`*.user`, `appsettings.Development.json`, `appsettings.Local.json`). La cadena de conexión de ejemplo está en este README; no hay credenciales reales dentro del ZIP. El script rechaza el archivo si falta el README, la migración de SQL Server, el PDF o las pruebas, o si se coló alguna carpeta prohibida.

Para dejar el ZIP en otra ruta: `./scripts/Armar-Entrega.ps1 C:\entrega\SGCPapi-WebIII.zip`.

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
dotnet ef database update --project src/SGCP.Api --context SgcpContext
dotnet run --project src/SGCP.Api
```

La primera ejecución siembra permisos, responsabilidades y la cuenta `admin` con la contraseña configurada. La operación es idempotente y no sobrescribe cuentas existentes. La migración crea el esquema; el sembrado de datos se ejecuta al arrancar la API. No se cargan precios, clientes, cotizaciones ni pedidos ficticios.

API local: `http://localhost:5081/api/v1/`. Swagger: `http://localhost:5081/swagger`. Para acceso desde otro equipo, configura la escucha en la IP de red y permite solo la red del equipo en el firewall. Los clientes deben usar esa dirección en lugar de `localhost`.

## Usar SQL Server en lugar de MySQL

La API funciona con **MySQL 8** (valor por defecto) o con **SQL Server 2019 o posterior**, incluido LocalDB y Express. El motor se elige con `BaseDatos:Proveedor`, y cada motor tiene su propia migración:

| Motor | `BaseDatos:Proveedor` | Contexto de EF Core | Migraciones |
|---|---|---|---|
| MySQL 8 | `MySql` (o vacío) | `SgcpContext` | `src/SGCP.Api/Datos/Migraciones/` |
| SQL Server | `SqlServer` | `SgcpContextSqlServer` | `src/SGCP.Api/Datos/MigracionesSqlServer/` |

**Como hay dos contextos, todo comando `dotnet ef` lleva `--context`:** `SgcpContext` para MySQL y `SgcpContextSqlServer` para SQL Server. Sin él, EF responde «More than one DbContext was found».

El modelo es el mismo. Solo cambian tres cosas:

- Las columnas calculadas usan `[Columna]` en lugar de `` `Columna` ``.
- El contenido de las evidencias va en `varbinary(max)` en lugar de `mediumblob`.
- La unicidad condicional cambia de forma. MySQL usa un índice único sobre una columna calculada `CASE WHEN condición THEN Columna END`. SQL Server no permite filtrar por una columna calculada, así que ahí es un índice único filtrado sobre la columna real, `... WHERE condición`. Por ejemplo, una sola recepción CONFIRMADA por entrega: `UNIQUE (EntregaId) WHERE [Estado] = 'CONFIRMADA'`.

**1. Configurar la conexión local** con user-secrets, sin dejar credenciales en el repositorio. Las cadenas son ejemplos, no credenciales reales:

```bash
dotnet user-secrets set "BaseDatos:Proveedor" "SqlServer" --project src/SGCP.Api
# Windows, autenticación integrada (LocalDB o instancia local):
dotnet user-secrets set "ConnectionStrings:Sgcp" "Server=(localdb)\MSSQLLocalDB;Database=sgcp;Trusted_Connection=True;TrustServerCertificate=True" --project src/SGCP.Api
# O con usuario SQL (Linux, Docker o servidor remoto):
dotnet user-secrets set "ConnectionStrings:Sgcp" "Server=localhost,1433;Database=sgcp;User ID=sgcp_app;Password=CAMBIAR_CLAVE;TrustServerCertificate=True" --project src/SGCP.Api
dotnet user-secrets set "Jwt:Clave" "<clave de al menos 32 bytes en Base64>" --project src/SGCP.Api
dotnet user-secrets set "DatosIniciales:ClaveAdministrador" "<contraseña inicial de admin>" --project src/SGCP.Api
```

**2. Aplicar las migraciones**. Se indica `--context SgcpContextSqlServer`, porque si no EF usa el contexto de MySQL:

```bash
dotnet tool restore
dotnet ef database update --project src/SGCP.Api --context SgcpContextSqlServer
```

También se puede aplicar `database/EsquemaSqlServer.sql`, que es idempotente, desde SSMS o `sqlcmd`.

**3. Cargar los datos de prueba**. Con `DatosIniciales:Ejemplos = true`, el primer arranque siembra los permisos, los roles, la cuenta `admin` y el catálogo de ejemplo. La carga es idempotente: no duplica registros ni sobrescribe cuentas existentes.

```bash
dotnet user-secrets set "DatosIniciales:Ejemplos" "true" --project src/SGCP.Api
dotnet run --project src/SGCP.Api
```

**4. Abrir Swagger** en `http://localhost:5081/swagger`. Pulsa **Authorize**, pega el token que devuelve `POST /api/v1/auth/login` y prueba los endpoints. Si detienes la API y la vuelves a arrancar, los datos siguen en SQL Server. Un `GET` posterior lo demuestra.

**Volver a MySQL:** `dotnet user-secrets remove "BaseDatos:Proveedor" --project src/SGCP.Api` y restaura la cadena de MySQL.

**Comprobación:** el flujo `.github/workflows/sqlserver.yml` levanta SQL Server 2022, revisa que no haya cambios de modelo sin migrar, aplica la migración dos veces y ejecuta el verificador completo con `SGCP_SQLSERVER_TEST`. Para correrlo en tu equipo:

```bash
SGCP_SQLSERVER_TEST="Server=localhost,1433;User ID=sa;Password=CAMBIAR_CLAVE;TrustServerCertificate=True" dotnet run --project tests/SGCP.Verificacion
```

El verificador crea una base `sgcp_pruebas_<uuid>` y la elimina al terminar.

## Migraciones y pruebas

Para repetir los 12 casos de Web III, abre [pruebas/API_Integrador.http](pruebas/API_Integrador.http) en Visual Studio con la API en ejecución. Selecciona el entorno `local` y crea `pruebas/http-client.env.json.user` con `{ "local": { "claveAdministrador": "TU_CLAVE_LOCAL" } }`, usando tu contraseña local de administrador. Los archivos `*.user` están excluidos de Git. Puedes cambiar `api` y `usuarioAdministrador` en ese mismo entorno si usas otro puerto o cuenta.

Ejecuta los bloques de arriba hacia abajo, incluidas las preparaciones de catálogo, precio, cotización aceptada y pedido. Los valores son sintéticos y cada recorrido crea datos propios. El cliente principal se conserva para la persistencia y el DELETE exitoso usa un segundo cliente sin historial. Antes del caso 12, detén y vuelve a iniciar la API con la misma base, manteniendo abierto el editor HTTP. El archivo indica qué código y datos comprobar en cada respuesta.

Las [evidencias de Web III](evidencias/WebIII/LEEME.md) reúnen las capturas de Swagger y los 12 casos, incluida la comparación antes y después de reiniciar la API.

La migración inicial fue generada con **MySql.EntityFrameworkCore**, el proveedor oficial de Oracle, no convertida desde otro motor. `database/InicialSprint1.sql` es el script MySQL para revisión.

No uses `--idempotent` con MySQL: EF genera `IF NOT EXISTS(...) BEGIN ... END`, que es sintaxis de T-SQL y MySQL rechaza. El script del repositorio es el plano, sin esa envoltura; se aplica una sola vez sobre una base vacía. Para reaplicar sin riesgo usá `dotnet ef database update`, que sí consulta `__EFMigrationsHistory`.

```bash
dotnet build --warnaserror
dotnet ef migrations has-pending-model-changes --project src/SGCP.Api --context SgcpContext
dotnet ef migrations script --project src/SGCP.Api --context SgcpContext --output artifacts/EsquemaCompleto.sql
dotnet run --project tests/SGCP.Verificacion
```

Sin `SGCP_MYSQL_TEST`, las pruebas usan SQLite en un archivo temporal solo para verificar comportamiento: **esto no certifica MySQL**. La aplicación principal únicamente referencia el proveedor MySQL; SQLite pertenece al proyecto de pruebas.

Para verificar con MySQL, configura `SGCP_MYSQL_TEST` con una conexión de pruebas o ejecuta `scripts/Verificar-MySql.ps1`. El verificador crea una base `sgcp_pruebas_<uuid>`, aplica la migración dos veces, comprueba que no queden migraciones pendientes, ejecuta el recorrido HTTP y elimina únicamente esa base aislada al terminar. El usuario necesita CREATE DATABASE; concedelo acotado al prefijo con ``GRANT ALL ON `sgcp%`.* TO 'sgcp'@'%';`` en lugar de dar permiso sobre todo el servidor.

Aparte de esa verificación completa hay una corta, `scripts/Verificar-Sprint1.py`, que recorre por HTTP el criterio de aceptación del Sprint 1 contra una API **ya levantada**: que el pedido se niegue si la cotización no está aceptada, que el pedido confirmado guarde costo Bs 112,00 y margen Bs 28,00, y que una vigencia de precio posterior no recalcule ese margen. Sirve para comprobar en segundos la base que se va a mostrar en una demostración; no reemplaza a `tests/SGCP.Verificacion`, que aplica las migraciones en una base aislada y cubre los cuatro sprints.

```bash
SGCP_CLAVE_ADMIN='la-clave-del-administrador' python3 scripts/Verificar-Sprint1.py
```

Necesita Python 3.8 o posterior y no usa paquetes externos. La dirección se cambia con `SGCP_API` y el usuario con `SGCP_USUARIO`. Deja en la base los registros que crea, con códigos marcados con la hora para no chocar con los índices únicos.

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
| Clientes | `GET/POST clientes`, `GET/PUT/DELETE clientes/{id}`, `GET clientes/{id}/pedidos`, `POST clientes/{id}/activar`, `desactivar` |
| Clasificaciones | `GET/POST clasificaciones`, `GET/PUT clasificaciones/{id}`, `POST clasificaciones/{id}/estado` |
| Productos | `GET/POST productos`, `GET/PUT productos/{id}`, `POST productos/{id}/estado` |
| Costos | `GET/POST productos/{id}/precios`, `PUT precios/{id}`, `POST precios/{id}/finalizar` |
| Cotizaciones | `GET/POST cotizaciones`, `GET/PUT cotizaciones/{id}`, `POST .../presentar`, `aceptar`, `rechazar`, `nueva-version` |
| Pedidos | `GET/POST pedidos`, `GET pedidos/{id}`, `POST pedidos/{id}/cancelar` |
| Resumen | `GET resumen`, filtrado según permisos |

**Clientes como recurso REST (Web III).** Responde con el DTO `ClienteRespuesta`, nunca con la entidad de EF Core. Pedidos es su recurso relacionado.

| Operación | Éxito | Errores |
|---|---|---|
| `GET clientes` | 200, paginado | 400 si la paginación es inválida |
| `GET clientes/{id}` | 200 | 404 `NO_ENCONTRADO` |
| `POST clientes` | **201 con cabecera `Location`** a `clientes/{id}` | 400 si faltan campos obligatorios; 409 `CLIENTE_DUPLICADO` |
| `PUT clientes/{id}` | 200 | 400, 404, 409 |
| `DELETE clientes/{id}` | 204, con registro `ELIMINAR_CLIENTE` en la auditoría | 404; **409 `CLIENTE_CON_HISTORIAL`** si tiene cotizaciones, pedidos, pagos o cuenta de comprador (se desactiva en lugar de borrarse, para no romper la trazabilidad de saldos) |
| `GET clientes/{id}/pedidos` | 200, paginado: código, fecha, estado comercial y total | 404 si el cliente no existe; 403 si un comprador pide los de otro cliente |

`POST productos` y `POST cotizaciones` también responden 201 con `Location`.

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
| v4.3.2 | 2026-11-20 | 4 | Resolver incidencias para Producción (endpoint, filtro opcional y verificación) sin rehabilitar el animal ni el lote | Sofia Cuellar Roca |
| v4.1.4 | 2026-11-19 | 4 | Corrección (W3-P1): PDF de evidencias rehecho en A4, una prueba por página con la captura de Swagger a todo el ancho, identificada por operación y código HTTP | Dario Haziel Tambo Parra |
| v4.1.3 | 2026-11-19 | 4 | Web III (W3-P1): sección de entrega al inicio del README (proyecto, versión de .NET, integrantes con usuario de GitHub, repositorio, recurso evaluado y pasos con SQL Server) y PDF de evidencias `docs/Evidencias_API_Integrador.pdf` armado con las capturas de `evidencias/WebIII` | Dario Haziel Tambo Parra |
| v4.4.1 | 2026-11-18 | 4 | Web III (W3-P4): comentarios XML y respuestas de Swagger en Productos, Cotizaciones y Pedidos, contacto del equipo y 12 casos reproducibles en `pruebas/API_Integrador.http`, con preparación del historial comercial y comprobación de persistencia tras reiniciar la API; capturas en `evidencias/WebIII` | Victor Manuel Manrique Arancivia |
| v4.2.0 | 2026-11-18 | 4 | Web III (W3-P2): Clientes como recurso REST. DTO de salida `ClienteRespuesta`; `POST` con 201 y `Location` en clientes, productos y cotizaciones; `DELETE clientes/{id}` con la regla `CLIENTE_CON_HISTORIAL` (409) y auditoría; `GET clientes/{id}/pedidos` con aislamiento del comprador; `ProducesResponseType` y comentarios en `ClientesController`; 20 comprobaciones nuevas en el verificador | Angel Sebastian Eguez Mur |
| v4.1.2 | 2026-11-18 | 4 | SQL Server como segundo motor, elegible con `BaseDatos:Proveedor`: `SgcpContextSqlServer` con migración propia (`Datos/MigracionesSqlServer`) y script `database/EsquemaSqlServer.sql`; manejo de interbloqueos de SQL Server; verificador con `SGCP_SQLSERVER_TEST` y flujo de CI con SQL Server 2022. MySQL sigue siendo el motor por defecto y no cambia | Dario Haziel Tambo Parra |
| v4.1.1 | 2026-11-18 | 4 | Corrección pedida por la web (S3/S4-P2): `GET recepciones/conflictos` y `GET recepciones/{id}` devuelven la versión y el estado actuales de la entrega, para que Ventas revise conflictos sin `entregas.administrar`; el reporte ENTREGAS trae la columna «Entrega» con el id, y `GET pedidos/{pedidoId}/constancia` emite la constancia a Ventas y al comprador conociendo solo el pedido | Dario Haziel Tambo Parra |
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
| Frío e incidencias | `GET/POST controles-frio`, `GET incidencias?resuelta=`, `POST incidencias/{id}/resolver`. Frío referencia un lote o una entrega y usa °C con evaluación humana |
| Cargos | `GET/POST cargos`, `POST cargos/{id}/anular`. El POST exige acuerdoServicio y prestadoEn para servicios ya prestados; los cargos de producto nacen al confirmar el pedido |
| Condición acordada | `GET/POST pedidos/{id}/condiciones-pago` |
| Cobranza | `GET/POST pagos`, `POST pagos/{id}/anular`, `GET saldos?clienteId=...&pedidoId=...` |
| Señas | `GET/POST senas`, `POST senas/{id}/aplicar`, `resolver` |
| Transporte | `GET/POST transportes`, `PUT transportes/{id}`, `POST transportes/{id}/estado`, `GET responsables-entrega` |
| Rutas | `GET/POST rutas`, `POST rutas/{id}/finalizar` |
| Entregas | `GET/POST entregas`, `POST entregas/{id}/despachar`, `reasignar`, `GET mis-entregas` |
| Recepción | `POST recepciones/sincronizar`, `GET recepciones/{id}` (incluye `entrega.version`), `GET recepciones/conflictos` (incluye `versionEntrega`, `estadoEntrega` y `pedido`), `POST recepciones/{id}/revisar` (`versionActual` = la versión leída), `GET recepciones/{id}/evidencias/{evidenciaId}` |
| Reportes | `GET reportes` devuelve tipos permitidos; `GET reportes/{tipo}`, `GET reportes/{tipo}/exportar/pdf`, `.../xlsx` |
| Constancia | `GET entregas/{id}/constancia` o `GET pedidos/{pedidoId}/constancia` (toma la entrega ENTREGADA más reciente del pedido; el id de la entrega también sale en el reporte ENTREGAS): PDF con receptor, peso/unidad, fechas UTC, saldo histórico, hashes y evidencia gráfica; solo recepción conforme confirmada y acceso de administración/ventas o comprador propio |
| Fiscal condicional | `GET/POST pedidos/{pedidoId}/registro-fiscal`: referencia estable, estado SIN_PROVEEDOR, sin emisión fiscal real |

### Reglas de operación y migración

- Las órdenes admiten previsiones con producto, cantidad estimada y unidad; no generan disponibilidad ni rendimientos garantizados. El catálogo nunca genera stock. Un faeneo requiere animal aprobado; una fabricación consume existencias aptas no reservadas. La confirmación crea varios lotes con producto y clasificación, todavía sin cantidad confirmada. Pesaje confirma bruto/tara/neto; entrada física incrementa stock una sola vez por lote y producto.
- La recepción externa documentada permite abastecer sin obligar a faenar un animal. Debe pasar igualmente por pesaje y entrada física. Cada consumo envía referenciaUnica UUID, existenciaId y cantidad: reintentar con el mismo contenido devuelve el original; cambiar el contenido con la misma referencia se rechaza. Una existencia tiene una ubicación textual; no hay transferencias entre cámaras ni reposiciones parciales sobre el mismo lote en este alcance.
- Una reserva se puede repartir entre lotes: una abierta por detalle/existencia; la suma no puede exceder el detalle. El despacho exige reserva completa, lote apto y condición comercial satisfecha. Descuenta stock una sola vez y conserva el movimiento. Cancelar un pedido exige acuerdo de resolución para sus señas, libera reservas y marca CANCELADA su entrega preparada; después de despachar requiere resolución comercial.
- El rechazo bloquea los lotes afectados y sus derivados, libera reservas activas y registra incidencias. Nunca suma de vuelta insumos ya consumidos. Resolver una incidencia guarda el motivo y la fecha, y no rehabilita el animal ni el lote. Las anulaciones conservan fila, motivo y auditoría; no existe DELETE físico.
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
