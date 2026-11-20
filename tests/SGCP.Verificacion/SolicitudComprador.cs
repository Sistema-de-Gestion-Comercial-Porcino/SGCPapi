using System.Net.Http.Json;
using System.Text.Json.Nodes;

/// <summary>CU05/RF01-RF02: el comprador solicita; Ventas prepara y confirma conforme a CU06.</summary>
public static class SolicitudComprador
{
    public static async Task Ejecutar(HttpClient http)
    {
        var numero = 0;
        void Check(bool valor, string texto)
        {
            if (!valor) throw new Exception("FALLÓ CU05: " + texto);
            Console.WriteLine($"OK CU05 {++numero} · {texto}");
        }
        async Task<(int Estado, JsonNode? Json, Uri? Location)> Call(string metodo, string ruta, object? cuerpo = null)
        {
            using var req = new HttpRequestMessage(new HttpMethod(metodo), ruta);
            if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo);
            using var res = await http.SendAsync(req);
            var contenido = await res.Content.ReadAsStringAsync();
            if ((int)res.StatusCode >= 500) throw new Exception($"{metodo} {ruta}: {contenido}");
            return ((int)res.StatusCode, string.IsNullOrWhiteSpace(contenido) ? null : JsonNode.Parse(contenido), res.Headers.Location);
        }
        async Task<JsonNode> Ok(string metodo, string ruta, object? cuerpo = null)
        {
            var r = await Call(metodo, ruta, cuerpo);
            if (r.Estado >= 300) throw new Exception($"CU05 {metodo} {ruta}: {r.Estado} {r.Json}");
            return r.Json ?? new JsonObject();
        }
        void Token(string token) => http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        string Id(JsonNode j) => j["id"]!.ToString();
        async Task<JsonNode> Login(string login, string clave) => await Ok("POST", "auth/login", new { login, clave });
        object Solicitud(string producto, decimal cantidad = 1) => new { detalles = new[] { new { productoId = producto, cantidad } } };

        var admin = (await Login("admin", "Prueba-local-2026!"))["token"]!.ToString();
        Token(admin);
        var clasificacion = Id(await Ok("POST", "clasificaciones", new { codigo = "CU05CLS", nombre = "Prueba CU05", familia = "CARNE_FRESCA", descripcion = "Datos sintéticos" }));
        async Task<string> Producto(string codigo) => Id(await Ok("POST", "productos", new { codigo, nombre = codigo, presentacion = "Prueba", unidadVenta = "kg", descripcion = "", clasificacionProductoId = clasificacion }));
        var producto = await Producto("CU05PRD");
        var inactivo = await Producto("CU05OFF");
        await Ok("POST", $"productos/{inactivo}/estado", new { activo = false });
        var rolVentas = (await Ok("GET", "roles")).AsArray().Single(x => x!["nombre"]!.ToString() == "VENTAS")!;
        await Ok("POST", "usuarios", new { nombre = "Ventas CU05", login = "ventas.cu05", correo = "ventas.cu05@prueba.local", telefono = "70000001", clave = "Ventas-CU05-2026!", roles = new[] { Id(rolVentas) } });
        async Task<JsonNode> Comprador(string login)
        {
            await Ok("POST", "auth/registro-comprador", new { nombre = login, login, correo = login + "@prueba.local", clave = "Comprador-CU05-2026!", telefono = "70000002" });
            return await Login(login, "Comprador-CU05-2026!");
        }
        var primero = await Comprador("comprador.cu05");
        var segundo = await Comprador("ajeno.cu05");
        var cliente = primero["usuario"]!["clienteId"]!.ToString();
        var ajeno = segundo["usuario"]!["clienteId"]!.ToString();
        var comprador = primero["token"]!.ToString();
        var ventas = (await Login("ventas.cu05", "Ventas-CU05-2026!"))["token"]!.ToString();

        http.DefaultRequestHeaders.Authorization = null;
        Check((await Call("POST", "cotizaciones/solicitudes", Solicitud(producto))).Estado == 401, "Solicitud exige sesión");
        Token(ventas);
        Check((await Call("POST", "cotizaciones/solicitudes", Solicitud(producto))).Estado == 403, "Ventas usa su contrato de preparación, no suplanta al comprador");
        Token(comprador);
        Check((await Call("POST", "cotizaciones/solicitudes", new { detalles = Array.Empty<object>() })).Estado == 400, "Solicitud vacía rechazada");
        Check((await Call("POST", "cotizaciones/solicitudes", new { detalles = new object?[] { null } })).Estado == 400, "Línea nula rechazada sin error de servidor");
        Check((await Call("POST", "cotizaciones/solicitudes", Solicitud(producto, 0))).Estado == 400, "Cantidad cero rechazada");
        Check((await Call("POST", "cotizaciones/solicitudes", Solicitud(producto, 1.0001m))).Estado == 400, "Cantidad con cuatro decimales rechazada");
        Check((await Call("POST", "cotizaciones/solicitudes", Solicitud(inactivo))).Estado == 400, "Producto inactivo rechazado");
        Check((await Call("POST", "cotizaciones/solicitudes", Solicitud(Guid.NewGuid().ToString()))).Estado == 400, "Producto inexistente rechazado");
        Check((await Call("POST", "cotizaciones/solicitudes", new { detalles = Enumerable.Repeat(new { productoId = producto, cantidad = 1 }, 101) })).Estado == 400, "Máximo de 100 líneas aplicado");

        var r = await Call("POST", "cotizaciones/solicitudes", new { clienteId = ajeno, canal = "LOCAL", estado = "ACEPTADA", vigenteHasta = DateTime.UtcNow.AddYears(1),
            detalles = new[] { new { productoId = producto, cantidad = 1.5m, precioUnitario = 0.01m, unidad = "unidad", costoReferencia = 0.01m } } });
        var id = Id(r.Json!);
        Check(r.Estado == 201 && r.Location?.AbsolutePath.EndsWith("/cotizaciones/" + id) == true, "Solicitud crea recurso con 201 y Location");
        var propia = await Ok("GET", r.Location!.ToString());
        Check(propia["clienteId"]!.ToString() == cliente && propia["canal"]!.ToString() == "PORTAL" && propia["estado"]!.ToString() == "BORRADOR", "Cliente, canal y estado no pueden manipularse");
        Check(propia["vigenteHasta"] is null && propia["total"] is null && propia["detalles"]![0]!["precioUnitario"] is null && propia["detalles"]![0]!["subtotal"] is null,
            "Sin oferta: vigencia e importes pendientes, incluso para producto sin precio");
        Check(propia["detalles"]![0]!["unidad"]!.ToString() == "kg" && propia["pedidoId"] is null, "Unidad procede del catálogo y no se crea pedido");
        Check((await Ok("GET", "cotizaciones"))["elementos"]!.AsArray().Any(x => Id(x!) == id), "Comprador encuentra su solicitud en el listado");
        Check((await Call("POST", $"cotizaciones/{id}/aceptar")).Estado == 409, "Solicitud no revisada no puede aceptarse");
        Check((await Call("PUT", $"cotizaciones/{id}/solicitud", Solicitud(producto, 2.25m))).Estado == 200, "Comprador modifica las cantidades propias pendientes");
        Token(segundo["token"]!.ToString());
        Check((await Call("GET", $"cotizaciones/{id}")).Estado == 403, "Otro comprador no consulta la solicitud");
        Check((await Call("PUT", $"cotizaciones/{id}/solicitud", Solicitud(producto))).Estado == 403, "Otro comprador no modifica la solicitud");
        Check(!(await Ok("GET", "cotizaciones"))["elementos"]!.AsArray().Any(x => Id(x!) == id), "Listado no muestra solicitudes ajenas");
        Token(comprador);
        Check((await Call("PUT", $"cotizaciones/{Guid.NewGuid()}/solicitud", Solicitud(producto))).Estado == 404, "Solicitud inexistente responde 404");
        var cuerpoVentas = new { clienteId = cliente, canal = "PORTAL", vigenteHasta = DateTime.UtcNow.AddHours(2), detalles = new[] { new { productoId = producto, cantidad = 2.25m, unidad = "kg", precioUnitario = 5m } } };
        Check((await Call("POST", "cotizaciones", cuerpoVentas)).Estado == 403, "Comprador no recibe administración de cotizaciones");
        Check((await Call("PUT", $"cotizaciones/{id}", cuerpoVentas)).Estado == 403, "Comprador no fija precios ni vigencia");
        Check((await Call("POST", $"cotizaciones/{id}/presentar")).Estado == 403, "Comprador no presenta ofertas en nombre de Ventas");
        var pedido = new { cotizacionId = id, fechaComprometida = DateTime.UtcNow.AddDays(1), condicionPago = "CONTADO" };
        Check((await Call("POST", "pedidos", pedido)).Estado == 403, "Comprador no confirma pedidos internos");

        Token(ventas);
        Check((await Call("POST", $"cotizaciones/{id}/presentar")).Json?["codigo"]?.ToString() == "SOLICITUD_PENDIENTE", "Ventas debe preparar antes de presentar");
        Check((await Call("POST", "pedidos", pedido)).Estado == 409, "Ventas no confirma una solicitud sin aceptación");
        Token(admin);
        await Ok("POST", $"productos/{producto}/precios", new { costoReferencia = 2m, precioVenta = 5m, vigenteDesde = DateTime.UtcNow.AddMinutes(-1) });
        Token(ventas);
        await Ok("PUT", $"cotizaciones/{id}", cuerpoVentas);
        var preparada = await Ok("GET", $"cotizaciones/{id}");
        Check(preparada["total"]!.GetValue<decimal>() == 11.25m && preparada["vigenteHasta"] is not null, "Ventas establece oferta y vigencia sobre la solicitud original");
        Token(comprador);
        Check((await Call("PUT", $"cotizaciones/{id}/solicitud", Solicitud(producto))).Estado == 409, "Comprador no sobrescribe una oferta ya preparada");
        Token(ventas);
        await Ok("POST", $"cotizaciones/{id}/presentar");
        Check((await Call("POST", "pedidos", pedido)).Estado == 409, "Presentada todavía no permite confirmar pedido");
        Token(comprador);
        await Ok("POST", $"cotizaciones/{id}/aceptar");
        Token(ventas);
        var pedidoId = Id(await Ok("POST", "pedidos", pedido));
        Token(comprador);
        var confirmado = await Ok("GET", $"pedidos/{pedidoId}");
        Check(confirmado["estadoComercial"]!.ToString() == "CONFIRMADO" && confirmado["total"]!.GetValue<decimal>() == 11.25m,
            "CU06: comprador consulta su pedido confirmado por Ventas");
        Check(confirmado["detalles"]![0]!["costoReferenciaAcordado"] is null, "Pedido propio no expone costos internos");
        Token(segundo["token"]!.ToString());
        Check((await Call("GET", $"pedidos/{pedidoId}")).Estado == 403, "Pedido confirmado mantiene aislamiento entre compradores");
        Token(admin);
        var auditoria = await Ok("GET", "auditoria?tamanoPagina=100");
        Check(auditoria["elementos"]!.AsArray().Any(x => x!["entidadId"]!.ToString() == id && x["operacion"]!.ToString() == "SOLICITAR"), "Solicitud identificada en auditoría");
        Console.WriteLine($"RESULTADO CU05: {numero} comprobaciones correctas.");
    }
}
