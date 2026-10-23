using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sisman.Mantenimiento.IntegrationTests;

/// <summary>
/// RF-08 y RF-09. La semilla deja 3 OT (números 1 a 3, estado 40 Pendiente). Cada prueba que necesita
/// una OT propia la genera desde una fecha nueva de un MP aprobado (MP 19 a 30).
/// </summary>
public class OrdenesTrabajoTests(ApiFactory fabrica) : IClassFixture<ApiFactory>
{
    private const string Ruta = "/api/v1/ordenes-trabajo";

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

    // Fuera del rango de fechas de la semilla (hoy + 25 a hoy + 32).
    private static int _desfase = 200;

    /// <summary>Genera una OT nueva y devuelve su JSON.</summary>
    private static async Task<JsonElement> NuevaOtAsync(HttpClient http, int mp = 19)
    {
        var mpId = (await Json(await http.GetAsync($"/api/v1/mantenimientos-preventivos?codigo=MP-{mp:D7}")))
            .GetProperty("items")[0].GetProperty("id").GetInt32();
        var dia = DateOnly.FromDateTime(DateTime.Now).AddDays(Interlocked.Increment(ref _desfase)).ToString("yyyy-MM-dd");
        var f = await http.PostAsJsonAsync($"/api/v1/mantenimientos-preventivos/{mpId}/fechas", new { fechaInicio = dia, fechaFin = dia });
        Assert.Equal(HttpStatusCode.Created, f.StatusCode);
        var r = await http.PostAsync($"/api/v1/fechas/{(await Json(f)).GetProperty("id").GetInt32()}/ordenes-trabajo", null);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return await Json(r);
    }

    private static Task<HttpResponseMessage> CambiarAsync(HttpClient http, int id, int estado) =>
        http.PutAsJsonAsync($"{Ruta}/{id}/estado", new { estado });

    // ---------- RF-08 ----------

    [Fact(DisplayName = "RF-08/RN-41: la consulta ordena por número descendente y pagina en el servidor")]
    public async Task Consulta_Ordena_Por_Numero_Descendente_Y_Pagina()
    {
        var http = await ClienteAsync();
        for (var i = 0; i < 4; i++) await NuevaOtAsync(http, 20);

        var todo = await Json(await http.GetAsync(Ruta));
        var total = todo.GetProperty("total").GetInt32();
        Assert.True(total >= 7);
        var numeros = todo.GetProperty("items").EnumerateArray().Select(o => o.GetProperty("numero").GetInt32()).ToList();
        Assert.Equal(numeros.OrderDescending(), numeros);
        Assert.Equal(0, todo.GetProperty("items")[0].GetProperty("historial").GetArrayLength());

        var p2 = await Json(await http.GetAsync($"{Ruta}?pagina=2&tamanoPagina=3"));
        Assert.Equal(3, p2.GetProperty("items").GetArrayLength());
        Assert.Equal(numeros[3], p2.GetProperty("items")[0].GetProperty("numero").GetInt32());
    }

    [Fact(DisplayName = "RF-08: filtra por número")]
    public async Task Filtra_Por_Numero()
    {
        var json = await Json(await (await ClienteAsync()).GetAsync($"{Ruta}?numero=2"));
        Assert.Equal(1, json.GetProperty("total").GetInt32());
        Assert.Equal(2, json.GetProperty("items")[0].GetProperty("numero").GetInt32());
    }

    [Fact(DisplayName = "RN-42: los filtros se combinan (estado + rango de fechas objetivo)")]
    public async Task Filtros_Se_Combinan()
    {
        var http = await ClienteAsync();
        var ot = await NuevaOtAsync(http, 21);
        var dia = ot.GetProperty("fechaObjetivo").GetString();
        Assert.Equal(HttpStatusCode.OK, (await CambiarAsync(http, ot.GetProperty("id").GetInt32(), 41)).StatusCode);

        var json = await Json(await http.GetAsync($"{Ruta}?estado=41&fechaDesde={dia}&fechaHasta={dia}"));
        Assert.Equal(1, json.GetProperty("total").GetInt32());
        Assert.Equal(ot.GetProperty("numero").GetInt32(), json.GetProperty("items")[0].GetProperty("numero").GetInt32());

        var ninguna = await Json(await http.GetAsync($"{Ruta}?estado=40&fechaDesde={dia}&fechaHasta={dia}"));
        Assert.Equal(0, ninguna.GetProperty("total").GetInt32());
    }

    [Theory(DisplayName = "RN-40: tamaño de página fuera de rango o estado inválido dan 400")]
    [InlineData("tamanoPagina=500")]
    [InlineData("tamanoPagina=0")]
    [InlineData("estado=99")]
    [InlineData("fechaDesde=2026-12-01&fechaHasta=2026-11-01")]
    public async Task Filtro_Invalido_Da_400(string consulta)
    {
        var r = await (await ClienteAsync()).GetAsync($"{Ruta}?{consulta}");
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("validacion.error", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-03: el especialista puede consultar OT")]
    public async Task Especialista_Puede_Consultar()
    {
        var http = await ClienteAsync("especialista");
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync(Ruta)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync($"{Ruta}?numero=1")).StatusCode);
    }

    // ---------- RF-09 ----------

    [Fact(DisplayName = "RF-09/RN-37/RN-38: Pendiente → Concluido → Pendiente (reapertura) queda en el historial")]
    public async Task Concluir_Y_Reabrir_Queda_En_El_Historial()
    {
        var http = await ClienteAsync();
        var id = (await NuevaOtAsync(http, 22)).GetProperty("id").GetInt32();

        var concluida = await CambiarAsync(http, id, 41);
        Assert.Equal(HttpStatusCode.OK, concluida.StatusCode);
        var json = await Json(concluida);
        Assert.Equal(41, json.GetProperty("estado").GetInt32());
        Assert.Equal("planificador", json.GetProperty("modificadoPor").GetString());

        Assert.Equal(HttpStatusCode.OK, (await CambiarAsync(http, id, 40)).StatusCode);

        var detalle = await Json(await http.GetAsync($"{Ruta}/{id}"));
        Assert.Equal(40, detalle.GetProperty("estado").GetInt32());
        var historial = detalle.GetProperty("historial").EnumerateArray().ToList();
        Assert.Equal(2, historial.Count);
        Assert.Equal((40, 41), (historial[0].GetProperty("estadoAnterior").GetInt32(), historial[0].GetProperty("estadoNuevo").GetInt32()));
        Assert.Equal((41, 40), (historial[1].GetProperty("estadoAnterior").GetInt32(), historial[1].GetProperty("estadoNuevo").GetInt32()));
        Assert.All(historial, h => Assert.Equal("planificador", h.GetProperty("usuario").GetString()));
    }

    [Fact(DisplayName = "RN-37: Pendiente → Anulación → Concluido por anulación, y desde ahí no hay salida")]
    public async Task Anular_Hasta_El_Final()
    {
        var http = await ClienteAsync();
        var id = (await NuevaOtAsync(http, 29)).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await CambiarAsync(http, id, 38)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CambiarAsync(http, id, 42)).StatusCode);

        var otra = await CambiarAsync(http, id, 40);
        Assert.Equal(HttpStatusCode.Conflict, otra.StatusCode);
        Assert.Equal("ot.transicion-invalida", (await Json(otra)).GetProperty("codigo").GetString());
    }

    [Theory(DisplayName = "RN-37: transiciones prohibidas desde Pendiente dan 409 y no cambian nada")]
    [InlineData(37)]
    [InlineData(39)]
    [InlineData(42)]
    [InlineData(40)]
    public async Task Transicion_Prohibida_Da_409(int estado)
    {
        var http = await ClienteAsync();
        var id = (await NuevaOtAsync(http, 30)).GetProperty("id").GetInt32();
        var r = await CambiarAsync(http, id, estado);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("ot.transicion-invalida", (await Json(r)).GetProperty("codigo").GetString());

        var detalle = await Json(await http.GetAsync($"{Ruta}/{id}"));
        Assert.Equal(40, detalle.GetProperty("estado").GetInt32());
        Assert.Equal(0, detalle.GetProperty("historial").GetArrayLength());
    }

    [Theory(DisplayName = "RN-36: un estado fuera del catálogo da 400")]
    [InlineData(0)]
    [InlineData(43)]
    public async Task Estado_Fuera_Del_Catalogo_Da_400(int estado)
    {
        var r = await CambiarAsync(await ClienteAsync(), 1, estado);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("validacion.error", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RF-09: cambiar el estado de una OT inexistente da 404")]
    public async Task Ot_Inexistente_Da_404()
    {
        var r = await CambiarAsync(await ClienteAsync(), 999999, 41);
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("ot.no-encontrada", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-03: el especialista no puede cambiar el estado (403)")]
    public async Task Especialista_No_Puede_Cambiar_Estado() =>
        Assert.Equal(HttpStatusCode.Forbidden, (await CambiarAsync(await ClienteAsync("especialista"), 1, 41)).StatusCode);

    [Fact(DisplayName = "RN-01: sin token la consulta y el cambio de estado dan 401")]
    public async Task Sin_Token_Da_401()
    {
        var http = fabrica.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync(Ruta)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await CambiarAsync(http, 1, 41)).StatusCode);
    }
}
