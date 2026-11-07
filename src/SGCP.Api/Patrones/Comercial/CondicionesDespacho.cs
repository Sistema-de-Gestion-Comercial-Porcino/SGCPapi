using SGCP.Api.Comun;
using SGCP.Api.Dominio.Entidades;
using SGCP.Api.Dominio.Enumeraciones;

namespace SGCP.Api.Patrones.Comercial;

public interface IEstrategiaCondicion
{
    FormaPago Modalidad
    {
        get;
    }
    void Validar(CondicionPago condicion, decimal saldo);
}

public class CondicionContado : IEstrategiaCondicion
{
    public FormaPago Modalidad => FormaPago.CONTADO;
    public void Validar(CondicionPago condicion, decimal saldo)
    {
        if (saldo > 0)
            throw new ReglaException("PAGO_PENDIENTE", "El despacho al contado requiere el pago completo.");
    }
}

public class CondicionCredito : IEstrategiaCondicion
{
    public FormaPago Modalidad => FormaPago.CREDITO_ACORDADO;
    public void Validar(CondicionPago condicion, decimal saldo)
    {
        if (condicion.Vencimiento is null || condicion.Vencimiento <= DateTime.UtcNow || condicion.ImporteAutorizado < saldo)
            throw new ReglaException("CREDITO_NO_AUTORIZADO", "Se requiere crédito vigente con importe suficiente y vencimiento acordado.");
    }
}

public class SelectorCondicion(IEnumerable<IEstrategiaCondicion> estrategias)
{
    private readonly IReadOnlyDictionary<FormaPago, IEstrategiaCondicion> disponibles = estrategias.ToDictionary(x => x.Modalidad);
    public IEstrategiaCondicion Para(FormaPago forma) => disponibles.TryGetValue(forma, out var estrategia)
        ? estrategia : throw new ReglaException("CONDICION_NO_DISPONIBLE", "La condición de pago no está implementada.");
}
