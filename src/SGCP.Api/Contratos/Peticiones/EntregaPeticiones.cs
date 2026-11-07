using System.ComponentModel.DataAnnotations;
using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Contratos.Peticiones;

public record CondicionPeticion(FormaPago Modalidad, decimal ImporteAutorizado, DateTime? Vencimiento);
public record AplicacionPeticion(Guid CargoId, decimal Importe);
public record PagoPeticion(Guid ClienteId, [Required, StringLength(80)] string ReferenciaUnica,
    decimal Importe, DateTime Fecha, [Required, StringLength(80)] string Medio,
    [Required, MinLength(1), MaxLength(100)] List<AplicacionPeticion> Aplicaciones);
public record SenaPeticion(Guid PedidoId, [Required, StringLength(80)] string ReferenciaUnica,
    decimal Importe, DateTime Fecha, [Required, StringLength(80)] string Medio);
public record AplicarSenaPeticion(Guid CargoId);
public record TransportePeticion([Required, StringLength(80)] string Codigo, TipoTransporte Modalidad,
    [Required, StringLength(180)] string Transportista, [Required, StringLength(100)] string Contacto,
    [Required, StringLength(40)] string Placa, [Required, StringLength(300)] string DescripcionVehiculo,
    [Required, StringLength(300)] string Conservacion);
public record RutaPeticion(Guid TransporteId, Guid ResponsableId, DateTime Fecha,
    [Required, StringLength(180)] string Zona);
public record EntregaPeticion(Guid PedidoId, Guid? RutaId, Guid ResponsableId, ModalidadEntrega Modalidad,
    DateTime FechaPrevista, int OrdenEnRuta = 1);
public record ReasignarPeticion(Guid ResponsableId, Guid? RutaId, int VersionActual,
    [Required, StringLength(1000)] string Motivo);
public record EvidenciaPeticion([Required, StringLength(40)] string Tipo,
    [Required, MaxLength(700000)] string ContenidoBase64);
public record RecepcionPeticion(Guid ClaveUnica, Guid EntregaId, int VersionBase, Guid AutorId, DateTime FechaReal, decimal PesoRecibido, [Required, StringLength(20)] string Unidad,
    bool Conforme, [Required, StringLength(180)] string Receptor, [StringLength(1000)] string Observacion,
    [Required, MinLength(1), MaxLength(3)] List<EvidenciaPeticion> Evidencias);
public record SincronizarPeticion([Required, MinLength(1), MaxLength(20)] List<RecepcionPeticion> Recepciones);
public record RevisarRecepcionPeticion(bool Aplicar, int VersionActual, [Required, StringLength(1000)] string Motivo);
