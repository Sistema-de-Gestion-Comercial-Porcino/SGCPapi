using System.Security.Claims;

namespace SGCP.Api.Seguridad;

/// <summary>Jerarquía CU02: la autoridad técnica no permite alterar historia comercial.</summary>
public static class RolesSistema
{
    public static readonly string[] Financieros = ["costos.consultar", "costos.administrar", "cargos.administrar", "pagos.administrar", "saldos.consultar", "fiscal.administrar"];
    public static int Nivel(string rol) => rol switch
    {
        "SUPERADMIN" => 100, "PROPIETARIO" => 80, "ADMINISTRADOR" => 60, _ => 10
    };
    public static int Nivel(ClaimsPrincipal p) => p.FindAll("rol").Select(c => Nivel(c.Value)).DefaultIfEmpty(0).Max();
    public static bool GestionGlobal(ClaimsPrincipal p) => p.IsInRole("SUPERADMIN") || p.IsInRole("PROPIETARIO");
    public static bool Asignable(ClaimsPrincipal p, string rol) => rol != "ADMINISTRACION" && rol != "COMPRADOR" &&
        (p.IsInRole("SUPERADMIN") || (Nivel(p) > Nivel(rol) && (rol != "FINANZAS" || p.IsInRole("PROPIETARIO"))));
}
