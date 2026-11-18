using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Web III · Clientes como recurso REST (W3-P2): DTO de salida, 201 con Location, DELETE con regla de dominio
/// y pedidos del cliente. Corre después de Sprints234, que deja creado al comprador «comprador.s234» con pedidos
/// y al cliente «Ajeno S234».
/// </summary>
public static class ClientesRest
{
    public static async Task Ejecutar(HttpClient http)
    {
        int total = 0;
        void Check(bool cumple, string nombre)
        {
            if (!cumple)
                throw new Exception("FALLÓ Clientes REST: " + nombre);
            Console.WriteLine("OK P2 " + ++total + " · " + nombre);
        }
        async Task<(HttpStatusCode Estado, JsonNode? Json, Uri? Location)> Call(string method, string path, object? body = null)
        {
            using var req = new HttpRequestMessage(new HttpMethod(method), path);
            if (body is not null)
                req.Content = JsonContent.Create(body);
            using var res = await http.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();
            if ((int)res.StatusCode >= 500)
                throw new Exception(method + " " + path + ": " + text);
            return (res.StatusCode, string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text), res.Headers.Location);
        }
        async Task<string> Login(string login, string clave)
        {
            var r = await Call("POST", "auth/login", new { login, clave });
            return r.Json!["token"]!.ToString();
        }
        void Token(string t) => http.DefaultRequestHeaders.Authorization = new("Bearer", t);
        string Codigo((HttpStatusCode, JsonNode? Json, Uri?) r) => r.Json?["codigo"]?.ToString() ?? "";
        object NuevoCliente(string nombre, string nit) => new
        {
            nombre,
            tipo = "Restaurante",
            telefono = "70000099",
            direccion = "Av. Prueba 1",
            identificacionFiscal = nit,
            condicionHabitual = "CONTADO",
            observaciones = ""
        };

        var admin = await Login("admin", "Prueba-local-2026!");
        Token(admin);

        // POST: 201, Location y cuerpo con el DTO de salida
        var creado = await Call("POST", "clientes", NuevoCliente("Cliente REST", "NITREST1"));
        Check(creado.Estado == HttpStatusCode.Created, "POST clientes responde 201 Created");
        Check(creado.Location is not null && creado.Location.AbsolutePath.EndsWith("/api/v1/clientes/" + creado.Json!["id"], StringComparison.OrdinalIgnoreCase),
            "POST clientes trae la cabecera Location con la URL del cliente");
        Check(creado.Json!["nombre"]?.ToString() == "Cliente REST" && creado.Json["activo"]?.GetValue<bool>() == true
            && creado.Json["fechaAlta"] is not null && creado.Json["pedidos"] is null && creado.Json["cuentasPortal"] is null,
            "El cuerpo del POST es ClienteRespuesta, sin las colecciones de la entidad");
        var id = creado.Json["id"]!.ToString();
        var porLocation = await Call("GET", creado.Location!.ToString());
        Check(porLocation.Estado == HttpStatusCode.OK && porLocation.Json!["id"]!.ToString() == id, "GET a la URL de Location devuelve el mismo cliente");

        // Validación y PUT
        var invalido = await Call("POST", "clientes", new { nombre = "", tipo = "", telefono = "", direccion = "", identificacionFiscal = "", condicionHabitual = "CONTADO", observaciones = "" });
        Check(invalido.Estado == HttpStatusCode.BadRequest, "POST sin campos obligatorios responde 400");
        var editado = await Call("PUT", "clientes/" + id, NuevoCliente("Cliente REST editado", "NITREST1"));
        Check(editado.Estado == HttpStatusCode.OK && editado.Json!["nombre"]?.ToString() == "Cliente REST editado", "PUT clientes/{id} actualiza y devuelve ClienteRespuesta");
        Check((await Call("PUT", "clientes/" + Guid.NewGuid(), NuevoCliente("Nadie", "NITREST9"))).Estado == HttpStatusCode.NotFound, "PUT de un id inexistente responde 404");

        // DELETE con regla de dominio
        var borrado = await Call("DELETE", "clientes/" + id);
        Check(borrado.Estado == HttpStatusCode.NoContent, "DELETE de un cliente sin historial responde 204");
        var tras = await Call("GET", "clientes/" + id);
        Check(tras.Estado == HttpStatusCode.NotFound && Codigo(tras) == "NO_ENCONTRADO", "Después del DELETE, GET clientes/{id} responde 404");
        var auditoria = await Call("GET", "auditoria?tamanoPagina=100");
        Check(auditoria.Json!["elementos"]!.AsArray().Any(x => x!["operacion"]?.ToString() == "ELIMINAR_CLIENTE" && x["entidadId"]?.ToString() == id),
            "El borrado queda en la auditoría como ELIMINAR_CLIENTE");
        var inexistente = Guid.NewGuid();
        Check((await Call("DELETE", "clientes/" + inexistente)).Estado == HttpStatusCode.NotFound, "DELETE de un id inexistente responde 404");
        Check((await Call("GET", "clientes/" + inexistente)).Estado == HttpStatusCode.NotFound, "GET de un id inexistente responde 404");

        var comprador = await Login("comprador.s234", "Comprador-2026!");
        Token(comprador);
        var clienteComprador = (await Call("GET", "auth/yo")).Json!["clienteId"]!.ToString();
        Token(admin);
        var conPedido = await Call("DELETE", "clientes/" + clienteComprador);
        Check(conPedido.Estado == HttpStatusCode.Conflict && Codigo(conPedido) == "CLIENTE_CON_HISTORIAL"
            && conPedido.Json!["mensaje"]!.ToString().Contains("Desactív"), "DELETE de un cliente con pedidos responde 409 CLIENTE_CON_HISTORIAL y sugiere desactivarlo");
        Check((await Call("GET", "clientes/" + clienteComprador)).Estado == HttpStatusCode.OK, "El cliente con historial sigue existiendo");

        // Recurso relacionado: pedidos del cliente
        var pedidos = await Call("GET", "clientes/" + clienteComprador + "/pedidos");
        var elementos = pedidos.Json!["elementos"]!.AsArray();
        Check(pedidos.Estado == HttpStatusCode.OK && elementos.Count >= 2
            && elementos.All(x => x!["codigo"] is not null && x["estadoComercial"] is not null && x["total"] is not null && x["clienteId"] is null && x["detalles"] is null),
            "GET clientes/{id}/pedidos devuelve sus pedidos con código, fecha, estado y total");
        Check((await Call("GET", "clientes/" + Guid.NewGuid() + "/pedidos")).Estado == HttpStatusCode.NotFound, "Pedidos de un cliente inexistente responde 404");
        Check((await Call("GET", "clientes/" + clienteComprador + "/pedidos?tamanoPagina=7")).Estado == HttpStatusCode.BadRequest, "Un tamaño de página inválido responde 400");
        var ajeno = (await Call("GET", "clientes?busqueda=Ajeno%20S234")).Json!["elementos"]![0]!["id"]!.ToString();
        Token(comprador);
        var propios = await Call("GET", "clientes/" + clienteComprador + "/pedidos");
        Check(propios.Estado == HttpStatusCode.OK && propios.Json!["elementos"]!.AsArray().Count == elementos.Count, "El comprador ve los pedidos de su propio cliente");
        Check((await Call("GET", "clientes/" + ajeno + "/pedidos")).Estado == HttpStatusCode.Forbidden, "El comprador recibe 403 con los pedidos de otro cliente");
        Check((await Call("DELETE", "clientes/" + ajeno)).Estado == HttpStatusCode.Forbidden, "El comprador no puede borrar clientes (403)");
        Token(admin);
        Console.WriteLine("RESULTADO Clientes REST: " + total + " comprobaciones correctas.");
    }
}
