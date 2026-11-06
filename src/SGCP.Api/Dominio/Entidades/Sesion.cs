using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class Sesion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Inicio { get; set; }
    public DateTime Expira { get; set; }
    public bool Revocada { get; set; }
    public Guid UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
