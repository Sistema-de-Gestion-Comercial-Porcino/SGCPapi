using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class EvidenciaRecepcion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecepcionId
    {
        get; set;
    }
    public string Tipo { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = [];
    public string Sha256 { get; set; } = string.Empty;
}
