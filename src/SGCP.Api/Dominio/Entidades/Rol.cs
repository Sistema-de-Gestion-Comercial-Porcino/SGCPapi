using SGCP.Api.Dominio.Enumeraciones;
namespace SGCP.Api.Dominio.Entidades;

public class Rol
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
