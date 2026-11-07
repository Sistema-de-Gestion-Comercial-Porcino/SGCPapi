using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Dominio.Entidades;

public class ComprobanteFiscal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId
    {
        get; set;
    }
    public string ReferenciaUnica { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string NumeroFiscal { get; set; } = string.Empty;
    public string RespuestaProveedor { get; set; } = string.Empty;
    public DateTime Fecha
    {
        get; set;
    }
    public Guid ResponsableId
    {
        get; set;
    }
}
