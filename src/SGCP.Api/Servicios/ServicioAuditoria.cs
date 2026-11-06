using SGCP.Api.Datos;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Seguridad;
namespace SGCP.Api.Servicios;

public class ServicioAuditoria(SgcpContext db, ActorActual actor)
{
    public void Registrar(string operacion,
            string entidad,
            Guid id,
            Guid? usuario = null) => db.RegistrosAuditoria.Add(new RegistroAuditoria
            {
                Operacion = operacion,
                Entidad = entidad,
                EntidadId = id,
                Fecha = DateTime.UtcNow,
                UsuarioId = usuario ?? actor.Id
            });
}
