using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;

public static class Perfiles
{
    public static async Task Migracion(SgcpContext db, IConfiguration config)
    {
        var admin = await db.Usuarios.Include(u => u.Roles).SingleAsync(u => u.Login == "admin");
        var id = admin.Id; var hash = admin.HashClave;
        var legado = new Rol { Nombre = "ADMINISTRACION", Descripcion = "Legado" };
        db.Roles.Add(legado);
        admin.Roles.Clear(); admin.Roles.Add(legado);
        await db.SaveChangesAsync();
        await DatosIniciales.Sembrar(db, config);
        await DatosIniciales.Sembrar(db, config);
        if (admin.Id != id || admin.HashClave != hash || !admin.Roles.Any(r => r.Nombre == "SUPERADMIN") || legado.Activo)
            throw new Exception("Migración no conserva identidad/contraseña o no es idempotente.");
        Console.WriteLine("OK PERFILES · Migración idempotente conserva identidad, contraseña y retira rol antiguo.");
    }

    public static async Task Ejecutar(HttpClient http)
    {
        var n = 0;
        void Check(bool ok, string nombre) { if (!ok) throw new Exception("FALLÓ PERFILES: " + nombre); Console.WriteLine($"OK PERFILES {++n} · {nombre}"); }
        async Task<(int Estado, JsonNode? Datos)> Call(string method, string url, object? body = null)
        {
            using var req = new HttpRequestMessage(new HttpMethod(method), url);
            if (body is not null) req.Content = JsonContent.Create(body);
            using var res = await http.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();
            return ((int)res.StatusCode, string.IsNullOrEmpty(text) ? null : JsonNode.Parse(text));
        }
        async Task<JsonNode> Ok(string method, string url, object? body = null)
        { var r = await Call(method, url, body); if(r.Estado >= 400) throw new Exception($"{url}: {r.Estado} {r.Datos}"); return r.Datos ?? new JsonObject(); }
        async Task<JsonNode> Login(string login)
        {
            var r = await Ok("POST", "auth/login", new { login, clave = login == "admin" ? "Prueba-local-2026!" : "Perfil-prueba-2026!" });
            http.DefaultRequestHeaders.Authorization = new("Bearer", r["token"]!.ToString()); return r;
        }
        var root = await Login("admin");
        var rootId = root["usuario"]!["id"]!.ToString();
        var rootToken = root["token"]!.ToString();
        var roles = (await Ok("GET", "roles")).AsArray().ToDictionary(r => r!["nombre"]!.ToString(), r => r!["id"]!.ToString());
        object Cuenta(string login, params string[] r) => new { nombre = login, login, correo = login + "@prueba.local", clave = "Perfil-prueba-2026!", roles = r.Select(x=>roles[x]).ToArray() };
        var owner = await Ok("POST", "usuarios", Cuenta("propietario.perfil", "PROPIETARIO"));
        var operador = await Ok("POST", "usuarios", Cuenta("administrador.perfil", "ADMINISTRADOR", "VENTAS"));
        var otro = await Ok("POST", "usuarios", Cuenta("operador.perfil", "PRODUCCION"));
        await Ok("POST", "usuarios", Cuenta("finanzas.perfil", "FINANZAS"));
        Check((await Call("POST", $"usuarios/{rootId}/estado", new { estado = "DESHABILITADO" })).Datos?["codigo"]?.ToString() == "ULTIMO_ADMINISTRADOR", "Último superadmin protegido");
        var a = await Login("administrador.perfil");
        var permisos = a["usuario"]!["permisos"]!.AsArray().Select(p=>p!.ToString()).ToArray();
        Check(!permisos.Contains("costos.consultar") && !permisos.Contains("pagos.administrar"), "Administrador + Ventas no obtiene finanzas indirectamente");
        Check(!permisos.Contains("saldos.consultar") && (await Call("GET", "saldos")).Estado == 403,
            "ADMINISTRADOR sigue sin saldos.consultar");
        Check((await Call("GET", "auditoria")).Estado == 403, "Administrador sin auditoría estratégica");
        Check((await Call("POST", "usuarios", Cuenta("escalamiento.perfil", "SUPERADMIN"))).Estado == 403, "Administrador no crea superadmins");
        Check((await Call("POST", $"usuarios/{owner["id"]}/estado", new { estado = "DESHABILITADO" })).Estado == 403, "Administrador no desactiva propietario");
        Check((await Call("POST", $"usuarios/{operador["id"]}/estado", new { estado = "DESHABILITADO" })).Estado == 403, "Administrador no cambia sus privilegios");
        Check((await Call("POST", "usuarios", Cuenta("empleado.perfil", "ALMACEN"))).Estado == 201, "Administrador crea empleados operativos");
        Check((await Call("POST", "usuarios", Cuenta("finanzas.ilegal", "FINANZAS"))).Estado == 403, "Administrador no concede autorización financiera");
        var finanzas = await Login("finanzas.perfil");
        var permisosFinanzas = finanzas["usuario"]!["permisos"]!.AsArray().Select(p => p!.ToString()).ToHashSet();
        Check(new[] { "clientes.consultar", "catalogo.consultar", "pedidos.consultar", "precios.consultar", "reportes.consultar" }.All(permisosFinanzas.Contains)
            && (await Call("GET", "clientes")).Estado == 200 && (await Call("GET", "productos")).Estado == 200,
            "FINANZAS consulta clientes y catálogo para cobrar");
        Check((await Call("GET", "pedidos")).Estado == 200 && (await Call("GET", "saldos")).Estado == 200
            && (await Call("GET", "seguimientos-cobro")).Estado == 200 && (await Call("GET", "reportes")).Estado == 200,
            "FINANZAS consulta pedidos, saldos, seguimientos y reportes");
        Check(!permisosFinanzas.Contains("usuarios.administrar") && !permisosFinanzas.Contains("catalogo.administrar"),
            "FINANZAS no recibe administración de usuarios ni catálogo");
        await Login("propietario.perfil");
        Check((await Call("POST", "usuarios", Cuenta("nuevo.admin.perfil", "ADMINISTRADOR"))).Estado == 201, "Propietario crea administradores");
        Check((await Call("POST", $"usuarios/{rootId}/estado", new { estado = "BLOQUEADO" })).Estado == 403, "Propietario no bloquea superadmin");
        Check((await Call("GET", "auditoria")).Estado == 200, "Propietario consulta auditoría");
        var reportes = (await Ok("GET", "reportes")).AsArray();
        Check(reportes.Count == 7, "Propietario tiene los siete reportes existentes");
        await Ok("PUT", "usuarios/" + operador["id"], Cuenta("administrador.perfil", "ADMINISTRADOR", "VENTAS", "FINANZAS"));
        var autorizado = await Login("administrador.perfil");
        Check(autorizado["usuario"]!["permisos"]!.AsArray().Any(p=>p!.ToString()=="costos.consultar"), "Autorización financiera explícita del propietario");
        http.DefaultRequestHeaders.Authorization = new("Bearer", rootToken);
        var cliente = await Ok("POST", "clientes", new { nombre="Cliente documentos", tipo="Persona", telefono="70000000", identificacionFiscal=" CI 123 ", tipoDocumento="CI", condicionHabitual="CONTADO", direccion="", observaciones="" });
        var cid = cliente["id"]!.ToString();
        Check(cliente["identificacionFiscal"]!.ToString()=="CI123", "Documento normalizado");
        await Ok("POST", $"clientes/{cid}/desactivar");
        Check((await Call("POST", "clientes", new { nombre="Duplicado", tipo="Persona", telefono="70000000", identificacionFiscal="CI123", condicionHabitual="CONTADO", direccion="", observaciones="" })).Estado == 409, "Documento único también para cliente desactivado");
        var tabla = await Ok("GET", "reportes/CLIENTES");
        Check(tabla["columnas"]!.AsArray().All(c=>c!.ToString()!="Id") && !tabla["filas"]!.ToJsonString().Contains(cid), "Reporte clientes no muestra GUID");
        foreach(var f in new[]{"pdf","xlsx"}) { using var res=await http.GetAsync("reportes/CLIENTES/exportar/"+f); Check(res.IsSuccessStatusCode, "Exportación " + f + " autorizada"); }
        Console.WriteLine($"RESULTADO PERFILES: {n} comprobaciones correctas.");
    }
}
