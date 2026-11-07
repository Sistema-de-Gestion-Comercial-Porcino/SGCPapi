using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class ControlSanitario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AnimalId
    {
        get; set;
    }
    public ResultadoSanitario Resultado
    {
        get; set;
    }
    public string Observacion { get; set; } = string.Empty;
    public Guid ResponsableId
    {
        get; set;
    }
    public DateTime Fecha
    {
        get; set;
    }
}
