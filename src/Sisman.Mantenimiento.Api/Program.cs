using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Sisman.Mantenimiento.Api.Errores;
using Sisman.Mantenimiento.Application;
using Sisman.Mantenimiento.Infrastructure;
using Sisman.Mantenimiento.Infrastructure.Persistencia;
using Sisman.Mantenimiento.Infrastructure.Seguridad;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Sisman.Mantenimiento.Application.Abstracciones.IUsuarioActual, UsuarioActual>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection(JwtOpciones.Seccion).Get<JwtOpciones>() ?? new JwtOpciones();
if (Encoding.UTF8.GetByteCount(jwt.Clave) < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres (256 bits). Defínala con user-secrets o la variable Jwt__Clave.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt.Emisor,
        ValidAudience = jwt.Audiencia,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
        ClockSkew = TimeSpan.FromSeconds(30)
    });

// RN-01: todo exige token salvo lo marcado [AllowAnonymous]; un endpoint nuevo no queda abierto por olvido.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
    .AddPolicy(Politicas.Planificador, p => p.RequireRole("Planificador"))
    .AddPolicy(Politicas.Consulta, p => p.RequireRole("Planificador", "Especialista"));

builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "SISMAN Mantenimiento API", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
        Description = "Token obtenido en POST /api/v1/auth/login"
    });
    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
    });
});

var app = builder.Build();

app.UseMiddleware<CorrelacionMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Migraciones y datos de ejemplo al arrancar (solo si se habilita; ver README).
if (app.Configuration.GetValue<bool>("Migraciones:AplicarAlArrancar"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<SismanDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<SemillaDatos>().EjecutarAsync();
}

app.Run();

public partial class Program;
