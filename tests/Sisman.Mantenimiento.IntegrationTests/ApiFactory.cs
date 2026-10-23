using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sisman.Mantenimiento.Infrastructure.Persistencia;

namespace Sisman.Mantenimiento.IntegrationTests;

/// <summary>
/// Levanta la API completa contra una base propia de la prueba (nombre único), con migraciones y semilla.
/// La instancia sale de SISMAN_TEST_CONNECTION o, si no existe, de LocalDB. La base se borra al terminar.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ClavePlanificador = "Plan-prueba-1";
    public const string ClaveEspecialista = "Espe-prueba-1";

    private readonly string _cadena;
    private readonly string _baseDatos = $"SismanPruebas_{Guid.NewGuid():N}";

    public ApiFactory()
    {
        var origen = Environment.GetEnvironmentVariable("SISMAN_TEST_CONNECTION")
            ?? @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";
        _cadena = new SqlConnectionStringBuilder(origen) { InitialCatalog = _baseDatos }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Sisman", _cadena);
        builder.UseSetting("Jwt:Clave", "clave-solo-para-pruebas-automaticas-0123456789");
        builder.UseSetting("Migraciones:AplicarAlArrancar", "true");
        builder.UseSetting("Semilla:Habilitada", "true");
        builder.UseSetting("Semilla:ClavePlanificador", ClavePlanificador);
        builder.UseSetting("Semilla:ClaveEspecialista", ClaveEspecialista);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        using (var scope = Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<SismanDbContext>().Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}
