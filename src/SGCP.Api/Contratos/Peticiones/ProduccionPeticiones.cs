using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Contratos.Peticiones;

public record AnimalPeticion(
    [Required, StringLength(80)] string Identificacion,
    [Required, StringLength(300)] string Procedencia,
    [Required, StringLength(180)] string Proveedor,
    decimal PesoIngreso);
public record SanidadPeticion([property: JsonRequired] ResultadoSanitario Resultado,
    [Required, StringLength(1000)] string Observacion, DateTime Fecha);
public record OrdenPeticion(TipoOrdenProduccion Tipo, Guid? AnimalId,
    DateTime InicioPrevisto, DateTime FinPrevisto, [StringLength(1000)] string Observacion = "",
    [MaxLength(100)] List<PrevisionPeticion>? Previsiones = null);
public record PrevisionPeticion(Guid ProductoId, decimal CantidadEstimada, [Required, StringLength(20)] string Unidad);
public record ResultadoPeticion(Guid ProductoId, [Required, StringLength(80)] string CodigoLote,
    DateTime Vencimiento);
public record ConfirmarProduccionPeticion([Required, MinLength(1), MaxLength(100)] List<ResultadoPeticion> Resultados);
public record InsumoPeticion(Guid ExistenciaId, decimal Cantidad, Guid ReferenciaUnica);
public record LoteExternoPeticion(Guid ProductoId, [Required, StringLength(80)] string Codigo,
    [Required, StringLength(180)] string Referencia, [Required, StringLength(500)] string Procedencia,
    DateTime FechaProduccion, DateTime Vencimiento);
public record PesajePeticion(Guid LoteId, decimal PesoBruto, decimal Tara,
    [Required, StringLength(20)] string Unidad, [StringLength(1000)] string Observacion = "");
public record EntradaPeticion(Guid PesajeId, [Required, StringLength(180)] string Ubicacion);
public record ReservaPeticion(Guid DetallePedidoId, Guid ExistenciaId, decimal Cantidad);
public record FrioPeticion(Guid? LoteId, decimal Temperatura, [property: JsonRequired] EvaluacionFrio Evaluacion,
    [Required, StringLength(180)] string Ubicacion, DateTime Fecha,
    [Required, StringLength(1000)] string Observacion, Guid? EntregaId = null, DateTime? InicioFalla = null);
public record CargoPeticion(Guid PedidoId, [Required, StringLength(300)] string Concepto, decimal Importe,
    [Required, StringLength(300)] string AcuerdoServicio, DateTime PrestadoEn);
