using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Infrastructure.Persistencia;

namespace Sisman.Mantenimiento.IntegrationTests;

/// <summary>
/// RF-07. La semilla deja el correlativo de OT en 3 (OT 1 a 3 en los MP 23 a 25) y los MP 19 a 30 aprobados.
/// Esta clase tiene su propia base, así que los números son predecibles salvo por el orden de las pruebas:
/// por eso cada prueba compara contra el correlativo leído en el momento.
/// </summary>
public class GenerarOtTests(ApiFactory fabrica) : IClassFixture<ApiFactory>
{
    private const string RutaMp = "/api/v1/mantenimientos-preventivos";

    private async Task<HttpClient> ClienteAsync(string usuario = "planificador")
    {
        var clave = usuario == "planificador" ? ApiFactory.ClavePlanificador : ApiFactory.ClaveEspecialista;
        var http = fabrica.CreateClient();
        var r = await http.PostAsJsonAsync("/api/v1/auth/login", new { usuario, clave });
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await Json(r)).GetProperty("token").GetString());
        return http;
    }

    private static async Task<JsonElement> Json(HttpResponseMessage r) =>
        JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync());

    private static async Task<int> IdMpAsync(HttpClient http, int numero) =>
        (await Json(await http.GetAsync($"{RutaMp}?codigo=MP-{numero:D7}"))).GetProperty("items")[0].GetProperty("id").GetInt32();

    // Empieza en 100 días: la semilla ya programa fechas entre hoy + 25 y hoy + 32 (MP 23 a 30), y repetirlas daría 409.
    private static int _desfase = 100;

    /// <summary>Programa una fecha nueva (siempre distinta) en un MP aprobado y devuelve su id.</summary>
    private static async Task<int> NuevaFechaAsync(HttpClient http, int mpId)
    {
        var dias = Interlocked.Increment(ref _desfase);
        var dia = DateOnly.FromDateTime(DateTime.Now).AddDays(dias).ToString("yyyy-MM-dd");
        var r = await http.PostAsJsonAsync($"{RutaMp}/{mpId}/fechas", new { fechaInicio = dia, fechaFin = dia });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await Json(r)).GetProperty("id").GetInt32();
    }

    private async Task<int> CorrelativoAsync()
    {
        using var scope = fabrica.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SismanDbContext>();
        return (await db.Correlativos.AsNoTracking().SingleAsync(c => c.Nombre == "OT")).UltimoValor;
    }

    [Fact(DisplayName = "RF-07/RN-31/RN-35: la OT nace Alta, Pendiente y Preventivo, y la fecha queda enlazada")]
    public async Task Genera_Ot_Con_Valores_Iniciales_Y_Enlaza_La_Fecha()
    {
        var http = await ClienteAsync();
        var mpId = await IdMpAsync(http, 26);
        var fechaId = await NuevaFechaAsync(http, mpId);
        var antes = await CorrelativoAsync();

        var r = await http.PostAsync($"/api/v1/fechas/{fechaId}/ordenes-trabajo", null);

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var ot = await Json(r);
        Assert.Equal(antes + 1, ot.GetProperty("numero").GetInt32());
        Assert.Equal(40, ot.GetProperty("estado").GetInt32());
        Assert.Equal("Alta", ot.GetProperty("prioridad").GetString());
        Assert.Equal("Preventivo", ot.GetProperty("tipo").GetString());
        Assert.Equal(mpId, ot.GetProperty("mantenimientoPreventivoId").GetInt32());
        Assert.Equal("planificador", ot.GetProperty("creadoPor").GetString());

        // Location apunta al detalle de la OT y el detalle responde.
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync(r.Headers.Location)).StatusCode);

        // RN-35: la fecha del MP muestra el número de OT.
        var mp = await Json(await http.GetAsync($"{RutaMp}/{mpId}"));
        var fecha = mp.GetProperty("fechas").EnumerateArray().Single(f => f.GetProperty("id").GetInt32() == fechaId);
        Assert.Equal(antes + 1, fecha.GetProperty("numeroOt").GetInt32());
        Assert.Equal(fecha.GetProperty("fechaInicio").GetString(), ot.GetProperty("fechaObjetivo").GetString());
    }

    [Fact(DisplayName = "RN-30: una fecha que ya tiene OT da 409 y no consume número")]
    public async Task Fecha_Con_Ot_Da_409_Sin_Consumir_Numero()
    {
        var http = await ClienteAsync();
        var fechaId = await NuevaFechaAsync(http, await IdMpAsync(http, 27));
        Assert.Equal(HttpStatusCode.Created, (await http.PostAsync($"/api/v1/fechas/{fechaId}/ordenes-trabajo", null)).StatusCode);
        var antes = await CorrelativoAsync();

        var otra = await http.PostAsync($"/api/v1/fechas/{fechaId}/ordenes-trabajo", null);

        Assert.Equal(HttpStatusCode.Conflict, otra.StatusCode);
        Assert.Equal("fecha.con-ot", (await Json(otra)).GetProperty("codigo").GetString());
        Assert.Equal(antes, await CorrelativoAsync());
    }

    [Fact(DisplayName = "RF-07: una fecha inexistente da 404 fecha.no-encontrada")]
    public async Task Fecha_Inexistente_Da_404()
    {
        var r = await (await ClienteAsync()).PostAsync("/api/v1/fechas/999999/ordenes-trabajo", null);
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("fecha.no-encontrada", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-03: el especialista no puede generar OT (403)")]
    public async Task Especialista_No_Puede_Generar()
    {
        var plan = await ClienteAsync();
        var fechaId = await NuevaFechaAsync(plan, await IdMpAsync(plan, 28));
        var r = await (await ClienteAsync("especialista")).PostAsync($"/api/v1/fechas/{fechaId}/ordenes-trabajo", null);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact(DisplayName = "RF-07: una OT inexistente da 404 ot.no-encontrada")]
    public async Task Ot_Inexistente_Da_404()
    {
        var r = await (await ClienteAsync()).GetAsync("/api/v1/ordenes-trabajo/999999");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("ot.no-encontrada", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-34/RNF-02: 20 solicitudes simultáneas reciben 20 números distintos y consecutivos")]
    public async Task Veinte_Solicitudes_Simultaneas_Sin_Duplicados()
    {
        var http = await ClienteAsync();
        var mps = new[] { 19, 20, 21, 22, 29 };
        var fechas = new List<int>();
        foreach (var numero in mps)
        {
            var id = await IdMpAsync(http, numero);
            for (var i = 0; i < 4; i++) fechas.Add(await NuevaFechaAsync(http, id));
        }
        var antes = await CorrelativoAsync();

        // Un cliente por solicitud para que viajen en paralelo de verdad.
        var clientes = await Task.WhenAll(fechas.Select(_ => ClienteAsync()));
        using var salida = new Barrier(fechas.Count);
        var respuestas = await Task.WhenAll(fechas.Select((f, i) => Task.Run(async () =>
        {
            salida.SignalAndWait(TimeSpan.FromSeconds(30));
            return await clientes[i].PostAsync($"/api/v1/fechas/{f}/ordenes-trabajo", null);
        })));

        Assert.All(respuestas, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        var numeros = new List<int>();
        foreach (var r in respuestas) numeros.Add((await Json(r)).GetProperty("numero").GetInt32());

        Assert.Equal(20, numeros.Distinct().Count());
        Assert.Equal(Enumerable.Range(antes + 1, 20), numeros.Order());
        Assert.Equal(antes + 20, await CorrelativoAsync());
    }

    [Fact(DisplayName = "RN-30/RN-33: dos solicitudes simultáneas sobre la misma fecha generan una sola OT")]
    public async Task Misma_Fecha_En_Paralelo_Genera_Una_Sola_Ot()
    {
        var http = await ClienteAsync();
        var fechaId = await NuevaFechaAsync(http, await IdMpAsync(http, 30));
        var antes = await CorrelativoAsync();
        var clientes = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => ClienteAsync()));

        using var salida = new Barrier(clientes.Length);
        var respuestas = await Task.WhenAll(clientes.Select(c => Task.Run(async () =>
        {
            salida.SignalAndWait(TimeSpan.FromSeconds(30));
            return await c.PostAsync($"/api/v1/fechas/{fechaId}/ordenes-trabajo", null);
        })));

        Assert.Single(respuestas, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Equal(4, respuestas.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(antes + 1, await CorrelativoAsync());
    }

    [Fact(DisplayName = "RN-33: si algo falla después de tomar el número, el correlativo vuelve atrás")]
    public async Task Falla_En_La_Transaccion_Deshace_El_Correlativo()
    {
        var antes = await CorrelativoAsync();
        using (var scope = fabrica.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransacciones>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.EnTransaccionAsync<int>(async ct =>
            {
                var numero = await repo.SiguienteNumeroAsync("OT", ct);
                Assert.Equal(antes + 1, numero);
                throw new InvalidOperationException("falla simulada en el último paso");
            }, CancellationToken.None));
        }
        Assert.Equal(antes, await CorrelativoAsync());
    }
}
