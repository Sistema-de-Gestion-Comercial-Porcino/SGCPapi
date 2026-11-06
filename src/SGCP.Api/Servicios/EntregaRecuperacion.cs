using System.Net;
using System.Net.Mail;
namespace SGCP.Api.Servicios;

public class EntregaRecuperacion(IConfiguration config, IWebHostEnvironment entorno, ILogger<EntregaRecuperacion> log)
{
    public async Task Enviar(string correo, string token)
    {
        var url = (config["Recuperacion:UrlWeb"] ?? "http://localhost:5080") + "/Cuenta/Restablecer?token=" + Uri.EscapeDataString(token);
        try
        {
            if (entorno.IsDevelopment() || entorno.IsEnvironment("Testing"))
            {
                var carpeta = config["Recuperacion:CarpetaLocal"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "SGCP",
                        "recuperaciones");
                Directory.CreateDirectory(carpeta);
                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode(carpeta, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                var archivo = Path.Combine(carpeta, Guid.NewGuid() + ".txt");
                await File.WriteAllTextAsync(archivo, $"Para: {correo}\nVigencia: 30 minutos\n{url}\n");
                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode(archivo, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                return;
            }
            using var smtp = new SmtpClient(config["Correo:Servidor"],
                    config.GetValue<int?>("Correo:Puerto") ?? 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(config["Correo:Usuario"], config["Correo:Clave"])
            };
            using var mensaje = new MailMessage(config["Correo:Remitente"]!,
                    correo,
                    "Recupera tu acceso a SGCP",
                    $"Abre este enlace durante los próximos 30 minutos: {url}");
            await smtp.SendMailAsync(mensaje);
        }
        catch (Exception e) { log.LogError(e, "No se pudo entregar el mensaje de recuperación. Revisar configuración del canal."); }
    }
}
