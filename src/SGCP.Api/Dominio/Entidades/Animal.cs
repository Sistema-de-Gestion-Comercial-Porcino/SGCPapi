using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class Animal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Identificacion { get; set; } = string.Empty;
    public string Procedencia { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public decimal PesoIngreso
    {
        get; set;
    }
    public EstadoAnimal Estado
    {
        get; set;
    }
    public DateTime FechaIngreso
    {
        get; set;
    }
    public bool Activo
    {
        get; set;
    }
}
