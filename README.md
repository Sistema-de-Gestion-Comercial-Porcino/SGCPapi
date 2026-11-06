# SGCPapi

API del Sistema de Gestión Comercial Porcino. ASP.NET Core 10 con controladores, Entity Framework Core 10 y **MySQL 8**. La web MVC y Android consumen esta API; no acceden directamente a la base de datos.

## Equipo

- Dario Haziel Tambo Parra
- Angel Sebastian Eguez Mur
- Sofia Cuellar Roca
- Victor Manuel Manrique Arancivia

## Sprint 1

Acceso, registro público de comprador, recuperación de contraseña, usuarios y permisos, clientes, clasificaciones, productos, vigencias de costo/precio, cotizaciones versionadas y pedidos con margen histórico. CU01–CU07. Builder construye el pedido; no se implementan aún inventario, producción, cobros, reparto operativo ni exportación.

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

No hace falta crear la base a mano: `CREATE DATABASE sgcp;` basta, y la migración construye las tablas. En PowerShell 7, `./scripts/Preparar-Desarrollo.ps1` pide los valores sin mostrarlos, guarda secretos y ejecuta la migración. Si usas Visual Studio/PowerShell anterior, configura las claves con el administrador de secretos del proyecto.

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
dotnet ef migrations script --project src/SGCP.Api --output artifacts/InicialSprint1.sql
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

Para levantar el sistema completo (API, web y Android) en orden, hay una guía del equipo en `Modelado_Chancheria/00_Definicion/Como_ejecutar_el_sistema.md`, con la configuración de MySQL local, el recorrido de comprobación y una tabla de errores frecuentes.

El motor pasó de SQL Server a MySQL el 6 de noviembre de 2026. Qué se cambió, por qué el proveedor de Oracle y no Pomelo, y las tres trampas de MySQL: `Modelado_Chancheria/00_Definicion/Cambio_a_MySQL.md`.
