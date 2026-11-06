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
            ["ADMINISTRACION"] = Permisos.Todos,
            ["VENTAS"] = Permisos.Todos.Where(x => x.StartsWith("clientes.")
                || x.StartsWith("cotizaciones.")
                || x.StartsWith("pedidos.")
                || x == "catalogo.consultar"
                || x == "costos.consultar")
                    .ToArray(),
            ["PRODUCCION"] = ["catalogo.consultar", "pedidos.consultar"],
            ["ALMACEN"] = ["catalogo.consultar", "pedidos.consultar"],
            ["ENTREGA"] = ["catalogo.consultar"],
            ["COMPRADOR"] = ["catalogo.consultar", "cotizaciones.consultar", "cotizaciones.responder", "pedidos.consultar"]
        };
        foreach (var (nombre, codigos) in roles)
            if (!await db.Roles.AnyAsync(x => x.Nombre == nombre))
                db.Roles.Add(new Rol
                {
                    Nombre = nombre,
                    Descripcion = nombre,
                    Permisos = permisos.Where(x => codigos.Contains(x.Codigo)).ToList()
                });
        await db.SaveChangesAsync();
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
                Roles = [await db.Roles.SingleAsync(x => x.Nombre == "ADMINISTRACION")]
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
