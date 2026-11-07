namespace SGCP.Api.Patrones.Fiscal;

public record RespuestaFiscal(string Estado, string Numero, string Respuesta);
public interface IProveedorFiscal
{
    Task<RespuestaFiscal> Registrar(string referencia, Guid pedidoId);
}

public sealed class ProveedorFiscalNoConfiguradoAdapter : IProveedorFiscal
{
    public Task<RespuestaFiscal> Registrar(string referencia, Guid pedidoId) => Task.FromResult(
        new RespuestaFiscal("SIN_PROVEEDOR", "", "Operación comercial registrada sin comprobante fiscal. Modalidad y proveedor pendientes de confirmar."));
}
