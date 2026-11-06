using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string HashClave { get; set; } = string.Empty;
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.ACTIVO;
    public int IntentosFallidos { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public Guid? ClienteId { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<Rol> Roles { get; set; } = new List<Rol>();
    public ICollection<Sesion> Sesiones { get; set; } = new List<Sesion>();
    public ICollection<RecuperacionAcceso> Recuperaciones { get; set; } = new List<RecuperacionAcceso>();
    public ICollection<RegistroAuditoria> Auditorias { get; set; } = new List<RegistroAuditoria>();
}
