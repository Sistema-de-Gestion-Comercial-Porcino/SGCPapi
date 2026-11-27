using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
namespace SGCP.Api.Datos;

public static class DatosIniciales
{
    public static async Task Sembrar(SgcpContext db, IConfiguration config)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        foreach (var codigo in Permisos.Todos)
            if (!await db.Permisos.AnyAsync(x => x.Codigo == codigo))
                db.Permisos.Add(new Permiso
                {
                    Codigo = codigo,
                    Descripcion = codigo.Replace('.', ' '),
                    Modulo = codigo.Split('.')[0],
                    Accion = codigo.Split('.')[1]
                });
        await db.SaveChangesAsync();
        var permisos = await db.Permisos.ToListAsync();
        Dictionary<string, string[]> roles = new()
        {
            ["SUPERADMIN"] = Permisos.Todos,
            ["PROPIETARIO"] = Permisos.Todos,
            ["ADMINISTRADOR"] = ["usuarios.consultar", "usuarios.administrar", "clientes.consultar", "clientes.administrar",
                "catalogo.consultar", "catalogo.administrar", "precios.consultar", "pedidos.consultar", "operacion.consultar",
                "produccion.consultar", "produccion.administrar", "inventario.consultar", "inventario.administrar",
                "frio.administrar", "entregas.administrar", "entregas.revisar", "entregas.atender", "reportes.consultar"],
            ["FINANZAS"] = [.. RolesSistema.Financieros, "clientes.consultar", "catalogo.consultar", "pedidos.consultar", "precios.consultar", "reportes.consultar"],
            ["VENTAS"] = ["clientes.consultar", "clientes.administrar", "catalogo.consultar", "costos.consultar",
                "precios.consultar", "cotizaciones.consultar", "cotizaciones.administrar", "cotizaciones.responder",
                "pedidos.consultar", "pedidos.administrar", "cargos.administrar", "pagos.administrar", "saldos.consultar",
                "reportes.consultar", "entregas.revisar", "entregas.atender", "fiscal.administrar"],
            ["PRODUCCION"] = ["operacion.consultar", "catalogo.consultar", "produccion.consultar", "produccion.administrar", "inventario.consultar", "reportes.consultar"],
            ["ALMACEN"] = ["operacion.consultar", "catalogo.consultar", "produccion.consultar", "inventario.consultar", "inventario.administrar", "frio.administrar", "reportes.consultar"],
            ["ENTREGA"] = ["operacion.consultar", "catalogo.consultar", "entregas.administrar", "entregas.atender", "frio.administrar", "reportes.consultar"],
            ["COMPRADOR"] = ["catalogo.consultar", "precios.consultar", "cotizaciones.consultar", "cotizaciones.solicitar", "cotizaciones.responder", "pedidos.consultar", "reportes.consultar"]
        };
        foreach (var (nombre, codigos) in roles)
        {
            var rol = await db.Roles.Include(x => x.Permisos).SingleOrDefaultAsync(x => x.Nombre == nombre);
            if (rol is null)
            {
                rol = new Rol { Nombre = nombre, Descripcion = nombre };
                db.Roles.Add(rol);
            }
            rol.Permisos = permisos.Where(x => codigos.Contains(x.Codigo)).ToList();
        }
        await db.SaveChangesAsync();
        // Migración de datos idempotente: únicamente el admin inicial asciende a raíz.
        var anterior = await db.Roles.Include(r => r.Permisos).SingleOrDefaultAsync(r => r.Nombre == "ADMINISTRACION");
        if (anterior is { Activo: true })
        {
            var cuentas = await db.Usuarios.Include(u => u.Roles).Where(u => u.Roles.Any(r => r.Nombre == "ADMINISTRACION")).ToListAsync();
            var raiz = await db.Roles.SingleAsync(r => r.Nombre == "SUPERADMIN");
            var operador = await db.Roles.SingleAsync(r => r.Nombre == "ADMINISTRADOR");
            var inicial = cuentas.SingleOrDefault(u => u.Login == "admin" && u.Estado == EstadoUsuario.ACTIVO);
            if (inicial is null && !await db.Usuarios.AnyAsync(u => u.Estado == EstadoUsuario.ACTIVO && u.Roles.Any(r => r.Nombre == "SUPERADMIN")))
                throw new InvalidOperationException("No se encontró la cuenta admin activa para migrar. Revisa las cuentas antes de continuar.");
            foreach (var u in cuentas)
            {
                u.Roles.Remove(anterior);
                var destino = u == inicial ? raiz : operador;
                if (!u.Roles.Any(r => r.Id == destino.Id)) u.Roles.Add(destino);
                db.RegistrosAuditoria.Add(new RegistroAuditoria { UsuarioId = inicial?.Id ?? u.Id, Entidad = "Usuario", EntidadId = u.Id,
                    Operacion = "MIGRAR_ROL_" + destino.Nombre, Fecha = DateTime.UtcNow });
            }
            anterior.Activo = false;
            anterior.Permisos.Clear();
            foreach (var s in await db.Sesiones.Where(s => !s.Revocada).ToListAsync()) s.Revocada = true;
            await db.SaveChangesAsync();
        }
        if (!await db.Usuarios.AnyAsync())
        {
            var clave = config["DatosIniciales:ClaveAdministrador"];
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 10)
                throw new InvalidOperationException("Configura DatosIniciales:ClaveAdministrador con al menos 10 caracteres para crear la primera cuenta.");
            var admin = new Usuario
            {
                Nombre = "Administración",
                Login = "admin",
                Correo = config["DatosIniciales:CorreoAdministrador"] ?? "admin@sgcp.local",
                CreadoEn = DateTime.UtcNow,
                Roles = [await db.Roles.SingleAsync(x => x.Nombre == "SUPERADMIN")]
            };
            admin.HashClave = new PasswordHasher<Usuario>().HashPassword(admin, clave);
            db.Usuarios.Add(admin);
        }
        if (config.GetValue<bool>("DatosIniciales:Ejemplos") && !await db.ClasificacionesProducto.AnyAsync())
        {
            var datos = new[] { ("CLS-001", "Costilla", TipoProducto.CARNE_FRESCA),
                    ("CLS-002", "Lomo", TipoProducto.CARNE_FRESCA),
                    ("CLS-003", "Jamón elaborado", TipoProducto.DERIVADO),
                    ("CLS-004", "Sal de cura", TipoProducto.INSUMO) };
            foreach (var (codigo, nombre, familia) in datos)
            {
                var c = new ClasificacionProducto
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    Familia = familia,
                    Descripcion = "Catálogo ilustrativo para la demostración"
                };
                db.ClasificacionesProducto.Add(c);
                if (familia != TipoProducto.INSUMO)
                    db.Productos.Add(new Producto
                    {
                        Codigo = codigo.Replace("CLS", "PRD"),
                        Nombre = nombre == "Jamón elaborado" ? nombre : nombre + " de cerdo",
                        Presentacion = "Pieza",
                        UnidadVenta = "kg",
                        ClasificacionProducto = c,
                        FechaAlta = DateTime.UtcNow
                    });
            }
        }
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
