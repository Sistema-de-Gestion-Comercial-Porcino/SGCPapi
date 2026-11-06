namespace SGCP.Api.Comun;

public class ReglaException(string codigo, string mensaje, int estado = 409, params object[] detalles) : Exception(mensaje)
{
    public string Codigo { get; } = codigo;
    public int Estado { get; } = estado;
    public object[] Detalles { get; } = detalles;
}
