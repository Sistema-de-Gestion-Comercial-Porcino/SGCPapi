using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SGCP.Api;
using SGCP.Api.Datos;

var archivo = Path.Combine(Path.GetTempPath(), "sgcp-pruebas-" + Guid.NewGuid() + ".db");
var sql = Environment.GetEnvironmentVariable("SGCP_MYSQL_TEST");
var servidor = args.Contains("--servidor");
var puerto = servidor ? 5091 : 5093;
var cadena = sql is null ? null : new MySqlConnectionStringBuilder(sql) { Database = "sgcp_pruebas_" + Guid.NewGuid().ToString("N") }.ConnectionString;
var app = Arranque.Crear(["--environment", "Testing", "--urls", $"http://127.0.0.1:{puerto}"], b =>
{
    b.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Clave"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)), ["Jwt:Emisor"] = "SGCPapi", ["Jwt:Audiencia"] = "SGCPclientes", ["DatosIniciales:ClaveAdministrador"] = "Prueba-local-2026!", ["DatosIniciales:Ejemplos"] = "true", ["DatosIniciales:Habilitar"] = "false", ["Recuperacion:CarpetaLocal"] = archivo + "-correos", ["Logging:LogLevel:Default"] = "Warning" });
    b.Services.RemoveAll<SgcpContext>();
    b.Services.RemoveAll<DbContextOptions<SgcpContext>>();
    b.Services.RemoveAll<IDbContextOptionsConfiguration<SgcpContext>>();
    b.Services.AddDbContext<SgcpContext>(o => { if (cadena is null) o.UseSqlite("Data Source=" + archivo); else o.UseMySQL(cadena); });
});
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SgcpContext>();
    if (cadena is null)
        await db.Database.EnsureCreatedAsync();
    else
    { await db.Database.MigrateAsync(); await db.Database.MigrateAsync(); if ((await db.Database.GetPendingMigrationsAsync()).Any()) throw new Exception("Migraciones pendientes tras database update."); }
    await DatosIniciales.Sembrar(db, app.Configuration);
}
await app.StartAsync();
Console.WriteLine(cadena is null ? "Motor de pruebas: SQLite aislado. No valida la ejecución de migraciones MySQL." : "Motor de pruebas: MySQL, migración aplicada en una base aislada.");
if (servidor) { Console.WriteLine("Servidor de verificación disponible en http://127.0.0.1:5091"); await app.WaitForShutdownAsync(); return; }
using var http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{puerto}/api/v1/") };
var cuenta = 0;
void Comprobar(bool condicion, string nombre) { if (!condicion) throw new Exception("FALLÓ: " + nombre); Console.WriteLine("OK " + (++cuenta) + " · " + nombre); }
async Task<(int Estado, JsonNode? Json)> Llamar(string metodo, string ruta, object? cuerpo = null)
{
    using var req = new HttpRequestMessage(new HttpMethod(metodo), ruta);
    if (cuerpo is not null)
        req.Content = JsonContent.Create(cuerpo);
    using var res = await http.SendAsync(req);
    var texto = await res.Content.ReadAsStringAsync();
    JsonNode? json = string.IsNullOrWhiteSpace(texto) ? null : JsonNode.Parse(texto);
    if ((int)res.StatusCode >= 500)
        throw new Exception($"{metodo} {ruta}: {texto}");
    return ((int)res.StatusCode, json);
}
async Task<JsonNode> Exito(string metodo, string ruta, object? cuerpo = null)
{
    var r = await Llamar(metodo, ruta, cuerpo);
    if (r.Estado >= 400)
        throw new Exception($"{metodo} {ruta}: {r.Estado} {r.Json}");
    return r.Json ?? new JsonObject();
}
string Id(JsonNode n) => n["id"]!.GetValue<string>();
try
{
    Comprobar((await Llamar("GET", "clientes")).Estado == 401, "API exige autenticación");
    var acceso = await Exito("POST", "auth/login", new { login = "admin", clave = "Prueba-local-2026!" });
    var token = acceso["token"]!.GetValue<string>();
    http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    var admin = Id(acceso["usuario"]!);
    Comprobar((await Llamar("POST", $"usuarios/{admin}/estado", new { estado = "DESHABILITADO" })).Json?["codigo"]?.ToString() == "ULTIMO_ADMINISTRADOR", "Último administrador protegido");
    var roles = (await Exito("GET", "roles")).AsArray();
    var ventas = Id(roles.First(x => x!["nombre"]!.ToString() == "VENTAS")!);
    var vendedor = await Exito("POST", "usuarios", new { nombre = "Ventas prueba", login = "ventas.prueba", correo = "ventas@prueba.local", telefono = "70000000", clave = "Ventas-prueba-2026!", roles = new[] { ventas } });
    Comprobar((await Llamar("PUT", $"usuarios/{admin}", new { nombre = "Administración", login = "admin", correo = "admin@sgcp.local", roles = new[] { ventas } })).Json?["codigo"]?.ToString() == "ULTIMO_ADMINISTRADOR", "No se puede retirar el último rol administrador");
    var productos = await Exito("GET", "productos");
    var producto = productos["elementos"]!.AsArray().First(x => x!["codigo"]!.ToString() == "PRD-001")!;
    var pid = Id(producto);
    var cid = producto["clasificacionProductoId"]!.ToString();
    Comprobar(!producto.AsObject().ContainsKey("stock"), "Alta de producto no crea existencias");
    await Exito("POST", $"productos/{pid}/precios", new { costoReferencia = 28, precioVenta = 35, vigenteDesde = DateTime.UtcNow.AddMinutes(-10) });
    Comprobar((await Llamar("PUT", $"productos/{pid}", new { codigo = "PRD-001", nombre = "Costilla", presentacion = "Pieza", unidadVenta = "unidad", descripcion = "", clasificacionProductoId = cid })).Json?["codigo"]?.ToString() == "UNIDAD_INCOMPATIBLE", "Unidad no cambia si tiene precios");
    Comprobar((await Llamar("POST", $"clasificaciones/{cid}/estado", new { activo = false })).Json?["codigo"]?.ToString() == "CLASIFICACION_EN_USO", "Clasificación conserva productos activos");
    await Exito("POST", "auth/registro-comprador", new { nombre = "Comprador prueba", login = "comprador.prueba", correo = "comprador@prueba.local", clave = "Comprador-prueba-2026!", telefono = "71111111" });
    var comprador = await Exito("POST", "auth/login", new { login = "comprador.prueba", clave = "Comprador-prueba-2026!" });
    var compradorToken = comprador["token"]!.ToString();
    var cliente = comprador["usuario"]!["clienteId"]!.ToString();
    object Cotizacion(string client) => new { clienteId = client, canal = "WHATSAPP", vigenteHasta = DateTime.UtcNow.AddHours(2), detalles = new[] { new { productoId = pid, cantidad = 4, unidad = "kg", precioUnitario = 35 } } };
    object Confirmar(string cot) => new { cotizacionId = cot, fechaComprometida = DateTime.UtcNow.AddDays(1), condicionPago = "CREDITO_ACORDADO" };
    var cot = Id(await Exito("POST", "cotizaciones", Cotizacion(cliente)));
    await Exito("POST", $"cotizaciones/{cot}/presentar");
    Comprobar((await Llamar("POST", "pedidos", Confirmar(cot))).Json?["codigo"]?.ToString() == "COTIZACION_NO_ACEPTADA", "Presentada no crea pedido");
    await Exito("POST", $"cotizaciones/{cot}/rechazar", new { motivo = "Necesita cambiar cantidades" });
    Comprobar((await Llamar("POST", "pedidos", Confirmar(cot))).Json?["codigo"]?.ToString() == "COTIZACION_NO_ACEPTADA", "Rechazada no crea pedido");
    var nueva = Id(await Exito("POST", $"cotizaciones/{cot}/nueva-version", new { vigenteHasta = DateTime.UtcNow.AddHours(3) }));
    Comprobar((await Exito("GET", $"cotizaciones/{nueva}"))["version"]!.GetValue<int>() == 2, "Nueva versión conserva vínculo e historial");
    await Exito("POST", $"cotizaciones/{nueva}/presentar");
    await Exito("POST", $"cotizaciones/{nueva}/aceptar");
    await Exito("POST", $"productos/{pid}/precios", new { costoReferencia = 28, precioVenta = 40, vigenteDesde = DateTime.UtcNow.AddMinutes(-5) });
    var pedido = Id(await Exito("POST", "pedidos", Confirmar(nueva)));
    var pv = await Exito("GET", $"pedidos/{pedido}");
    Comprobar(pv["detalles"]![0]!["precioAcordado"]!.GetValue<decimal>() == 35, "Pedido toma precio aceptado, aunque el catálogo cambió");
    Comprobar(pv["margenEstimadoConfirmado"]!.GetValue<decimal>() == 28, "Margen de 4 kg × (35 − 28) = Bs 28");
    await Exito("POST", $"productos/{pid}/precios", new { costoReferencia = 30, precioVenta = 45, vigenteDesde = DateTime.UtcNow.AddMinutes(-1) });
    Comprobar((await Exito("GET", $"pedidos/{pedido}"))["margenEstimadoConfirmado"]!.GetValue<decimal>() == 28, "Nueva vigencia conserva el margen histórico");
    Comprobar((await Llamar("POST", "pedidos", Confirmar(nueva))).Json?["codigo"]?.ToString() == "COTIZACION_YA_TIENE_PEDIDO", "Una cotización no duplica pedidos");
    Comprobar((await Llamar("POST", $"clientes/{cliente}/desactivar")).Json?["codigo"]?.ToString() == "CLIENTE_CON_PEDIDOS", "Cliente con pedido pendiente no se desactiva");
    var ajeno = Id(await Exito("POST", "clientes", new { nombre = "Otro cliente", tipo = "Restaurante", telefono = "72222222", direccion = "Local", identificacionFiscal = "123", condicionHabitual = "CONTADO", observaciones = "" }));
    var ajena = Id(await Exito("POST", "cotizaciones", Cotizacion(ajeno)));
    http.DefaultRequestHeaders.Authorization = new("Bearer", compradorToken);
    Comprobar((await Llamar("GET", "usuarios")).Estado == 403, "Comprador sin permisos internos");
    Comprobar((await Llamar("GET", $"cotizaciones/{ajena}")).Estado == 403, "Comprador no consulta cotizaciones ajenas");
    var cat = await Exito("GET", $"productos/{pid}");
    Comprobar(cat["precioVigente"]?["costoReferencia"] is null, "Catálogo no filtra costos al comprador");
    var propio = await Exito("GET", $"pedidos/{pedido}");
    Comprobar(propio["margenEstimadoConfirmado"] is null && propio["detalles"]![0]!["costoReferenciaAcordado"] is null, "Pedido oculta margen y costo al comprador");
    http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    await Exito("POST", $"pedidos/{pedido}/cancelar", new { motivo = "Cancelación acordada" });
    Comprobar((await Exito("GET", $"pedidos/{pedido}"))["motivoCancelacion"]!.ToString() == "Cancelación acordada", "Cancelación conserva motivo e histórico");
    var caduca = Id(await Exito("POST", "cotizaciones", Cotizacion(ajeno)));
    await Exito("POST", $"cotizaciones/{caduca}/presentar");
    await Exito("POST", $"cotizaciones/{caduca}/aceptar");
    await using (var scope = app.Services.CreateAsyncScope())
    { var db = scope.ServiceProvider.GetRequiredService<SgcpContext>(); var c = await db.Cotizaciones.FindAsync(Guid.Parse(caduca)); c!.VigenteHasta = DateTime.UtcNow.AddMinutes(-1); await db.SaveChangesAsync(); }
    Comprobar((await Llamar("POST", "pedidos", Confirmar(caduca))).Json?["codigo"]?.ToString() == "COTIZACION_NO_VIGENTE", "Aceptada fuera de vigencia tampoco crea pedido");
    var recuperar = await Exito("POST", "auth/recuperar", new { correo = "comprador@prueba.local" });
    Comprobar(recuperar["token"] is null, "Recuperación no entrega token al solicitante anónimo");
    var correo = await File.ReadAllTextAsync(Directory.GetFiles(archivo + "-correos").Single());
    var rt = correo.Split("token=")[1].Trim();
    await Exito("POST", "auth/restablecer", new { token = rt, clave = "Nueva-clave-2026!" });
    Comprobar((await Llamar("POST", "auth/restablecer", new { token = rt, clave = "Otra-clave-2026!" })).Estado == 400, "Token de recuperación de un solo uso");
    http.DefaultRequestHeaders.Authorization = new("Bearer", compradorToken);
    Comprobar((await Llamar("GET", "auth/yo")).Estado == 401, "Restablecer revoca sesiones anteriores");
    http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    var loginVentas = await Exito("POST", "auth/login", new { login = "ventas.prueba", clave = "Ventas-prueba-2026!" });
    await Exito("POST", $"usuarios/{Id(vendedor)}/estado", new { estado = "DESHABILITADO" });
    http.DefaultRequestHeaders.Authorization = new("Bearer", loginVentas["token"]!.ToString());
    Comprobar((await Llamar("GET", "clientes")).Estado == 401, "Desactivar revoca JWT activo");
    http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    await Exito("POST", $"usuarios/{Id(vendedor)}/estado", new { estado = "ACTIVO" });
    for (var i = 0; i < 5; i++)
        await Llamar("POST", "auth/login", new { login = "ventas.prueba", clave = "incorrecta" });
    Comprobar((await Llamar("POST", "auth/login", new { login = "ventas.prueba", clave = "Ventas-prueba-2026!" })).Estado == 403, "Cinco fallos bloquean la cuenta");
    Comprobar((await Exito("GET", "auditoria"))["totalRegistros"]!.GetValue<int>() > 20, "Operaciones auditadas");
    await Exito("POST", "auth/cerrar-sesion");
    Comprobar((await Llamar("GET", "auth/yo")).Estado == 401, "Cerrar sesión invalida el token");
    Console.WriteLine($"RESULTADO: {cuenta} comprobaciones correctas.");
}
finally
{
    await app.StopAsync();
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<SgcpContext>().Database.EnsureDeletedAsync();
    if (Directory.Exists(archivo + "-correos"))
        Directory.Delete(archivo + "-correos", true);
    await app.DisposeAsync();
}
