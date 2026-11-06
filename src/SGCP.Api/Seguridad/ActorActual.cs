using System.Security.Claims;
using SGCP.Api.Comun;
namespace SGCP.Api.Seguridad;

public class ActorActual(IHttpContextAccessor acceso)
{
    public ClaimsPrincipal Principal => acceso.HttpContext?.User ?? new();
    public Guid Id => Guid.TryParse(Principal.FindFirstValue("sub"),
            out var id) ? id : throw new ReglaException("SESION_REQUERIDA",
            "Inicia sesión.",
            401);
    public Guid SesionId => Guid.TryParse(Principal.FindFirstValue("sid"),
            out var id) ? id : throw new ReglaException("SESION_REQUERIDA",
            "Inicia sesión.",
            401);
    public Guid? ClienteId => Guid.TryParse(Principal.FindFirstValue("clienteId"), out var id) ? id : null;
    public bool Comprador => Principal.IsInRole("COMPRADOR");
    public bool Tiene(string permiso) => Principal.HasClaim("permiso", permiso);
    public void VerCliente(Guid cliente)
    {
        if (Comprador && ClienteId != cliente)
            throw new ReglaException("SIN_PERMISO",
            "No tienes acceso a esta operación.",
            403);
    }
}
