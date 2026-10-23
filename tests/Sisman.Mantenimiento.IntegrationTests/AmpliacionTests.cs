using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClosedXML.Excel;

namespace Sisman.Mantenimiento.IntegrationTests;

/// <summary>
/// RF-01, RF-06, RF-10, RF-12 y catálogos. Semilla: 30 MP (19 a 30 aprobados), fechas en hoy + 25 a hoy + 32
/// para los MP 23 a 30, y OT en las fechas de los MP 23 a 25. Las fechas que crean estas pruebas van a
/// hoy + 500 o más, fuera del rango del planning que se prueba.
/// </summary>
public class AmpliacionTests(ApiFactory fabrica) : IClassFixture<ApiFactory>
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

    private static string Dia(int dias) => DateOnly.FromDateTime(DateTime.Now).AddDays(dias).ToString("yyyy-MM-dd");

    private static int _desfase = 500;

    /// <summary>Entrada válida armada con los catálogos y la primera máquina que publica la API.</summary>
    private static async Task<Dictionary<string, object?>> EntradaValidaAsync(HttpClient http)
    {
        var cat = await Json(await http.GetAsync("/api/v1/catalogos"));
        var maquina = (await Json(await http.GetAsync("/api/v1/maquinas")))[0];
        int Primero(string lista) => cat.GetProperty(lista)[0].GetProperty("id").GetInt32();
        return new()
        {
            ["descripcion"] = "Inspeccionar fajas de transmisión",
            ["maquinaId"] = maquina.GetProperty("id").GetInt32(),
            ["sistemaId"] = maquina.GetProperty("sistemas")[0].GetProperty("id").GetInt32(),
            ["equipoId"] = maquina.GetProperty("sistemas")[0].GetProperty("equipos")[0].GetProperty("id").GetInt32(),
            ["tipoTrabajoId"] = Primero("tiposTrabajo"),
            ["fuenteId"] = Primero("fuentes"),
            ["oportunidadId"] = Primero("oportunidades"),
            ["especialidadId"] = Primero("especialidades"),
            ["frecuencia"] = 3,
            ["valorFrecuencia"] = 1,
            ["mes"] = 4
        };
    }

    // ---------- Catálogos ----------

    [Fact(DisplayName = "Catálogos: devuelve las cuatro listas y las máquinas con sistemas y equipos")]
    public async Task Catalogos_Y_Maquinas()
    {
        var http = await ClienteAsync("especialista");
        var cat = await Json(await http.GetAsync("/api/v1/catalogos"));
        foreach (var lista in new[] { "tiposTrabajo", "fuentes", "oportunidades", "especialidades" })
            Assert.True(cat.GetProperty(lista).GetArrayLength() > 0, lista);

        var maquinas = await Json(await http.GetAsync("/api/v1/maquinas"));
        Assert.Equal(8, maquinas.GetArrayLength());
        var m = maquinas[0];
        Assert.False(string.IsNullOrEmpty(m.GetProperty("planta").GetString()));
        Assert.True(m.GetProperty("sistemas")[0].GetProperty("equipos").GetArrayLength() > 0);
    }

    // ---------- RF-01 ----------

    [Fact(DisplayName = "RF-01/RN-10: el alta crea un MP Nuevo con código correlativo y los siguientes son consecutivos")]
    public async Task Alta_Con_Codigo_Correlativo()
    {
        var http = await ClienteAsync();
        var entrada = await EntradaValidaAsync(http);

        var r1 = await http.PostAsJsonAsync(RutaMp, entrada);
        Assert.Equal(HttpStatusCode.Created, r1.StatusCode);
        var mp1 = await Json(r1);
        Assert.Equal("Nuevo", mp1.GetProperty("estado").GetString());
        Assert.Equal(4, mp1.GetProperty("mes").GetInt32());
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync(r1.Headers.Location)).StatusCode);

        var mp2 = await Json(await http.PostAsJsonAsync(RutaMp, entrada));
        var n1 = int.Parse(mp1.GetProperty("codigo").GetString()!["MP-".Length..]);
        var n2 = int.Parse(mp2.GetProperty("codigo").GetString()!["MP-".Length..]);
        Assert.True(n1 > 30, $"el correlativo debe seguir a la semilla (MP-0000030); fue {n1}");
        Assert.Equal(n1 + 1, n2);
        Assert.Matches(@"^MP-\d{7}$", mp2.GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RF-01/RN-11 y RN-13: alta sin descripción o mensual sin mes da 400")]
    public async Task Alta_Invalida_Da_400()
    {
        var http = await ClienteAsync();
        var sinDescripcion = await EntradaValidaAsync(http);
        sinDescripcion["descripcion"] = "";
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync(RutaMp, sinDescripcion)).StatusCode);

        var sinMes = await EntradaValidaAsync(http);
        sinMes["mes"] = null;
        var r = await http.PostAsJsonAsync(RutaMp, sinMes);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.True((await Json(r)).GetProperty("errors").TryGetProperty("Mes", out _));
    }

    [Fact(DisplayName = "RF-01: un equipo que no pertenece al sistema da 400 mp.referencia-invalida")]
    public async Task Alta_Con_Equipo_Ajeno_Da_400()
    {
        var http = await ClienteAsync();
        var entrada = await EntradaValidaAsync(http);
        var otraMaquina = (await Json(await http.GetAsync("/api/v1/maquinas")))[1];
        entrada["equipoId"] = otraMaquina.GetProperty("sistemas")[0].GetProperty("equipos")[0].GetProperty("id").GetInt32();
        var r = await http.PostAsJsonAsync(RutaMp, entrada);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("mp.referencia-invalida", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-03: el especialista no puede registrar MP (403)")]
    public async Task Especialista_No_Puede_Registrar()
    {
        var entrada = await EntradaValidaAsync(await ClienteAsync());
        Assert.Equal(HttpStatusCode.Forbidden, (await (await ClienteAsync("especialista")).PostAsJsonAsync(RutaMp, entrada)).StatusCode);
    }

    // ---------- RF-06 ----------

    private static async Task<int> NuevaFechaAsync(HttpClient http, int mpId)
    {
        var d = Dia(Interlocked.Increment(ref _desfase));
        var r = await http.PostAsJsonAsync($"{RutaMp}/{mpId}/fechas", new { fechaInicio = d, fechaFin = d });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await Json(r)).GetProperty("id").GetInt32();
    }

    [Fact(DisplayName = "RF-06/RN-25: se elimina una fecha sin OT (204) y desaparece del MP")]
    public async Task Elimina_Fecha_Sin_Ot()
    {
        var http = await ClienteAsync();
        var mpId = await IdMpAsync(http, 19);
        var fechaId = await NuevaFechaAsync(http, mpId);

        var r = await http.DeleteAsync($"{RutaMp}/{mpId}/fechas/{fechaId}");
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);

        var mp = await Json(await http.GetAsync($"{RutaMp}/{mpId}"));
        Assert.DoesNotContain(mp.GetProperty("fechas").EnumerateArray(), f => f.GetProperty("id").GetInt32() == fechaId);
        Assert.Equal(HttpStatusCode.NotFound, (await http.DeleteAsync($"{RutaMp}/{mpId}/fechas/{fechaId}")).StatusCode);
    }

    [Fact(DisplayName = "RN-25: una fecha con OT no se elimina (409 fecha.con-ot)")]
    public async Task Fecha_Con_Ot_No_Se_Elimina()
    {
        var http = await ClienteAsync();
        var mpId = await IdMpAsync(http, 23);
        var fecha = (await Json(await http.GetAsync($"{RutaMp}/{mpId}"))).GetProperty("fechas")[0];
        Assert.NotEqual(JsonValueKind.Null, fecha.GetProperty("numeroOt").ValueKind);

        var r = await http.DeleteAsync($"{RutaMp}/{mpId}/fechas/{fecha.GetProperty("id").GetInt32()}");
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("fecha.con-ot", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RF-06: la fecha de otro MP da 404; el especialista no puede eliminar (403)")]
    public async Task Fecha_Ajena_Y_Especialista()
    {
        var http = await ClienteAsync();
        var fechaId = await NuevaFechaAsync(http, await IdMpAsync(http, 20));
        var otroMp = await IdMpAsync(http, 21);
        Assert.Equal(HttpStatusCode.NotFound, (await http.DeleteAsync($"{RutaMp}/{otroMp}/fechas/{fechaId}")).StatusCode);

        var mpId = await IdMpAsync(http, 20);
        Assert.Equal(HttpStatusCode.Forbidden, (await (await ClienteAsync("especialista")).DeleteAsync($"{RutaMp}/{mpId}/fechas/{fechaId}")).StatusCode);
    }

    // ---------- RF-12 ----------

    [Fact(DisplayName = "RF-12: la exportación devuelve un Excel con una fila por OT del filtro")]
    public async Task Exporta_Excel_Con_Los_Filtros()
    {
        var http = await ClienteAsync();
        var total = (await Json(await http.GetAsync("/api/v1/ordenes-trabajo?estado=40"))).GetProperty("total").GetInt32();
        Assert.True(total > 0);

        var r = await http.GetAsync("/api/v1/ordenes-trabajo/exportacion?estado=40");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", r.Content.Headers.ContentType?.MediaType);
        Assert.EndsWith(".xlsx", r.Content.Headers.ContentDisposition?.FileNameStar ?? r.Content.Headers.ContentDisposition?.FileName?.Trim('"'));

        using var libro = new XLWorkbook(await r.Content.ReadAsStreamAsync());
        var hoja = libro.Worksheet(1);
        Assert.Equal("Número", hoja.Cell(1, 1).GetString());
        Assert.Equal("Estado", hoja.Cell(1, 2).GetString());
        Assert.Equal(total, hoja.LastRowUsed()!.RowNumber() - 1);
        Assert.Equal("Pendiente", hoja.Cell(2, 2).GetString());
    }

    [Fact(DisplayName = "RF-12: el especialista no exporta (403) y un filtro inválido da 400")]
    public async Task Exportacion_Permisos_Y_Filtros()
    {
        Assert.Equal(HttpStatusCode.Forbidden, (await (await ClienteAsync("especialista")).GetAsync("/api/v1/ordenes-trabajo/exportacion")).StatusCode);
        var r = await (await ClienteAsync()).GetAsync("/api/v1/ordenes-trabajo/exportacion?estado=99");
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    // ---------- RF-10 ----------

    [Fact(DisplayName = "RF-10/RN-43: el planning cuenta por día solo las fechas sin OT y rellena los días vacíos")]
    public async Task Planning_Cuenta_Fechas_Sin_Ot()
    {
        var json = await Json(await (await ClienteAsync("especialista")).GetAsync($"/api/v1/planning?desde={Dia(20)}&hasta={Dia(40)}"));
        // Semilla: fechas en hoy + 25 a hoy + 32; las de hoy + 25, 26 y 27 tienen OT.
        Assert.Equal(5, json.GetProperty("total").GetInt32());
        var dias = json.GetProperty("dias").EnumerateArray().ToList();
        Assert.Equal(21, dias.Count);
        int En(int d) => dias.Single(x => x.GetProperty("dia").GetString() == Dia(d)).GetProperty("cantidad").GetInt32();
        Assert.Equal(0, En(25));
        Assert.Equal(1, En(28));
        Assert.Equal(1, En(32));
        Assert.Equal(0, En(33));
    }

    [Fact(DisplayName = "RF-10: los filtros del planning se aplican (una máquina inexistente da cero)")]
    public async Task Planning_Con_Filtros()
    {
        var http = await ClienteAsync();
        var sinFiltro = (await Json(await http.GetAsync($"/api/v1/planning?desde={Dia(20)}&hasta={Dia(40)}"))).GetProperty("total").GetInt32();
        var planta = (await Json(await http.GetAsync("/api/v1/maquinas")))[0].GetProperty("plantaId").GetInt32();
        var conPlanta = (await Json(await http.GetAsync($"/api/v1/planning?desde={Dia(20)}&hasta={Dia(40)}&plantaId={planta}"))).GetProperty("total").GetInt32();
        var ninguna = (await Json(await http.GetAsync($"/api/v1/planning?desde={Dia(20)}&hasta={Dia(40)}&maquinaId=999999"))).GetProperty("total").GetInt32();
        Assert.Equal(sinFiltro, conPlanta); // la semilla tiene una sola planta
        Assert.Equal(0, ninguna);
    }

    [Theory(DisplayName = "RF-10: rango invertido, de más de 92 días o sin fechas da 400")]
    [InlineData(40, 20)]
    [InlineData(0, 100)]
    public async Task Planning_Rango_Invalido_Da_400(int desde, int hasta)
    {
        var r = await (await ClienteAsync()).GetAsync($"/api/v1/planning?desde={Dia(desde)}&hasta={Dia(hasta)}");
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("validacion.error", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RF-10: sin fechas el planning da 400")]
    public async Task Planning_Sin_Fechas_Da_400() =>
        Assert.Equal(HttpStatusCode.BadRequest, (await (await ClienteAsync()).GetAsync("/api/v1/planning")).StatusCode);

    // ---------- Mensajes ----------

    [Fact(DisplayName = "RN-37: el mensaje de transición prohibida usa nombres legibles")]
    public async Task Mensaje_De_Transicion_Legible()
    {
        var http = await ClienteAsync();
        var r = await http.PutAsJsonAsync("/api/v1/ordenes-trabajo/1/estado", new { estado = 39 });
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("No se puede pasar de «Pendiente» a «En programación».", (await Json(r)).GetProperty("detail").GetString());
    }
}
