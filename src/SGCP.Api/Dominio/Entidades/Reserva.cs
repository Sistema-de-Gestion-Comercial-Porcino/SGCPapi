using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Reserva
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DetallePedidoId
    {
        get; set;
    }
    public Guid ExistenciaId
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
    public decimal Cantidad
    {
        get; set;
    }
    public EstadoReserva Estado
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
    public string MotivoLiberacion { get; set; } = string.Empty;
}
