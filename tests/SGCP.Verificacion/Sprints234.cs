using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using System.IO.Compression;

public static class Sprints234
{
    public static async Task Ejecutar(HttpClient http, WebApplication app)
    {
        int total = 0;
        void Check(bool cumple, string nombre)
        {
            if (!cumple)
                throw new Exception("FALLÓ S2–S4: " + nombre);
            Console.WriteLine("OK P1 " + ++total + " · " + nombre);
        }
        async Task<(int Estado, JsonNode? Json)> Call(string method, string path, object? body = null)
        {
            using var req = new HttpRequestMessage(new HttpMethod(method), path);
            if (body is not null)
                req.Content = JsonContent.Create(body);
            using var res = await http.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();
            if ((int)res.StatusCode >= 500)
                throw new Exception(path + ": " + text);
            return ((int)res.StatusCode, string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text));
        }
        async Task<JsonNode> Ok(string method, string path, object? body = null)
        {
            var r = await Call(method, path, body);
            if (r.Estado >= 400)
                throw new Exception(path + ": " + r.Estado + " " + r.Json);
            return r.Json ?? new JsonObject();
        }
        string Id(JsonNode n) => n["id"]!.ToString();
        async Task<bool> Error(string code, string method, string path, object? body = null)
        {
            var respuesta = await Call(method, path, body);
            var coincide = respuesta.Json?["codigo"]?.ToString() == code;
            if (!coincide)
                Console.WriteLine("Esperado " + code + "; recibido " + respuesta.Estado + " " + respuesta.Json);
            return coincide;
        }
        using (var swagger = await http.GetAsync("/swagger/v1/swagger.json"))
            Check(swagger.IsSuccessStatusCode, "Swagger publica los contratos completos");
        var acceso = await Ok("POST", "auth/login", new
        {
            login = "admin",
            clave = "Prueba-local-2026!"
        });
        var token = acceso["token"]!.ToString();
        var admin = Id(acceso["usuario"]!);
        void Token(string t) => http.DefaultRequestHeaders.Authorization = new("Bearer", t);
        Token(token);
        var roles = (await Ok("GET", "roles")).AsArray();
        async Task<(string Id, string Token)> Cuenta(string perfil, string nombre)
        {
            var rol = Id(roles.Single(x => x!["nombre"]!.ToString() == perfil)!);
            var usuario = await Ok("POST", "usuarios", new
            {
                nombre,
                login = nombre,
                correo = nombre + "@test.local",
                clave = "Clave-pruebas-2026!",
                roles = new[] { rol }
            });
            var sesion = await Ok("POST", "auth/login", new
            {
                login = nombre,
                clave = "Clave-pruebas-2026!"
            });
            return (Id(usuario), sesion["token"]!.ToString());
        }
        var produccion = await Cuenta("PRODUCCION", "produccion.test");
        var almacen = await Cuenta("ALMACEN", "almacen.test");
        var reparto = await Cuenta("ENTREGA", "reparto.test");
        var otroReparto = await Cuenta("ENTREGA", "otro.reparto");
        var ventas = await Cuenta("VENTAS", "ventas.test");
        var productos = (await Ok("GET", "productos"))["elementos"]!.AsArray();
        var costilla = Id(productos.Single(x => x!["codigo"]!.ToString() == "PRD-001")!);
        var lomo = Id(productos.Single(x => x!["codigo"]!.ToString() == "PRD-002")!);
        var jamon = Id(productos.Single(x => x!["codigo"]!.ToString() == "PRD-003")!);
        Token(produccion.Token);
        Check((await Call("GET", "productos/" + costilla + "/precios")).Estado == 403, "Producción no accede a precios");
        Check((await Ok("GET", "productos/" + costilla))["precioVigente"] is null, "Catálogo tampoco filtra precios a producción");
        Check((await Call("GET", "pedidos")).Estado == 403, "Producción no recibe detalle comercial");
        Token(almacen.Token);
        var operativos = await Ok("GET", "pedidos-operativos");
        Check(!operativos.ToJsonString().Contains("precio", StringComparison.OrdinalIgnoreCase), "Almacén consulta pedidos operativos sin importes");
        Check((await Ok("GET", "productos/" + costilla))["precioVigente"] is null, "Almacén no recibe precios");
        Token(token);
        var animal = Id(await Ok("POST", "animales", new
        {
            identificacion = "AN-TEST",
            procedencia = "Granja",
            proveedor = "Proveedor",
            pesoIngreso = 100.125m
        }));
        object Orden(string? a, string tipo = "FAENEO") => new
        {
            tipo,
            animalId = a,
            inicioPrevisto = DateTime.UtcNow,
            finPrevisto = DateTime.UtcNow.AddHours(6),
            observacion = "Tiempo acordado, no estándar",
            previsiones = new[] { new { productoId = tipo == "FAENEO" ? costilla : jamon, cantidadEstimada = 10.125m, unidad = "kg" } }
        };
        Check(await Error("ANIMAL_RECHAZADO", "POST", "ordenes-produccion", Orden(animal)), "Sin aprobación sanitaria no hay faeneo");
        Check((await Call("POST", "animales/" + animal + "/controles-sanitarios", new
        {
            observacion = "Falta resultado",
            fecha = DateTime.UtcNow
        })).Estado == 400, "Omitir resultado sanitario no aprueba al animal por defecto");
        await Ok("POST", "animales/" + animal + "/controles-sanitarios", new
        {
            resultado = "APROBADO",
            observacion = "Inspección conforme",
            fecha = DateTime.UtcNow
        });
        var orden = Id(await Ok("POST", "ordenes-produccion", Orden(animal)));
        Check((await Ok("GET", "ordenes-produccion/" + orden))["previsiones"]!.AsArray().Count == 1 && (await Ok("GET", "existencias"))["elementos"]!.AsArray().Count == 0, "Previsión registra cantidad estimada sin crear disponibilidad");
        await Ok("POST", "ordenes-produccion/" + orden + "/iniciar");
        var resultado = await Ok("POST", "ordenes-produccion/" + orden + "/confirmar", new
        {
            resultados = new[] {
            new { productoId = costilla, codigoLote = "L-COSTILLA", vencimiento = DateTime.UtcNow.AddDays(15) },
            new { productoId = lomo, codigoLote = "L-LOMO", vencimiento = DateTime.UtcNow.AddDays(15) } }
        });
        var lote1 = resultado["lotes"]![0]!.ToString();
        var lote2 = resultado["lotes"]![1]!.ToString();
        var verOrden = await Ok("GET", "ordenes-produccion/" + orden);
        Check(verOrden["resultados"]!.AsArray().Count == 2 && lote1 != lote2, "Faeneo obtiene costilla y lomo en lotes separados");
        Check(verOrden["resultados"]!.AsArray().All(x => x!["tipoCarneODerivado"] is not null && x["cantidadReal"] is null), "Tipo de corte explícito, cantidad pendiente de pesaje");
        Check(await Error("ORDEN_NO_CONFIRMABLE", "POST", "ordenes-produccion/" + orden + "/confirmar", new
        {
            resultados = new[] { new { productoId = costilla, codigoLote = "DUP", vencimiento = DateTime.UtcNow.AddDays(10) } }
        }), "No se confirma dos veces la producción");
        async Task<(string Pesaje, string Entrada, string Existencia)> Ingresar(string lote, decimal bruto, string ubicacion = "Cámara 1")
        {
            var peso = Id(await Ok("POST", "pesajes", new
            {
                loteId = lote,
                pesoBruto = bruto,
                tara = 0.125m,
                unidad = "kg"
            }));
            var antes = (await Ok("GET", "existencias"))["elementos"]!.AsArray();
            Check(!antes.Any(x => x!["loteId"]!.ToString() == lote), "Pesaje confirmado no crea stock");
            var entrada = Id(await Ok("POST", "entradas-inventario", new
            {
                pesajeId = peso,
                ubicacion
            }));
            var e = (await Ok("GET", "existencias"))["elementos"]!.AsArray().Single(x => x!["loteId"]!.ToString() == lote)!;
            Check(e["cantidadFisica"]!.GetValue<decimal>() == bruto - 0.125m, "Entrada física suma neto exacto con tres decimales");
            return (peso, entrada, Id(e));
        }
        var entrada1 = await Ingresar(lote1, 20.125m);
        var entrada2 = await Ingresar(lote2, 15.125m);
        var datosLomo = productos.Single(x => Id(x!) == lomo)!;
        Check(await Error("UNIDAD_INCOMPATIBLE", "PUT", "productos/" + lomo, new
        {
            codigo = "PRD-002",
            nombre = "Lomo",
            presentacion = "Pieza",
            unidadVenta = "unidad",
            descripcion = "",
            clasificacionProductoId = datosLomo["clasificacionProductoId"]!.ToString()
        }), "Unidad de producto con lotes queda protegida aunque no tenga precios");
        Check(await Error("PRODUCTO_CON_EXISTENCIA", "POST", "productos/" + lomo + "/estado", new
        {
            activo = false
        }), "No se desactiva producto con stock físico");
        var externo = Id(await Ok("POST", "lotes/externos", new
        {
            productoId = costilla,
            codigo = "L-EXTERNO",
            referencia = "GUIA-234",
            procedencia = "Proveedor externo",
            fechaProduccion = DateTime.UtcNow.AddDays(-1),
            vencimiento = DateTime.UtcNow.AddDays(5)
        }));
        var entradaExterna = await Ingresar(externo, 3.125m);
        await Ok("POST", "entradas-inventario/" + entradaExterna.Entrada + "/anular", new
        {
            motivo = "Entrada física anotada por error"
        });
        Check((await Ok("GET", "existencias"))["elementos"]!.AsArray().Single(x => Id(x!) == entradaExterna.Existencia)!["cantidadFisica"]!.GetValue<decimal>() == 0, "Anulación válida revierte stock y conserva entrada");
        Check(await Error("ENTRADA_YA_REGISTRADA", "POST", "entradas-inventario", new
        {
            pesajeId = entrada1.Pesaje,
            ubicacion = "Cámara 1"
        }), "Entrada duplicada rechazada");
        var fabricacion = Id(await Ok("POST", "ordenes-produccion", Orden(null, "FABRICACION")));
        await Ok("POST", "ordenes-produccion/" + fabricacion + "/iniciar");
        var consumo = new
        {
            existenciaId = entrada2.Existencia,
            cantidad = 2.125m,
            referenciaUnica = Guid.NewGuid()
        };
        var primerConsumo = Id(await Ok("POST", "ordenes-produccion/" + fabricacion + "/consumos", consumo));
        Check(Id(await Ok("POST", "ordenes-produccion/" + fabricacion + "/consumos", consumo)) == primerConsumo, "Referencia repetida de consumo no descuenta dos veces");
        var derivado = (await Ok("POST", "ordenes-produccion/" + fabricacion + "/confirmar", new
        {
            resultados = new[] {
            new { productoId = jamon, codigoLote = "L-JAMON", vencimiento = DateTime.UtcNow.AddDays(20) } }
        }))["lotes"]![0]!.ToString();
        var entradaDerivado = await Ingresar(derivado, 2.125m);
        await Ok("POST", "lotes/" + lote2 + "/rechazar", new
        {
            motivo = "Rechazo detectado después del consumo"
        });
        var stockRechazo = (await Ok("GET", "existencias"))["elementos"]!.AsArray();
        Check(stockRechazo.Single(x => Id(x!) == entrada2.Existencia)!["cantidadFisica"]!.GetValue<decimal>() == 12.875m, "Rechazo no devuelve ficticiamente insumos consumidos");
        Check(stockRechazo.Single(x => Id(x!) == entradaDerivado.Existencia)!["disponible"]!.GetValue<decimal>() == 0, "Bloqueo se propaga al derivado producido");
        var rechazado = Id(await Ok("POST", "animales", new
        {
            identificacion = "AN-NO",
            procedencia = "Granja",
            proveedor = "Proveedor",
            pesoIngreso = 90
        }));
        await Ok("POST", "animales/" + rechazado + "/controles-sanitarios", new
        {
            resultado = "RECHAZADO",
            observacion = "Rechazo sanitario",
            fecha = DateTime.UtcNow
        });
        Check(await Error("ANIMAL_RECHAZADO", "POST", "ordenes-produccion", Orden(rechazado)), "Animal rechazado bloquea producción");
        var animalIncidencia = Id(await Ok("POST", "animales", new
        {
            identificacion = "AN-INCIDENCIA",
            procedencia = "Granja",
            proveedor = "Proveedor",
            pesoIngreso = 85
        }));
        await Ok("POST", "animales/" + animalIncidencia + "/controles-sanitarios", new
        {
            resultado = "RECHAZADO",
            observacion = "Incidencia para resolver",
            fecha = DateTime.UtcNow
        });
        var incidencia = (await Ok("GET", "incidencias?resuelta=false"))["elementos"]!.AsArray()
            .Single(x => x!["animalId"]?.ToString() == animalIncidencia);
        var incidenciaId = Id(incidencia!);
        Check((await Call("POST", "incidencias/" + incidenciaId + "/resolver", new
        {
            motivo = "Se documenta el cierre sin rehabilitar"
        })).Estado == 200, "Resolver incidencia responde 200");
        Check((await Call("POST", "incidencias/" + incidenciaId + "/resolver", new
        {
            motivo = "Segundo intento"
        })).Estado == 400, "Resolver una incidencia dos veces responde 400");
        Check((await Call("POST", "incidencias/" + Guid.NewGuid() + "/resolver", new
        {
            motivo = "No existe"
        })).Estado == 404, "Resolver una incidencia inexistente responde 404");
        Check((await Ok("GET", "animales?tamanoPagina=100"))["elementos"]!.AsArray()
            .Single(x => Id(x!) == animalIncidencia)!["estado"]!.ToString() == "BLOQUEADO", "Resolver la incidencia no rehabilita al animal");
        var animalAnulada = Id(await Ok("POST", "animales", new
        {
            identificacion = "AN-ANULADA",
            procedencia = "Granja",
            proveedor = "Proveedor",
            pesoIngreso = 80
        }));
        await Ok("POST", "animales/" + animalAnulada + "/controles-sanitarios", new
        {
            resultado = "APROBADO",
            observacion = "Apto para anular la orden",
            fecha = DateTime.UtcNow.AddMinutes(-2)
        });
        var ordenAnulada = Id(await Ok("POST", "ordenes-produccion", Orden(animalAnulada)));
        await Ok("POST", "ordenes-produccion/" + ordenAnulada + "/anular", new
        {
            motivo = "Se anula antes del rechazo sanitario"
        });
        await Ok("POST", "animales/" + animalAnulada + "/controles-sanitarios", new
        {
            resultado = "RECHAZADO",
            observacion = "Rechazo posterior a la anulación",
            fecha = DateTime.UtcNow
        });
        Check((await Ok("GET", "ordenes-produccion/" + ordenAnulada))["orden"]!["estado"]!.ToString() == "ANULADA", "Una orden anulada no pasa a bloqueada al rechazar el animal");
        // Revisión del 26 nov: sanidad, consumo irreversible, falla de cámara con hora de inicio, nombres y responsables.
        Check(await Error("ANIMAL_NO_REHABILITABLE", "POST", "animales/" + animalAnulada + "/controles-sanitarios", new
        {
            resultado = "RECHAZADO",
            observacion = "Segundo rechazo",
            fecha = DateTime.UtcNow
        }), "Un animal ya rechazado no se rechaza otra vez");
        Check((await Ok("GET", "incidencias?tamanoPagina=100"))["elementos"]!.AsArray().Count(x => x!["animalId"]?.ToString() == animalAnulada) == 1,
            "El rechazo repetido no duplica la incidencia");
        var animalApto = Id(await Ok("POST", "animales", new { identificacion = "AN-APTO-RECH", procedencia = "Granja", proveedor = "Proveedor", pesoIngreso = 92 }));
        await Ok("POST", "animales/" + animalApto + "/controles-sanitarios", new { resultado = "APROBADO", observacion = "Sin novedad", fecha = DateTime.UtcNow.AddMinutes(-3) });
        Check(await Error("MOTIVO_REQUERIDO", "POST", "animales/" + animalApto + "/controles-sanitarios", new
        {
            resultado = "RECHAZADO",
            observacion = "x",
            fecha = DateTime.UtcNow.AddMinutes(-2)
        }), "Rechazar un animal apto exige explicar qué le pasó");
        await Ok("POST", "animales/" + animalApto + "/controles-sanitarios", new { resultado = "RECHAZADO", observacion = "Cojera tras el transporte", fecha = DateTime.UtcNow.AddMinutes(-1) });
        Check((await Ok("GET", "incidencias?tamanoPagina=100"))["elementos"]!.AsArray().Single(x => x!["animalId"]?.ToString() == animalApto)!["motivo"]!.ToString()
            == "Pasó de apto a rechazado: Cojera tras el transporte", "La incidencia explica el paso de apto a rechazado");
        var filtrados = (await Ok("GET", "animales?busqueda=AN-APTO"))["elementos"]!.AsArray();
        Check(filtrados.Count == 1 && Id(filtrados[0]!) == animalApto, "Sanidad filtra animales por identificación (arete)");
        var externoFrio = Id(await Ok("POST", "lotes/externos", new
        {
            productoId = costilla,
            codigo = "L-EXT-FRIO",
            referencia = "GUIA-FRIO",
            procedencia = "Proveedor externo",
            fechaProduccion = DateTime.UtcNow.AddDays(-1),
            vencimiento = DateTime.UtcNow.AddDays(5)
        }));
        var stockFrio = await Ingresar(externoFrio, 6.125m, "Cámara 7");
        var fabAntes = Id(await Ok("POST", "ordenes-produccion", Orden(null, "FABRICACION")));
        await Ok("POST", "ordenes-produccion/" + fabAntes + "/iniciar");
        await Ok("POST", "ordenes-produccion/" + fabAntes + "/consumos", new { existenciaId = stockFrio.Existencia, cantidad = 1m, referenciaUnica = Guid.NewGuid() });
        Check(await Error("ORDEN_CON_CONSUMOS", "POST", "ordenes-produccion/" + fabAntes + "/anular", new { motivo = "Se quiere anular después de consumir" }),
            "Una fabricación con consumos no se anula: el consumo es irreversible");
        var derivadoAntes = (await Ok("POST", "ordenes-produccion/" + fabAntes + "/confirmar", new
        {
            resultados = new[] { new { productoId = jamon, codigoLote = "L-JAM-ANTES", vencimiento = DateTime.UtcNow.AddDays(20) } }
        }))["lotes"]![0]!.ToString();
        await Ingresar(derivadoAntes, 1.125m, "Cámara 8");
        await Task.Delay(1200);
        var inicioFalla = DateTime.UtcNow;
        await Task.Delay(1200);
        var fabDespues = Id(await Ok("POST", "ordenes-produccion", Orden(null, "FABRICACION")));
        await Ok("POST", "ordenes-produccion/" + fabDespues + "/iniciar");
        await Ok("POST", "ordenes-produccion/" + fabDespues + "/consumos", new { existenciaId = stockFrio.Existencia, cantidad = 1m, referenciaUnica = Guid.NewGuid() });
        var derivadoDespues = (await Ok("POST", "ordenes-produccion/" + fabDespues + "/confirmar", new
        {
            resultados = new[] { new { productoId = jamon, codigoLote = "L-JAM-DESPUES", vencimiento = DateTime.UtcNow.AddDays(20) } }
        }))["lotes"]![0]!.ToString();
        await Ingresar(derivadoDespues, 1.125m, "Cámara 8");
        Token(almacen.Token);
        Check(await Error("INICIO_FALLA_REQUERIDO", "POST", "controles-frio", new
        {
            temperatura = 9m, evaluacion = "RECHAZADO", ubicacion = "Cámara 7", fecha = DateTime.UtcNow, observacion = "Sin hora de inicio"
        }), "Rechazar una cámara exige la hora de inicio de la falla");
        await Ok("POST", "controles-frio", new
        {
            temperatura = 9m, evaluacion = "RECHAZADO", ubicacion = "Cámara 7", fecha = DateTime.UtcNow, inicioFalla = inicioFalla,
            observacion = "Registro de la cámara revisado: sin frío desde la hora indicada"
        });
        var lotesFrio = (await Ok("GET", "lotes?tamanoPagina=100"))["elementos"]!.AsArray();
        string EstadoLote(string id) => lotesFrio.Single(x => Id(x!) == id)!["estado"]!.ToString();
        Check(EstadoLote(externoFrio) == "BLOQUEADO", "La falla de la cámara bloquea el lote que estaba dentro");
        Check(EstadoLote(derivadoAntes) == "APTO", "El derivado que consumió antes del inicio de la falla sigue apto");
        Check(EstadoLote(derivadoDespues) == "BLOQUEADO", "El derivado que consumió después del inicio de la falla queda bloqueado");
        Check(EstadoLote(lote1) == "APTO", "Los lotes de otra cámara no se tocan");
        Check((await Ok("GET", "incidencias?tamanoPagina=100"))["elementos"]!.AsArray()
            .Any(x => x!["loteId"]?.ToString() == externoFrio && x["motivo"]!.ToString().StartsWith("Falla de frío en Cámara 7 desde")),
            "La incidencia del lote indica la cámara y desde cuándo falló");
        var conIngreso = (await Ok("GET", "existencias?tamanoPagina=100"))["elementos"]!.AsArray().Single(x => Id(x!) == stockFrio.Existencia)!;
        Check(conIngreso["ingreso"] is not null && conIngreso["ingresoResponsableId"]!.ToString() == admin && conIngreso["procedencia"]!.ToString() == "Proveedor externo",
            "Existencias dice quién ingresó el lote a la cámara, cuándo y de dónde viene");
        Token(produccion.Token);
        var nombres = (await Ok("GET", "usuarios/nombres")).AsArray();
        Check(nombres.Any(x => Id(x!) == almacen.Id && x!["nombre"]!.ToString() == "almacen.test") && nombres.All(x => x!.AsObject().Count == 2),
            "Producción ve el nombre de quien registró, sin login ni otros datos");
        Token(token);
        await Ok("POST", "auth/registro-comprador", new
        {
            nombre = "Comprador S234",
            login = "comprador.s234",
            correo = "c234@test.local",
            clave = "Comprador-2026!",
            telefono = "70000000"
        });
        var comprador = await Ok("POST", "auth/login", new
        {
            login = "comprador.s234",
            clave = "Comprador-2026!"
        });
        var compradorToken = comprador["token"]!.ToString();
        var cliente = comprador["usuario"]!["clienteId"]!.ToString();
        var ajeno = Id(await Ok("POST", "clientes", new
        {
            nombre = "Ajeno S234",
            tipo = "Restaurante",
            telefono = "70000001",
            direccion = "Otra",
            identificacionFiscal = "NIT234",
            condicionHabitual = "CONTADO",
            observaciones = ""
        }));
        async Task<string> Pedido(string client, decimal cantidad = 2)
        {
            var cot = Id(await Ok("POST", "cotizaciones", new
            {
                clienteId = client,
                canal = "LOCAL",
                vigenteHasta = DateTime.UtcNow.AddDays(1),
                detalles = new[] { new { productoId = costilla, cantidad, unidad = "kg", precioUnitario = 40.25m } }
            }));
            await Ok("POST", "cotizaciones/" + cot + "/presentar");
            await Ok("POST", "cotizaciones/" + cot + "/aceptar");
            return Id(await Ok("POST", "pedidos", new
            {
                cotizacionId = cot,
                fechaComprometida = DateTime.UtcNow.AddDays(1),
                condicionPago = "CREDITO_ACORDADO"
            }));
        }
        var pedido = await Pedido(cliente);
        var pedido2 = await Pedido(cliente);
        var pedidoAjeno = await Pedido(ajeno);
        async Task Reservar(string ped)
        {
            var detalle = Id((await Ok("GET", "pedidos/" + ped))["detalles"]![0]!);
            await Ok("POST", "reservas", new
            {
                detallePedidoId = detalle,
                existenciaId = entrada1.Existencia,
                cantidad = 2
            });
        }
        var detalle1 = Id((await Ok("GET", "pedidos/" + pedido))["detalles"]![0]!);
        Check(await Error("SIN_EXISTENCIA_DISPONIBLE", "POST", "reservas", new
        {
            detallePedidoId = detalle1,
            existenciaId = Guid.NewGuid(),
            cantidad = 2
        }), "No se reservan previsiones sin existencia");
        await Reservar(pedido);
        await Reservar(pedido2);
        await Reservar(pedidoAjeno);
        Check(await Error("ENTRADA_EN_USO", "POST", "entradas-inventario/" + entrada1.Entrada + "/anular", new
        {
            motivo = "Tiene reservas"
        }), "No se anula entrada reservada");
        var cargo = (await Ok("GET", "cargos?pedidoId=" + pedido))["elementos"]![0]!;
        Check(cargo["importe"]!.GetValue<decimal>() == 80.50m, "Cargo de producto conserva el precio acordado con centavos");
        Check(await Error("FECHA_INVALIDA", "POST", "cargos", new
        {
            pedidoId = pedido,
            concepto = "Servicio futuro",
            importe = 1,
            acuerdoServicio = "Todavía no prestado",
            prestadoEn = DateTime.UtcNow.AddDays(1)
        }), "Servicio todavía no prestado no genera cargo");
        var servicio = Id(await Ok("POST", "cargos", new
        {
            pedidoId = pedido,
            concepto = "Servicio acordado",
            importe = 10.25m,
            acuerdoServicio = "Corte adicional acordado",
            prestadoEn = DateTime.UtcNow
        }));
        var sena = Id(await Ok("POST", "senas", new
        {
            pedidoId = pedido,
            referenciaUnica = "SENA234",
            importe = 20.25m,
            fecha = DateTime.UtcNow,
            medio = "Efectivo"
        }));
        await Ok("POST", "senas/" + sena + "/aplicar", new
        {
            cargoId = Id(cargo)
        });
        Check(await Error("SENA_YA_RESUELTA", "POST", "senas/" + sena + "/aplicar", new
        {
            cargoId = Id(cargo)
        }), "Seña se aplica una sola vez");
        await Ok("POST", "pagos", new
        {
            clienteId = cliente,
            referenciaUnica = "PAGO234",
            importe = 10.25m,
            fecha = DateTime.UtcNow,
            medio = "Transferencia",
            aplicaciones = new[] { new { cargoId = servicio, importe = 10.25m } }
        });
        Check((await Ok("GET", "saldos?pedidoId=" + pedido))["total"]!.GetValue<decimal>() == 60.25m, "Saldo = cargos por concepto menos pagos aplicados");
        var transporteSync = Id(await Ok("POST", "transportes", new
        {
            codigo = "TR-SYNC",
            modalidad = "PROPIO",
            transportista = "Equipo reparto",
            contacto = "70000000",
            placa = "SYNC-234",
            descripcionVehiculo = "Vehículo refrigerado",
            conservacion = "Evaluación manual"
        }));
        var rutaSync = Id(await Ok("POST", "rutas", new
        {
            transporteId = transporteSync,
            responsableId = reparto.Id,
            fecha = DateTime.UtcNow.AddHours(1),
            zona = "Centro"
        }));
        var otraRutaSync = Id(await Ok("POST", "rutas", new
        {
            transporteId = transporteSync,
            responsableId = otroReparto.Id,
            fecha = DateTime.UtcNow.AddDays(1),
            zona = "Norte"
        }));
        async Task<string> Preparar(string ped, string responsable, bool local = false)
            => Id(await Ok("POST", "entregas", new
            {
                pedidoId = ped,
                responsableId = responsable,
                rutaId = local ? null : responsable == reparto.Id ? rutaSync : otraRutaSync,
                modalidad = local ? "RETIRO_LOCAL" : "DOMICILIO",
                fechaPrevista = DateTime.UtcNow.AddDays(1)
            }));
        var entrega = await Preparar(pedido, reparto.Id);
        var entrega2 = await Preparar(pedido2, reparto.Id);
        var entregaAjena = await Preparar(pedidoAjeno, otroReparto.Id);
        Check(await Error("CONDICION_PENDIENTE", "POST", "entregas/" + entrega + "/despachar"), "Despacho requiere condición acordada");
        await Ok("POST", "pedidos/" + pedido + "/condiciones-pago", new
        {
            modalidad = "CONTADO",
            importeAutorizado = 0
        });
        Check(await Error("PAGO_PENDIENTE", "POST", "entregas/" + entrega + "/despachar"), "Contado no despacha con deuda");
        Check(await Error("VENCIMIENTO_REQUERIDO", "POST", "pedidos/" + pedido + "/condiciones-pago", new
        {
            modalidad = "CREDITO_ACORDADO",
            importeAutorizado = 100
        }), "Crédito sin vencimiento se rechaza");
        foreach (var ped in new[] { pedido, pedido2, pedidoAjeno })
            await Ok("POST", "pedidos/" + ped + "/condiciones-pago", new
            {
                modalidad = "CREDITO_ACORDADO",
                importeAutorizado = 100,
                vencimiento = DateTime.UtcNow.AddDays(10)
            });
        foreach (var e in new[] { entrega, entrega2, entregaAjena })
            await Ok("POST", "entregas/" + e + "/despachar");
        Check((await Ok("GET", "saldos?pedidoId=" + pedido))["total"]!.GetValue<decimal>() == 60.25m, "Autorizar crédito no cobra ni liquida deuda");
        Check(await Error("RECEPCION_PENDIENTE", "GET", "entregas/" + entrega + "/constancia"), "Sin recepción sincronizada no hay constancia");
        Token(reparto.Token);
        Check(await Error("SIN_PERMISO", "POST", "controles-frio", new
        {
            loteId = lote1,
            temperatura = 3,
            evaluacion = "APTO",
            ubicacion = "Almacén",
            fecha = DateTime.UtcNow,
            observacion = "Sin asignación"
        }), "Reparto no modifica control de lotes del almacén");
        Check((await Call("GET", "saldos")).Estado == 403, "Reparto no ve saldos");
        var descargadas = await Ok("GET", "mis-entregas");
        Check(descargadas["entregas"]!.AsArray().Count == 2 && !descargadas.ToJsonString().Contains(entregaAjena), "Descarga solo entregas propias autorizadas");
        Check(!descargadas.ToJsonString().Contains("precio", StringComparison.OrdinalIgnoreCase), "Descarga no contiene importes comerciales");
        var png = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAEElEQVR4nGNw6UgDIgYIBQAg/gTJYWpeAAAAAABJRU5ErkJggg==";
        object Captura(string e, int version, bool conforme = true) => new
        {
            claveUnica = Guid.NewGuid(),
            entregaId = e,
            versionBase = version,
            autorId = reparto.Id,
            fechaReal = DateTime.UtcNow,
            pesoRecibido = 1.950m,
            unidad = "kg",
            conforme,
            receptor = "Cliente receptor",
            observacion = conforme ? "" : "Embalaje observado",
            evidencias = new[] { new { tipo = "image/png", contenidoBase64 = png } }
        };
        var captura = Captura(entrega, 2);
        Check(await Error("SIN_PERMISO", "POST", "recepciones/sincronizar", new
        {
            recepciones = new[] { captura, Captura(entregaAjena, 2) }
        }), "Lote inválido rechaza todas sus recepciones");
        await using (var alcance = app.Services.CreateAsyncScope())
            Check(!await alcance.ServiceProvider.GetRequiredService<SgcpContext>().Recepciones.AnyAsync(x => x.EntregaId == Guid.Parse(entrega)), "Rollback conserva atomicidad del lote completo");
        var concurrentes = await Task.WhenAll(
            Call("POST", "recepciones/sincronizar", new
            {
                recepciones = new[] { captura }
            }),
            Call("POST", "recepciones/sincronizar", new
            {
                recepciones = new[] { captura }
            }));
        Check(concurrentes.Any(x => x.Estado == 200) && concurrentes.All(x => x.Estado is 200 or 409), "Reintentos simultáneos conservan idempotencia y conflicto recuperable");
        var sincronizada = await Ok("POST", "recepciones/sincronizar", new
        {
            recepciones = new[] { captura }
        });
        var recepcion = sincronizada["resultados"]![0]!["recepcionId"]!.ToString();
        var recepcionConsultada = await Ok("GET", "recepciones/" + recepcion);
        Check(recepcionConsultada["recepcion"]!["pesoRecibido"]!.GetValue<decimal>() == 1.950m && recepcionConsultada["recepcion"]!["unidad"]!.ToString() == "kg", "Recepción conserva peso real y unidad");
        Check(!recepcionConsultada.ToJsonString().Contains("saldo", StringComparison.OrdinalIgnoreCase), "Repartidor consulta la evidencia sin saldo comercial");
        Check((await http.GetAsync("entregas/" + entrega + "/constancia")).StatusCode == System.Net.HttpStatusCode.Forbidden, "Constancia con saldo se limita a comprador y cobranza");
        var repetida = await Ok("POST", "recepciones/sincronizar", new
        {
            recepciones = new[] { captura }
        });
        Check(repetida["resultados"]![0]!["duplicada"]!.GetValue<bool>() && repetida["resultados"]![0]!["recepcionId"]!.ToString() == recepcion, "Reintento devuelve recepción original sin duplicar");
        var conflicto = await Ok("POST", "recepciones/sincronizar", new
        {
            recepciones = new[] { Captura(entrega2, 1, false) }
        });
        var conflictoId = conflicto["resultados"]![0]!["recepcionId"]!.ToString();
        Check(conflicto["resultados"]![0]!["estado"]!.ToString() == "CONFLICTO", "Versión antigua conserva captura para revisión");
        Check(await Error("SIN_PERMISO", "POST", "recepciones/sincronizar", new
        {
            recepciones = new[] { Captura(entregaAjena, 2) }
        }), "No se sincroniza una entrega ajena");
        Token(token);
        var entregas = (await Ok("GET", "entregas"))["elementos"]!.AsArray();
        Check(entregas.Single(x => Id(x!) == entrega)!["estado"]!.ToString() == "ENTREGADA", "Recepción conforme cambia estado a ENTREGADA");
        Check(entregas.Single(x => Id(x!) == entrega2)!["estado"]!.ToString() == "DESPACHADA", "Conflicto no sobrescribe estado del servidor");
        Token(ventas.Token);
        var localResp = (await Ok("GET", "responsables-entrega?modalidad=RETIRO_LOCAL")).AsArray();
        var domicilioResp = (await Ok("GET", "responsables-entrega?modalidad=DOMICILIO")).AsArray();
        Check(localResp.Any(x => Id(x!) == ventas.Id) && localResp.Any(x => Id(x!) == reparto.Id)
            && domicilioResp.All(x => x!["rol"]!.ToString() == "ENTREGA") && domicilioResp.Any(x => Id(x!) == reparto.Id),
            "Retiro en local lo atiende Ventas o un repartidor; a domicilio solo un repartidor");
        Check((await Ok("GET", "entregas"))["elementos"]!.AsArray().Count == 0 && (await Call("GET", "rutas")).Estado == 403
            && (await Call("GET", "transportes")).Estado == 403, "Ventas solo ve sus entregas de retiro en local; no administra rutas ni transportes");
        var capturaVentas = await Call("POST", "recepciones/sincronizar", new
        {
            recepciones = new[] { new { claveUnica = Guid.NewGuid(), entregaId = entrega2, versionBase = 2, autorId = ventas.Id, fechaReal = DateTime.UtcNow,
                pesoRecibido = 1.950m, unidad = "kg", conforme = true, receptor = "Cliente", observacion = "", evidencias = new[] { new { tipo = "image/png", contenidoBase64 = png } } } }
        });
        Check(capturaVentas.Estado == 403 && capturaVentas.Json?["codigo"]?.ToString() == "SIN_PERMISO" && capturaVentas.Json?["mensaje"]?.ToString()?.Contains("asignada") == true,
            "Ventas registra recepciones de retiro en local, pero solo de entregas asignadas a su cuenta");
        var enConflicto = (await Ok("GET", "recepciones/conflictos"))["elementos"]!.AsArray().Single(x => Id(x!) == conflictoId)!;
        Check(enConflicto["versionEntrega"]!.GetValue<int>() == 2 && enConflicto["estadoEntrega"]!.ToString() == "DESPACHADA" && enConflicto["pedido"] is not null,
            "Ventas lee en la bandeja de conflictos la versión y el estado actuales de la entrega");
        var detalleConflicto = await Ok("GET", "recepciones/" + conflictoId);
        var versionEntrega = detalleConflicto["entrega"]!["version"]!.GetValue<int>();
        Check(versionEntrega == 2 && Id(detalleConflicto["entrega"]!) == entrega2, "Ventas lee la versión de la entrega en el detalle de la recepción");
        await Ok("POST", "recepciones/" + conflictoId + "/revisar", new
        {
            aplicar = true,
            versionActual = versionEntrega,
            motivo = "Revisado con el responsable"
        });
        Check(!(await Ok("GET", "recepciones/conflictos"))["elementos"]!.AsArray().Any(x => Id(x!) == conflictoId), "Ventas resuelve el conflicto con la versión leída de la API");
        Token(token);
        Check(await Error("RECEPCION_OBSERVADA", "GET", "entregas/" + entrega2 + "/constancia"), "Observada no emite constancia definitiva");
        Check((await Ok("GET", "saldos?pedidoId=" + pedido))["total"]!.GetValue<decimal>() == 60.25m, "Entrega confirmada mantiene deuda independiente");
        using (var pdf = await http.GetAsync("entregas/" + entrega + "/constancia"))
        {
            var bytes = await pdf.Content.ReadAsByteArrayAsync();
            Check(pdf.IsSuccessStatusCode && bytes.AsSpan(0, 4).SequenceEqual("%PDF"u8), "Constancia PDF incluye evidencia y recepción conforme");
            var carpeta = Environment.GetEnvironmentVariable("SGCP_TEST_ARTIFACTS");
            if (carpeta is not null)
            {
                Directory.CreateDirectory(carpeta);
                await File.WriteAllBytesAsync(Path.Combine(carpeta, "Constancia.pdf"), bytes);
            }
        }
        Token(ventas.Token);
        var reporteEntregas = await Ok("GET", "reportes/ENTREGAS?versionVista=2");
        var columnasEntregas = reporteEntregas["columnas"]!.AsArray().Select(x => x!.ToString()).ToList();
        var colEntrega = columnasEntregas.IndexOf("Entrega");
        Check(colEntrega < 0 && reporteEntregas["constancias"]!.AsArray().Any(x => x?.ToString() == entrega),
            "Reporte ENTREGAS oculta el GUID y conserva la navegación a constancias");
        var anterior = await Ok("GET", "reportes/ENTREGAS");
        var columnaAnterior = anterior["columnas"]!.AsArray().ToList().FindIndex(c => c?.ToString() == "Entrega");
        Check(columnaAnterior >= 0 && anterior["filas"]!.AsArray().Any(f => f![columnaAnterior]?.ToString() == entrega),
            "API actualizada conserva el enlace de constancia para la web anterior");
        using (var porPedido = await http.GetAsync("pedidos/" + pedido + "/constancia"))
            Check(porPedido.IsSuccessStatusCode && (await porPedido.Content.ReadAsByteArrayAsync()).AsSpan(0, 4).SequenceEqual("%PDF"u8), "Ventas obtiene la constancia por pedido");
        Check(await Error("RECEPCION_PENDIENTE", "GET", "pedidos/" + pedidoAjeno + "/constancia"), "Pedido sin entrega confirmada no emite constancia");
        Check(await Error("NO_ENCONTRADO", "GET", "pedidos/" + Guid.NewGuid() + "/constancia"), "Constancia de pedido inexistente responde 404");
        Token(compradorToken);
        using (var propia = await http.GetAsync("pedidos/" + pedido + "/constancia"))
            Check(propia.IsSuccessStatusCode, "Comprador obtiene la constancia de su pedido sin conocer el id de la entrega");
        Check((await Call("GET", "pedidos/" + pedidoAjeno + "/constancia")).Estado == 403, "Comprador no obtiene constancia de pedido ajeno");
        Token(reparto.Token);
        Check((await Call("GET", "pedidos/" + pedido + "/constancia")).Estado == 403, "Reparto no obtiene la constancia con saldo por pedido");
        Token(token);
        var fiscal = await Ok("POST", "pedidos/" + pedido + "/registro-fiscal");
        var fiscal2 = await Ok("POST", "pedidos/" + pedido + "/registro-fiscal");
        Check(fiscal["estado"]!.ToString() == "SIN_PROVEEDOR" && Id(fiscal) == Id(fiscal2), "Adapter registra sin comprobante y conserva referencia única");
        foreach (var tipo in new[] { "CLIENTES", "COTIZACIONES", "PEDIDOS", "PRODUCCION", "INVENTARIO", "PAGOS", "ENTREGAS" })
            Check((await Ok("GET", "reportes/" + tipo))["columnas"]!.AsArray().Count > 0, "Reporte " + tipo + " ejecuta contra el motor real");
        Check(await Error("FILTRO_INVALIDO", "GET", "reportes/PAGOS?productoId=" + costilla), "Filtros incompatibles se rechazan en vez de ignorarse");
        var reportePedidos = await Ok("GET", "reportes/PEDIDOS?clienteId=" + cliente);
        Check(reportePedidos["columnas"]!.ToJsonString().Contains("Margen"), "Administración consulta margen histórico");
        Token(produccion.Token);
        Check((await Call("GET", "reportes/PAGOS")).Estado == 403, "Reporte de pagos restringido a perfiles autorizados");
        Token(compradorToken);
        Check((await Call("GET", "reportes/PEDIDOS?clienteId=" + ajeno)).Estado == 403, "Comprador no consulta reportes ajenos");
        Check((await http.GetAsync("reportes/PEDIDOS/exportar/xlsx?clienteId=" + ajeno)).StatusCode == System.Net.HttpStatusCode.Forbidden, "Exportación aplica el mismo aislamiento");
        var propios = await Ok("GET", "reportes/PEDIDOS");
        Check(propios["filas"]!.AsArray().Count == 2 && !propios["columnas"]!.ToJsonString().Contains("Margen") && !propios["columnas"]!.ToJsonString().Contains("Costo"), "Comprador solo ve pedidos propios sin columnas sensibles");
        using (var archivo = await http.GetAsync("reportes/PEDIDOS/exportar/xlsx"))
        {
            Check(archivo.IsSuccessStatusCode, "Exportación Excel del comprador disponible");
            using var zip = new ZipArchive(new MemoryStream(await archivo.Content.ReadAsByteArrayAsync()));
            var texto = "";
            foreach (var entry in zip.Entries.Where(x => x.FullName.EndsWith(".xml")))
            {
                using var reader = new StreamReader(entry.Open());
                texto += await reader.ReadToEndAsync();
            }
            Check(!texto.Contains("Margen") && !texto.Contains("Costo"), "Archivo XLSX no esconde costos ni márgenes en ninguna columna");
        }
        using (var archivo = await http.GetAsync("reportes/PEDIDOS/exportar/pdf"))
        {
            var bytes = await archivo.Content.ReadAsByteArrayAsync();
            Check(archivo.IsSuccessStatusCode && bytes.AsSpan(0, 4).SequenceEqual("%PDF"u8), "Exportación PDF funciona con fuente incluida");
            var carpeta = Environment.GetEnvironmentVariable("SGCP_TEST_ARTIFACTS");
            if (carpeta is not null)
            {
                Directory.CreateDirectory(carpeta);
                await File.WriteAllBytesAsync(Path.Combine(carpeta, "Pedidos_comprador.pdf"), bytes);
            }
        }
        Token(token);
        var transporte = Id(await Ok("POST", "transportes", new
        {
            codigo = "TR-234",
            modalidad = "EXTERNO",
            transportista = "Responsable externo",
            contacto = "70000000",
            placa = "234-TEST",
            descripcionVehiculo = "Vehículo refrigerado",
            conservacion = "Frío verificado manualmente"
        }));
        var ruta = Id(await Ok("POST", "rutas", new
        {
            transporteId = transporte,
            responsableId = reparto.Id,
            fecha = DateTime.UtcNow.AddHours(1),
            zona = "Centro"
        }));
        Check(await Error("TRANSPORTE_EN_USO", "POST", "transportes/" + transporte + "/estado", new
        {
            activo = false
        }), "Transporte de ruta pendiente no se desactiva");
        var pedidoRuta = await Pedido(cliente);
        await Reservar(pedidoRuta);
        await Ok("POST", "pedidos/" + pedidoRuta + "/condiciones-pago", new
        {
            modalidad = "CREDITO_ACORDADO",
            importeAutorizado = 100,
            vencimiento = DateTime.UtcNow.AddDays(10)
        });
        var entregaRuta = Id(await Ok("POST", "entregas", new
        {
            pedidoId = pedidoRuta,
            rutaId = ruta,
            responsableId = reparto.Id,
            modalidad = "DOMICILIO",
            fechaPrevista = DateTime.UtcNow.AddHours(2)
        }));
        await Ok("POST", "entregas/" + entregaRuta + "/despachar");
        Check(await Error("RUTA_PENDIENTE", "POST", "rutas/" + ruta + "/finalizar"), "No se cierra ruta con recepciones sin sincronizar");
        Token(reparto.Token);
        await Ok("POST", "controles-frio", new
        {
            entregaId = entregaRuta,
            temperatura = 3.25m,
            evaluacion = "APTO",
            ubicacion = "Vehículo",
            fecha = DateTime.UtcNow,
            observacion = "Revisión de conservación conforme"
        });
        await Ok("POST", "recepciones/sincronizar", new
        {
            recepciones = new[] { Captura(entregaRuta, 2, false) }
        });
        await Ok("POST", "rutas/" + ruta + "/finalizar");
        Check((await Ok("GET", "rutas"))["elementos"]!.AsArray().Single(x => Id(x!) == ruta)!["estado"]!.ToString() == "FINALIZADA", "Ruta termina con observación sin liquidar deuda");
        Token(token);
        await Ok("POST", "transportes/" + transporte + "/estado", new
        {
            activo = false
        });
        Check((await Ok("GET", "transportes"))["elementos"]!.AsArray().Single(x => Id(x!) == transporte)!["activo"]!.GetValue<bool>() == false, "Desactivación conserva transporte e historial de ruta");
        Check((await Ok("GET", "pedidos/" + pedido))["fecha"]!.ToString().EndsWith("Z"), "Fechas leídas de MySQL conservan contrato UTC");
        await Ok("POST", "productos/" + lomo + "/precios", new
        {
            costoReferencia = 20,
            precioVenta = 35,
            vigenteDesde = DateTime.UtcNow.AddMinutes(-1)
        });
        var cotRedondeo = Id(await Ok("POST", "cotizaciones", new
        {
            clienteId = cliente,
            canal = "LOCAL",
            vigenteHasta = DateTime.UtcNow.AddDays(1),
            detalles = new[] { new { productoId = costilla, cantidad = 0.125m, unidad = "kg", precioUnitario = 35 }, new { productoId = lomo, cantidad = 0.125m, unidad = "kg", precioUnitario = 35 } }
        }));
        await Ok("POST", "cotizaciones/" + cotRedondeo + "/presentar");
        await Ok("POST", "cotizaciones/" + cotRedondeo + "/aceptar");
        var pedidoRedondeo = Id(await Ok("POST", "pedidos", new
        {
            cotizacionId = cotRedondeo,
            fechaComprometida = DateTime.UtcNow.AddDays(1),
            condicionPago = "CONTADO"
        }));
        Check((await Ok("GET", "saldos?pedidoId=" + pedidoRedondeo))["total"]!.GetValue<decimal>() == 8.75m, "Cargos con fracciones de kilo suman el total acordado sin centavos extra");
        var pedidoCancelacion = await Pedido(cliente);
        await Reservar(pedidoCancelacion);
        var entregaCancelacion = await Preparar(pedidoCancelacion, reparto.Id, local: true);
        var cargoCancelacion = Id((await Ok("GET", "cargos?pedidoId=" + pedidoCancelacion))["elementos"]![0]!);
        var senaCancelacion = Id(await Ok("POST", "senas", new
        {
            pedidoId = pedidoCancelacion,
            referenciaUnica = "SENA-CANCELAR",
            importe = 5.25m,
            fecha = DateTime.UtcNow,
            medio = "Efectivo"
        }));
        await Ok("POST", "senas/" + senaCancelacion + "/aplicar", new
        {
            cargoId = cargoCancelacion
        });
        Check(await Error("SENA_SIN_ACUERDO", "POST", "pedidos/" + pedidoCancelacion + "/cancelar", new
        {
            motivo = "Cancelar con seña"
        }), "Cancelación con seña exige acuerdo explícito");
        var saldoAntes = (await Ok("GET", "saldos?pedidoId=" + pedidoCancelacion))["total"]!.GetValue<decimal>();
        await Ok("POST", "senas/" + senaCancelacion + "/resolver", new
        {
            motivo = "Se acuerda revisar la devolución presencialmente; aún no ejecutada"
        });
        await Ok("POST", "pedidos/" + pedidoCancelacion + "/cancelar", new
        {
            motivo = "Cancelación acordada con resolución de seña registrada"
        });
        Check((await Ok("GET", "saldos?pedidoId=" + pedidoCancelacion))["total"]!.GetValue<decimal>() == saldoAntes, "Acuerdo y cancelación no inventan devolución ni cobro");
        Check((await Ok("GET", "entregas"))["elementos"]!.AsArray().Single(x => Id(x!) == entregaCancelacion)!["estado"]!.ToString() == "CANCELADA", "Cancelar pedido antes del despacho cancela su entrega preparada");
        Check((await Ok("GET", "reportes/PRODUCCION?clasificacionId=" + datosLomo["clasificacionProductoId"]!.ToString()))["filas"]!.AsArray().Count == 1, "Reporte filtra por tipo de corte o derivado");
        await using (var scope = app.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SgcpContext>();
            Check(await db.Recepciones.CountAsync(x => x.EntregaId == Guid.Parse(entrega)) == 1, "MySQL contiene una sola recepción tras reintentar");
            Check(await db.EntradasInventario.CountAsync(x => x.LoteId == Guid.Parse(lote1)) == 1, "MySQL contiene una sola entrada física");
            var recepcionGuardada = await db.Recepciones.SingleAsync(x => x.Id == Guid.Parse(recepcion));
            Check(recepcionGuardada.SaldoPendienteAlConfirmar == 60.25m, "Constancia conserva saldo histórico de la confirmación");
            var auditoriaAntes = await db.RegistrosAuditoria.CountAsync();
            var pedidosAntes = await db.Pedidos.CountAsync();
            await Ok("GET", "reportes/PEDIDOS");
            Check(await db.RegistrosAuditoria.CountAsync() == auditoriaAntes + 1 && await db.Pedidos.CountAsync() == pedidosAntes,
                "Consultar reportes registra acceso auditado sin alterar pedidos");
            var fuente = await db.Pedidos.Include(x => x.Detalles).FirstAsync(x => x.Id == Guid.Parse(pedido));
            for (var i = 0; i < 2000; i++)
            {
                var c = new Cotizacion { Codigo = "VOL-C-" + i, Fecha = DateTime.UtcNow, VigenteHasta = DateTime.UtcNow.AddDays(1), ClienteId = fuente.ClienteId, Estado = EstadoCotizacion.ACEPTADA };
                var p = new Pedido
                {
                    Codigo = "VOL-P-" + i,
                    Fecha = DateTime.UtcNow,
                    FechaComprometida = DateTime.UtcNow.AddDays(1),
                    ClienteId = fuente.ClienteId,
                    CotizacionId = c.Id,
                    CostoEstimadoConfirmado = 60,
                    MargenEstimadoConfirmado = 20.50m,
                    Detalles = [new DetallePedido { ProductoId = Guid.Parse(costilla), CantidadConfirmada = 2, CantidadSolicitada = 2, Unidad = "kg", PrecioAcordado = 40.25m, CostoReferenciaAcordado = 30 }]
                };
                db.Cotizaciones.Add(c);
                db.Pedidos.Add(p);
            }
            await db.SaveChangesAsync();
            var reloj = System.Diagnostics.Stopwatch.StartNew();
            var volumen = await Ok("GET", "reportes/PEDIDOS");
            reloj.Stop();
            Check(volumen["filas"]!.AsArray().Count >= 2000, "Reporte con 2000 pedidos adicionales sin consulta por fila");
            Console.WriteLine("VOLUMEN: " + volumen["filas"]!.AsArray().Count + " pedidos; " + reloj.ElapsedMilliseconds + " ms (API HTTP + JSON).");
        }
        var pedidoCobro = await Pedido(cliente);
        await Ok("POST", "pedidos/" + pedidoCobro + "/condiciones-pago", new
        {
            modalidad = "CREDITO_ACORDADO",
            importeAutorizado = 100,
            vencimiento = DateTime.UtcNow.AddDays(15)
        });
        var condicionCredito = Id((await Ok("GET", "pedidos/" + pedidoCobro + "/condiciones-pago")).AsArray()[0]!);
        var creado = await Call("POST", "condiciones-pago/" + condicionCredito + "/seguimientos", new
        {
            proximaFecha = DateTime.UtcNow.AddDays(5)
        });
        Check(creado.Estado == 201, "Crear seguimiento de cobro responde 201");
        var seguimiento = Id(creado.Json!);
        var filasSeguimiento = (await Ok("GET", "seguimientos-cobro?clienteId=" + cliente + "&estado=PENDIENTE"))["elementos"]!.AsArray();
        var filaSeguimiento = filasSeguimiento.Single(x => Id(x!) == seguimiento)!;
        Check(filaSeguimiento["pedidoId"]!.ToString() == pedidoCobro
            && filaSeguimiento["clienteId"]!.ToString() == cliente
            && !string.IsNullOrWhiteSpace(filaSeguimiento["pedido"]!.ToString())
            && !string.IsNullOrWhiteSpace(filaSeguimiento["cliente"]!.ToString())
            && filaSeguimiento["vencimiento"] is not null,
            "Seguimientos paginados incluyen cliente, pedido y vencimiento para la web");
        Check(!(await Ok("GET", "seguimientos-cobro?estado=CERRADO"))["elementos"]!.AsArray().Any(x => Id(x!) == seguimiento),
            "Filtro de seguimientos excluye los estados distintos");
        var gestionPendiente = await Ok("POST", "seguimientos-cobro/" + seguimiento + "/gestion", new
        {
            resultado = "El cliente pidió una nueva fecha",
            proximaFecha = DateTime.UtcNow.AddDays(8)
        });
        Check(gestionPendiente["estado"]!.ToString() == "PENDIENTE", "Con saldo, la gestión vuelve a PENDIENTE");
        var cargoCobro = Id((await Ok("GET", "cargos?pedidoId=" + pedidoCobro))["elementos"]![0]!);
        var saldoCobro = (await Ok("GET", "saldos?pedidoId=" + pedidoCobro))["total"]!.GetValue<decimal>();
        await Ok("POST", "pagos", new
        {
            clienteId = cliente,
            referenciaUnica = "PAGO-COBRO-" + Guid.NewGuid().ToString("N")[..12],
            importe = saldoCobro,
            fecha = DateTime.UtcNow,
            medio = "Transferencia",
            aplicaciones = new[] { new { cargoId = cargoCobro, importe = saldoCobro } }
        });
        var gestionCerrada = await Ok("POST", "seguimientos-cobro/" + seguimiento + "/gestion", new
        {
            resultado = "Saldo cubierto"
        });
        Check(gestionCerrada["estado"]!.ToString() == "CERRADO", "Al pagar el total, la gestión cierra el seguimiento");
        Check((await Call("POST", "seguimientos-cobro/" + seguimiento + "/gestion", new
        {
            resultado = "Intento posterior"
        })).Estado == 400, "Un seguimiento cerrado no admite otra gestión");
        var pedidoContado = await Pedido(cliente);
        await Ok("POST", "pedidos/" + pedidoContado + "/condiciones-pago", new
        {
            modalidad = "CONTADO",
            importeAutorizado = 0
        });
        var condicionContado = Id((await Ok("GET", "pedidos/" + pedidoContado + "/condiciones-pago")).AsArray()[0]!);
        Check((await Call("POST", "condiciones-pago/" + condicionContado + "/seguimientos", new
        {
            proximaFecha = DateTime.UtcNow.AddDays(2)
        })).Estado == 400, "No se abre seguimiento sobre una condición de contado");
        Console.WriteLine("RESULTADO P1: " + total + " comprobaciones S2–S4 correctas.");
    }
}
