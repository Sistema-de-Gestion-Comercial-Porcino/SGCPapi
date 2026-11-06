using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Peticiones;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api.Controllers;

[ApiController, Route("api/v1/auth")]
public class AutenticacionController(ServicioAutenticacion servicio,
        SgcpContext db,
        ActorActual actor,
        ServicioAuditoria auditoria) : ControllerBase
{
    [HttpPost("login"),
            AllowAnonymous,
            Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("acceso")]
    public async Task<IActionResult> Login(AccesoPeticion p) => Ok(await servicio.Acceder(p));
    [HttpPost("registro-comprador"),
            AllowAnonymous]
    public async Task<IActionResult> Registro(RegistroCompradorPeticion p) => StatusCode(201,
            await servicio.Registrar(p));
    [HttpPost("recuperar"),
            AllowAnonymous,
            Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("acceso")]
    public async Task<IActionResult> Recuperar(RecuperarPeticion p) { await servicio.Recuperar(p.Correo); return Accepted(new { Mensaje = "Si la cuenta está habilitada, recibirás instrucciones para recuperar el acceso." }); }
    [HttpPost("restablecer"), AllowAnonymous] public async Task<IActionResult> Restablecer(RestablecerPeticion p) { await servicio.Restablecer(p); return Ok(new { Mensaje = "Contraseña actualizada. Inicia sesión nuevamente." }); }
    [HttpPost("cerrar-sesion"), Authorize]
    public async Task<IActionResult> Cerrar()
    {
        var s = await db.Sesiones.FindAsync(actor.SesionId);
        if (s is not null)
            s.Revocada = true;
        auditoria.Registrar("CERRAR_SESION",
            "Sesion",
            actor.SesionId);
        await db.SaveChangesAsync();
        return NoContent();
    }
    [HttpGet("yo"), Authorize] public async Task<IActionResult> Yo() => Ok(GeneradorToken.Resumen(await db.Usuarios.Include(x => x.Roles).ThenInclude(x => x.Permisos).SingleAsync(x => x.Id == actor.Id)));
}
