namespace SGCP.Api.Contratos.Respuestas;

public record ErrorRespuesta(string Codigo, string Mensaje, object[] Detalles);
public record ResultadoPaginado<T>(IReadOnlyList<T> Elementos, int Pagina, int TamanoPagina, int TotalRegistros, int TotalPaginas);
public record UsuarioResumen(Guid Id, string Nombre, string Login, string[] Roles, string[] Permisos, Guid? ClienteId);
public record AccesoRespuesta(string Token, DateTime Expira, UsuarioResumen Usuario);
