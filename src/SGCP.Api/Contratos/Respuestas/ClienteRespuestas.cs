using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Contratos.Respuestas;

/// <summary>Ficha del cliente que devuelve la API. Nunca se expone la entidad de EF Core ni sus colecciones.</summary>
public record ClienteRespuesta(
    Guid Id,
    string Nombre,
    string Tipo,
    string IdentificacionFiscal,
    string Telefono,
    string Direccion,
    FormaPago CondicionHabitual,
    bool Activo,
    DateTime FechaAlta,
    string Observaciones)
{
    public static ClienteRespuesta Desde(Cliente c) =>
        new(c.Id, c.Nombre, c.Tipo, c.IdentificacionFiscal, c.Telefono, c.Direccion, c.CondicionHabitual, c.Activo, c.FechaAlta, c.Observaciones);
}

/// <summary>Resumen de un pedido dentro de GET clientes/{id}/pedidos.</summary>
public record PedidoClienteRespuesta(Guid Id, string Codigo, DateTime Fecha, EstadoComercial EstadoComercial, decimal Total);
