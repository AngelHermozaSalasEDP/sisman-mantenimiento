using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sisman.Mantenimiento.Infrastructure.Persistencia;
using Xunit.Abstractions;

namespace Sisman.Mantenimiento.IntegrationTests;

/// <summary>
/// RNF-01: con 26 000 OT (el volumen del sistema original), la página de OT responde en menos de 2 s
/// y pesa menos de 200 KB. Las OT se insertan con una sola instrucción SQL para que la preparación sea rápida.
/// </summary>
public class RendimientoTests(ApiFactory fabrica, ITestOutputHelper salida) : IClassFixture<ApiFactory>
{
    private const int Volumen = 26_000;
    private static readonly TimeSpan Limite = TimeSpan.FromSeconds(2);
    private const int LimiteBytes = 200 * 1024;

    private static readonly SemaphoreSlim Carga = new(1, 1);

    private async Task CargarVolumenAsync()
    {
        await Carga.WaitAsync();
        try
        {
            using var scope = fabrica.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SismanDbContext>();
            if (await db.OrdenesTrabajo.CountAsync() >= Volumen) return;
            // La carga masiva en LocalDB puede pasar de los 30 s por defecto; solo afecta a la preparación, no a lo que se mide.
            db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
            await InsertarAsync(db);
        }
        finally
        {
            Carga.Release();
        }
    }

    private static async Task InsertarAsync(SismanDbContext db)
    {

        // Copia los datos del primer MP; números desde 100 001 para no chocar con los de la semilla.
        await db.Database.ExecuteSqlRawAsync($"""
            WITH n AS (
                SELECT TOP ({Volumen}) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS i
                FROM sys.all_objects a CROSS JOIN sys.all_objects b
            )
            INSERT INTO [sisman].[OrdenTrabajo]
                (Numero, Tipo, Prioridad, Estado, Descripcion, MaquinaId, SistemaId, EquipoId, TipoTrabajoId, FuenteId,
                 OportunidadId, EspecialidadId, MantenimientoPreventivoId, FechaObjetivo, CreadoEn, CreadoPor)
            SELECT 100000 + n.i, 1, 1, CASE n.i % 3 WHEN 0 THEN 41 WHEN 1 THEN 40 ELSE 38 END,
                   CONCAT(mp.Descripcion, ' (carga ', n.i, ')'), mp.MaquinaId, mp.SistemaId, mp.EquipoId, mp.TipoTrabajoId, mp.FuenteId,
                   mp.OportunidadId, mp.EspecialidadId, mp.Id, DATEADD(DAY, n.i % 365, '2026-01-01'), SYSUTCDATETIME(), 'carga'
            FROM n CROSS JOIN (SELECT TOP 1 * FROM [sisman].[MantenimientoPreventivo] ORDER BY Id) mp;
            """);
    }

    private async Task<HttpClient> ClienteAsync()
    {
        var http = fabrica.CreateClient();
        var r = await http.PostAsJsonAsync("/api/v1/auth/login", new { usuario = "planificador", clave = ApiFactory.ClavePlanificador });
        var token = JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync()).GetProperty("token").GetString();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    [Theory(DisplayName = "RNF-01: con 26 000 OT la página responde en menos de 2 s y pesa menos de 200 KB")]
    [InlineData("/api/v1/ordenes-trabajo")]
    [InlineData("/api/v1/ordenes-trabajo?estado=41")]
    [InlineData("/api/v1/ordenes-trabajo?pagina=300&tamanoPagina=50")]
    [InlineData("/api/v1/ordenes-trabajo?tamanoPagina=200")]
    [InlineData("/api/v1/ordenes-trabajo?fechaDesde=2026-03-01&fechaHasta=2026-03-31")]
    public async Task Pagina_De_Ot_Rapida_Y_Liviana(string ruta)
    {
        await CargarVolumenAsync();
        var http = await ClienteAsync();
        await http.GetAsync(ruta); // calienta planes de consulta y conexiones

        var reloj = Stopwatch.StartNew();
        var r = await http.GetAsync(ruta);
        var cuerpo = await r.Content.ReadAsByteArrayAsync();
        reloj.Stop();

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var total = JsonSerializer.Deserialize<JsonElement>(cuerpo).GetProperty("total").GetInt32();
        salida.WriteLine($"{ruta}: {reloj.ElapsedMilliseconds} ms, {cuerpo.Length / 1024.0:F1} KB, total {total}");

        Assert.True(reloj.Elapsed < Limite, $"tardó {reloj.ElapsedMilliseconds} ms");
        Assert.True(cuerpo.Length < LimiteBytes, $"pesó {cuerpo.Length / 1024.0:F1} KB");
    }

    [Fact(DisplayName = "RNF-01: el total refleja las 26 000 OT cargadas")]
    public async Task Total_Refleja_El_Volumen()
    {
        await CargarVolumenAsync();
        var r = await (await ClienteAsync()).GetAsync("/api/v1/ordenes-trabajo?tamanoPagina=1");
        var total = JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync()).GetProperty("total").GetInt32();
        Assert.True(total >= Volumen, $"total {total}");
    }
}
