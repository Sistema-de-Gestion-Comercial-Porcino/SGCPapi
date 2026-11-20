using System.ComponentModel.DataAnnotations;

namespace SGCP.Api.Contratos.Peticiones;

public record DetalleSolicitudCotizacionPeticion(Guid ProductoId,
    [Range(typeof(decimal), "0.001", "999999.999", ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)] decimal Cantidad);

public record SolicitudCotizacionPeticion(
    [Required, MinLength(1), MaxLength(100)] List<DetalleSolicitudCotizacionPeticion> Detalles);
