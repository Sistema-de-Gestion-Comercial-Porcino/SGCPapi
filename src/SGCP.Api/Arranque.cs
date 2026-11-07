using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SGCP.Api.Comun;
using SGCP.Api.Contratos.Respuestas;
using SGCP.Api.Datos;
using SGCP.Api.Dominio.Enumeraciones;
using SGCP.Api.Seguridad;
using SGCP.Api.Servicios;
namespace SGCP.Api;

public static class Arranque
{
    public static WebApplication Crear(string[] args, Action<WebApplicationBuilder>? personalizar = null)
    {
        var b = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ApplicationName = typeof(Arranque).Assembly.FullName });
        b.Configuration.AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables();
        b.Services.AddDbContext<SgcpContext>(o => o.UseMySQL(CadenaConexion.Exigir(b.Configuration)));
        b.Services.AddHttpContextAccessor();
        b.Services.AddScoped<ActorActual>();
        b.Services.AddScoped<GeneradorToken>();
        b.Services.AddScoped<UnidadTrabajo>();
        b.Services.AddScoped<ServicioAuditoria>();
        b.Services.AddScoped<ServicioAutenticacion>();
        b.Services.AddScoped<EntregaRecuperacion>();
        b.Services.AddScoped<ServicioUsuarios>();
        b.Services.AddScoped<ServicioClientes>();
        b.Services.AddScoped<ServicioCatalogo>();
        b.Services.AddScoped<ServicioPrecios>();
        b.Services.AddScoped<GeneradorCodigo>();
        b.Services.AddScoped<ServicioCotizaciones>();
        b.Services.AddScoped<ServicioConfirmarPedido>();
        b.Services.AddScoped<ServicioProduccion>();
        b.Services.AddScoped<ServicioFinanzas>();
        b.Services.AddScoped<SGCP.Api.Patrones.Reportes.ReportesFacade>();
        b.Services.AddSingleton<ExportadorReportes>();
        b.Services.AddScoped<ServicioFiscal>();
        b.Services.AddScoped<SGCP.Api.Patrones.Fiscal.IProveedorFiscal, SGCP.Api.Patrones.Fiscal.ProveedorFiscalNoConfiguradoAdapter>();
        b.Services.AddScoped<ServicioEntregas>();
        b.Services.AddScoped<SGCP.Api.Patrones.Comercial.SelectorCondicion>();
        b.Services.AddScoped<SGCP.Api.Patrones.Comercial.IEstrategiaCondicion, SGCP.Api.Patrones.Comercial.CondicionContado>();
        b.Services.AddScoped<SGCP.Api.Patrones.Comercial.IEstrategiaCondicion, SGCP.Api.Patrones.Comercial.CondicionCredito>();
        b.Services.AddScoped<ServicioInventario>();
        b.Services.AddScoped<SGCP.Api.Patrones.Produccion.SelectorFabrica>();
        b.Services.AddScoped<SGCP.Api.Patrones.Produccion.FabricaResultado, SGCP.Api.Patrones.Produccion.FabricaFaeneo>();
        b.Services.AddScoped<SGCP.Api.Patrones.Produccion.FabricaResultado, SGCP.Api.Patrones.Produccion.FabricaDerivado>();
        b.Services.AddControllers(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            .AddApplicationPart(typeof(Arranque).Assembly)
            .AddJsonOptions(o => { o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)); o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; });
        b.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = c => new BadRequestObjectResult(new ErrorRespuesta("DATOS_INVALIDOS",
                "Revisa los campos del formulario.",
                c.ModelState.Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => (object)new { Campo = x.Key, Mensajes = x.Value!.Errors.Select(e => e.ErrorMessage).ToArray() })
                    .ToArray())));
        personalizar?.Invoke(b);
        var clave = b.Configuration["Jwt:Clave"];
        if (string.IsNullOrWhiteSpace(clave) || Encoding.UTF8.GetByteCount(clave) < 32)
            throw new InvalidOperationException("Configura Jwt:Clave mediante user-secrets o variable de entorno (mínimo 32 bytes).");
        b.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = b.Configuration["Jwt:Emisor"] ?? "SGCPapi",
                ValidateAudience = true,
                ValidAudience = b.Configuration["Jwt:Audiencia"] ?? "SGCPclientes",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(10),
                NameClaimType = "name",
                RoleClaimType = "rol"
            };
            o.Events = new JwtBearerEvents
            {
                OnTokenValidated = async c =>
                {
                    if (!Guid.TryParse(c.Principal?.FindFirstValue("sid"),
                            out var sid) || !Guid.TryParse(c.Principal?.FindFirstValue("sub"),
                            out var uid))
                    {
                        c.Fail("Sesión inválida");
                        return;
                    }
                    var db = c.HttpContext.RequestServices.GetRequiredService<SgcpContext>();
                    var sesion = await db.Sesiones.AsNoTracking()
                        .Include(x => x.Usuario)
                        .ThenInclude(x => x.Roles)
                        .ThenInclude(x => x.Permisos)
                        .SingleOrDefaultAsync(x => x.Id == sid && x.UsuarioId == uid);
                    if (sesion is null
                        || sesion.Revocada
                        || sesion.Expira <= DateTime.UtcNow
                        || sesion.Usuario.Estado != EstadoUsuario.ACTIVO)
                    {
                        c.Fail("Sesión vencida o revocada");
                        return;
                    }
                    c.Principal = new ClaimsPrincipal(new ClaimsIdentity(GeneradorToken.Claims(sesion.Usuario, sid),
                            "Bearer",
                            "name",
                            "rol"));
                },
                OnChallenge = async c =>
                {
                    c.HandleResponse();
                    c.Response.StatusCode = 401;
                    await c.Response.WriteAsJsonAsync(new ErrorRespuesta("SESION_REQUERIDA",
                        "Inicia sesión nuevamente.",
                        []));
                },
                OnForbidden = async c =>
                {
                    c.Response.StatusCode = 403;
                    await c.Response.WriteAsJsonAsync(new ErrorRespuesta("SIN_PERMISO",
                        "No tienes permiso para esta operación.",
                        []));
                }
            };
        });
        b.Services.AddAuthorization(o => { foreach (var permiso in Permisos.Todos) o.AddPolicy(permiso, p => p.RequireAuthenticatedUser().RequireClaim("permiso", permiso)); });
        b.Services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = 429;
            o.AddPolicy("acceso", c => RateLimitPartition.GetFixedWindowLimiter(c.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new()
            {
                PermitLimit = 40,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
        });
        b.Services.AddEndpointsApiExplorer();
        b.Services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1",
                new()
                {
                    Title = "SGCP · API comercial",
                    Version = "v1"
                });
            o.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
            o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", doc)] = [] });
        });
        var app = b.Build();
        app.UseMiddleware<ErroresMiddleware>();
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGet("/salud", () => Results.Ok(new { Servicio = "SGCPapi", Estado = "Disponible" })).AllowAnonymous();
        return app;
    }
    public static async Task Inicializar(WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("DatosIniciales:Habilitar"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            await DatosIniciales.Sembrar(scope.ServiceProvider.GetRequiredService<SgcpContext>(), app.Configuration);
        }
    }
}
