using System.ComponentModel.DataAnnotations;
using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Contratos.Peticiones;

public record AccesoPeticion([Required, MaxLength(80)] string Login, [Required, MaxLength(128)] string Clave);
public record RegistroCompradorPeticion([Required, MaxLength(120)] string Nombre,
        [Required, RegularExpression(@"[a-zA-Z0-9._-]{3,80}")] string Login,
        [Required, EmailAddress, MaxLength(180)] string Correo,
        [Required, MinLength(10), MaxLength(128)] string Clave,
        [MaxLength(30)] string? Telefono);
public record RecuperarPeticion([Required, EmailAddress, MaxLength(180)] string Correo);
public record RestablecerPeticion([Required, MaxLength(200)] string Token, [Required, MinLength(10), MaxLength(128)] string Clave);
public record UsuarioPeticion([Required, MaxLength(120)] string Nombre,
        [Required, RegularExpression(@"[a-zA-Z0-9._-]{3,80}")] string Login,
        [Required, EmailAddress, MaxLength(180)] string Correo,
        [MaxLength(30)] string? Telefono,
        [MaxLength(128)] string? Clave,
        [Required, MinLength(1)] List<Guid> Roles);
public record EstadoUsuarioPeticion(EstadoUsuario Estado);
public record EstadoPeticion(bool Activo);
public record ClientePeticion([Required, MaxLength(120)] string Nombre,
        [Required, MaxLength(60)] string Tipo,
        [Required, MaxLength(30)] string Telefono,
        [MaxLength(250)] string Direccion,
        [MaxLength(40)] string IdentificacionFiscal,
        FormaPago CondicionHabitual,
        [MaxLength(500)] string Observaciones,
        [RegularExpression("CI|NIT|OTRO|SIN_ESPECIFICAR")] string TipoDocumento = "SIN_ESPECIFICAR");
public record ClasificacionPeticion([Required, RegularExpression(@"[a-zA-Z0-9_-]{1,20}")] string Codigo,
        [Required, MaxLength(100)] string Nombre,
        TipoProducto Familia,
        [MaxLength(400)] string Descripcion);
public record ProductoPeticion([Required, RegularExpression(@"[a-zA-Z0-9_-]{1,20}")] string Codigo,
        [Required, MaxLength(120)] string Nombre,
        [Required, MaxLength(60)] string Presentacion,
        [Required, RegularExpression(@"kg|g|unidad")] string UnidadVenta,
        [MaxLength(400)] string Descripcion,
        Guid ClasificacionProductoId);
public record PrecioPeticion([Range(typeof(decimal),
        "0.01",
        "9999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)] decimal CostoReferencia,
        [Range(typeof(decimal),
                "0.01",
                "9999999999.99",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true)] decimal PrecioVenta,
        DateTime VigenteDesde,
        bool ConfirmarMargenNoPositivo = false);
public record FinalizarPrecioPeticion(DateTime VigenteHasta);
public record DetalleCotizacionPeticion(Guid ProductoId,
        [Range(typeof(decimal),
                "0.001",
                "999999.999",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true)] decimal Cantidad,
        [Required, MaxLength(10)] string Unidad,
        [Range(typeof(decimal),
                "0.01",
                "9999999.99",
                ParseLimitsInInvariantCulture = true,
                ConvertValueInInvariantCulture = true)] decimal PrecioUnitario);
public record CotizacionPeticion(Guid ClienteId,
        CanalSolicitud Canal,
        DateTime VigenteHasta,
        [Required, MinLength(1), MaxLength(100)] List<DetalleCotizacionPeticion> Detalles);
public record MotivoPeticion([Required, MaxLength(500)] string Motivo);
public record NuevaVersionPeticion(DateTime VigenteHasta);
public record PedidoPeticion(Guid CotizacionId, DateTime FechaComprometida, FormaPago CondicionPago);
