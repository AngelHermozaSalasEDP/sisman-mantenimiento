using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Infrastructure.Persistencia;
using Sisman.Mantenimiento.Infrastructure.Seguridad;

namespace Sisman.Mantenimiento.Infrastructure;

public static class DependencyInjection
{
    public const string NombreConexion = "Sisman";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cadena = config.GetConnectionString(NombreConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{NombreConexion}» (variable de entorno ConnectionStrings__{NombreConexion}).");

        services.AddDbContext<SismanDbContext>(o => o.UseSqlServer(cadena, s => s.EnableRetryOnFailure()));
        services.Configure<JwtOpciones>(config.GetSection(JwtOpciones.Seccion));
        services.Configure<SemillaOpciones>(config.GetSection(SemillaOpciones.Seccion));

        services.AddSingleton<IReloj, RelojSistema>();
        services.AddSingleton<HasheadorClave>();
        services.AddSingleton<IHasheadorClave>(sp => sp.GetRequiredService<HasheadorClave>());
        services.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<IMpRepositorio, MpRepositorio>();
        services.AddScoped<SemillaDatos>();
        return services;
    }
}

/// <summary>Solo para <c>dotnet ef</c>: toma la cadena de la variable SISMAN_CONNECTION o usa LocalDB.</summary>
public class SismanDbContextFactory : IDesignTimeDbContextFactory<SismanDbContext>
{
    public SismanDbContext CreateDbContext(string[] args)
    {
        var cadena = Environment.GetEnvironmentVariable("SISMAN_CONNECTION")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=SismanMantenimiento;Trusted_Connection=True;TrustServerCertificate=True";
        return new SismanDbContext(new DbContextOptionsBuilder<SismanDbContext>().UseSqlServer(cadena).Options);
    }
}
