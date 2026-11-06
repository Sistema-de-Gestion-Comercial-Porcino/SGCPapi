using SGCP.Api;
var app = Arranque.Crear(args);
await Arranque.Inicializar(app);
await app.RunAsync();
public partial class Program { }
