using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class RegistroAuditoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Operacion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public Guid EntidadId { get; set; }
    public DateTime Fecha { get; set; }
    public Guid UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
