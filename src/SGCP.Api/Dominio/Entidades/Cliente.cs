using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string IdentificacionFiscal { get; set; } = string.Empty;
    public FormaPago CondicionHabitual { get; set; } = FormaPago.CONTADO;
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
    public string Observaciones { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Usuario> CuentasPortal { get; set; } = new List<Usuario>();
}
