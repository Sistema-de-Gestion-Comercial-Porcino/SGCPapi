using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.MySqlClient;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;

public static class MigracionHistorica
{
    public static async Task Verificar(string conexion)
    {
        var cadena = new MySqlConnectionStringBuilder(conexion)
        {
            Database = "sgcp_pruebas_upgrade_" + Guid.NewGuid().ToString("N")
        }.ConnectionString;
        if (!System.Text.RegularExpressions.Regex.IsMatch(new MySqlConnectionStringBuilder(cadena).Database, "^sgcp_pruebas_upgrade_[a-f0-9]{32}$"))
            throw new InvalidOperationException("La migración histórica solo puede usar una base temporal de pruebas.");
        await using var db = new SgcpContext(new DbContextOptionsBuilder<SgcpContext>().UseMySQL(cadena).Options);
        try
        {
            await db.GetService<IMigrator>().MigrateAsync("20261106130825_InicialSprint1");
            var clasificacion = new ClasificacionProducto { Codigo = "H-CLS", Nombre = "Corte histórico", Familia = TipoProducto.CARNE_FRESCA };
            var producto = new Producto
            {
                Codigo = "H-PROD",
                Nombre = "Producto histórico",
                UnidadVenta = "kg",
                Presentacion = "Pieza",
                ClasificacionProductoId = clasificacion.Id,
                FechaAlta = DateTime.UtcNow
            };
            var cliente = new Cliente { Nombre = "Cliente histórico", FechaAlta = DateTime.UtcNow };
            var cotizacion = new Cotizacion
            {
                Codigo = "H-COT",
                ClienteId = cliente.Id,
                Estado = EstadoCotizacion.ACEPTADA,
                Fecha = DateTime.UtcNow,
                VigenteHasta = DateTime.UtcNow.AddDays(1)
            };
            var pedido = new Pedido
            {
                Codigo = "H-PED",
                ClienteId = cliente.Id,
                CotizacionId = cotizacion.Id,
                Fecha = DateTime.UtcNow,
                FechaComprometida = DateTime.UtcNow.AddDays(1),
                CostoEstimadoConfirmado = 60,
                MargenEstimadoConfirmado = 20.50m,
                Detalles = [new DetallePedido
                {
                    ProductoId = producto.Id, CantidadSolicitada = 2, CantidadConfirmada = 2,
                    Unidad = "kg", PrecioAcordado = 40.25m, CostoReferenciaAcordado = 30
                }]
            };
            // Se inserta con el contrato del esquema histórico: TipoDocumento todavía no existe.
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Clientes (Id,Nombre,Tipo,Telefono,Direccion,IdentificacionFiscal,CondicionHabitual,Activo,FechaAlta,Observaciones) VALUES ({cliente.Id},{cliente.Nombre},{cliente.Tipo},{cliente.Telefono},{cliente.Direccion},{cliente.IdentificacionFiscal},{cliente.CondicionHabitual.ToString()},{cliente.Activo},{cliente.FechaAlta},{cliente.Observaciones})");
            db.AddRange(clasificacion, producto, cotizacion, pedido);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            await db.Database.MigrateAsync();
            await db.Database.MigrateAsync();
            var cargo = await db.Cargos.SingleAsync(x => x.PedidoId == pedido.Id);
            var conservado = await db.Pedidos.SingleAsync(x => x.Id == pedido.Id);
            if (cargo.Importe != 80.50m || conservado.MargenEstimadoConfirmado != 20.50m)
                throw new Exception("La actualización alteró los importes históricos.");
            Console.WriteLine("OK MIGRACIÓN · Base Sprint 1 con pedido existente actualizada dos veces; cargo 80,50 y margen 20,50 conservados.");
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }
}
