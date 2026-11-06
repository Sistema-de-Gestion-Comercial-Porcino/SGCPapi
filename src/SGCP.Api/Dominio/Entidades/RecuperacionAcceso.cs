using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class RecuperacionAcceso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TokenHash { get; set; } = string.Empty;
    public DateTime SolicitadoEn { get; set; }
    public DateTime ExpiraEn { get; set; }
    public DateTime? UtilizadoEn { get; set; }
    public bool Invalidado { get; set; }
    public Guid UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
