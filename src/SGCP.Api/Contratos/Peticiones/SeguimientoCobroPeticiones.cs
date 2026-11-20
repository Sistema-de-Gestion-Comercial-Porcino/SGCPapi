using System.ComponentModel.DataAnnotations;

namespace SGCP.Api.Contratos.Peticiones;

public record SeguimientoCobroPeticion(DateTime ProximaFecha);
public record GestionCobroPeticion([Required, StringLength(500, MinimumLength = 1)] string Resultado, DateTime? ProximaFecha);
