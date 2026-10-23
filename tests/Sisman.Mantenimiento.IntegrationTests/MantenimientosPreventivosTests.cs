using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sisman.Mantenimiento.IntegrationTests;

/// <summary>
/// RF-02 a RF-05. La semilla crea 30 MP: códigos 1 a 12 Nuevo, 13 a 18 Modificado, 19 a 30 Aprobado
/// (de ellos 23 a 30 con una fecha programada y 1 a 3 de esos con OT). Cada prueba que modifica usa un MP distinto.
/// </summary>
public class MantenimientosPreventivosTests(ApiFactory fabrica) : IClassFixture<ApiFactory>
{
    private const string Ruta = "/api/v1/mantenimientos-preventivos";

    private async Task<HttpClient> ClienteAsync(string usuario = "planificador")
    {
        var clave = usuario == "planificador" ? ApiFactory.ClavePlanificador : ApiFactory.ClaveEspecialista;
        var http = fabrica.CreateClient();
        var r = await http.PostAsJsonAsync("/api/v1/auth/login", new { usuario, clave });
        var token = (await Json(r)).GetProperty("token").GetString();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    private static async Task<JsonElement> Json(HttpResponseMessage r) =>
        JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync());

    /// <summary>Devuelve el MP con ese código (los códigos de la semilla son MP-0000001 a MP-0000030).</summary>
    private async Task<JsonElement> PorCodigoAsync(HttpClient http, int numero)
    {
        var r = await http.GetAsync($"{Ruta}?codigo=MP-{numero:D7}");
        var items = (await Json(r)).GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        return items[0];
    }

    private static object Entrada(JsonElement mp, string? descripcion = null, int? frecuencia = null, byte? mes = null, bool sinMes = false) => new
    {
        descripcion = descripcion ?? mp.GetProperty("descripcion").GetString(),
        maquinaId = mp.GetProperty("maquinaId").GetInt32(),
        sistemaId = mp.GetProperty("sistemaId").GetInt32(),
        equipoId = (int?)null,
        tipoTrabajoId = mp.GetProperty("tipoTrabajoId").GetInt32(),
        fuenteId = mp.GetProperty("fuenteId").GetInt32(),
        oportunidadId = mp.GetProperty("oportunidadId").GetInt32(),
        especialidadId = mp.GetProperty("especialidadId").GetInt32(),
        frecuencia = frecuencia ?? mp.GetProperty("frecuencia").GetInt32(),
        valorFrecuencia = mp.GetProperty("valorFrecuencia").GetInt32(),
        mes = sinMes ? null : mes ?? (mp.GetProperty("mes").ValueKind == JsonValueKind.Null ? (byte?)null : mp.GetProperty("mes").GetByte())
    };

    // ---------- RF-02 ----------

    [Fact(DisplayName = "RF-02/RN-40: sin filtros devuelve la primera página de 50 con el total")]
    public async Task Consulta_Sin_Filtros_Devuelve_Total_Y_Pagina()
    {
        var json = await Json(await (await ClienteAsync()).GetAsync(Ruta));
        Assert.Equal(30, json.GetProperty("total").GetInt32());
        Assert.Equal(50, json.GetProperty("tamanoPagina").GetInt32());
        Assert.Equal(30, json.GetProperty("items").GetArrayLength());
    }

    [Fact(DisplayName = "RN-40: la paginación se hace en el servidor")]
    public async Task Paginacion_En_El_Servidor()
    {
        var http = await ClienteAsync();
        var p2 = await Json(await http.GetAsync($"{Ruta}?pagina=2&tamanoPagina=12"));
        Assert.Equal(12, p2.GetProperty("items").GetArrayLength());
        Assert.Equal(30, p2.GetProperty("total").GetInt32());
        var p3 = await Json(await http.GetAsync($"{Ruta}?pagina=3&tamanoPagina=12"));
        Assert.Equal(6, p3.GetProperty("items").GetArrayLength());
    }

    [Theory(DisplayName = "RN-40: un tamaño de página fuera de 1 a 200 da 400")]
    [InlineData(500)]
    [InlineData(0)]
    public async Task Tamano_Fuera_De_Rango_Da_400(int tamano)
    {
        var r = await (await ClienteAsync()).GetAsync($"{Ruta}?tamanoPagina={tamano}");
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("validacion.error", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-42: los filtros se combinan (estado + tipo de trabajo)")]
    public async Task Filtros_Se_Combinan()
    {
        var http = await ClienteAsync();
        var aprobados = await Json(await http.GetAsync($"{Ruta}?estado=Aprobado"));
        Assert.Equal(12, aprobados.GetProperty("total").GetInt32());
        Assert.All(aprobados.GetProperty("items").EnumerateArray(), m => Assert.Equal("Aprobado", m.GetProperty("estado").GetString()));

        var tipo = aprobados.GetProperty("items")[0].GetProperty("tipoTrabajoId").GetInt32();
        var combinado = await Json(await http.GetAsync($"{Ruta}?estado=Aprobado&tipoTrabajoId={tipo}"));
        Assert.InRange(combinado.GetProperty("total").GetInt32(), 1, 12);
        Assert.All(combinado.GetProperty("items").EnumerateArray(), m =>
        {
            Assert.Equal("Aprobado", m.GetProperty("estado").GetString());
            Assert.Equal(tipo, m.GetProperty("tipoTrabajoId").GetInt32());
        });
    }

    [Fact(DisplayName = "RF-02: filtra por código")]
    public async Task Filtra_Por_Codigo()
    {
        var mp = await PorCodigoAsync(await ClienteAsync(), 7);
        Assert.Equal("MP-0000007", mp.GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RF-02: el detalle trae las fechas programadas con su número de OT")]
    public async Task Detalle_Trae_Fechas_Y_Ot()
    {
        var http = await ClienteAsync();
        var lista = await PorCodigoAsync(http, 23);
        var detalle = await Json(await http.GetAsync($"{Ruta}/{lista.GetProperty("id").GetInt32()}"));
        var fechas = detalle.GetProperty("fechas");
        Assert.Equal(1, fechas.GetArrayLength());
        Assert.Equal(1, fechas[0].GetProperty("numeroOt").GetInt32());
    }

    [Fact(DisplayName = "RF-02: un MP inexistente da 404 mp.no-encontrado")]
    public async Task Mp_Inexistente_Da_404()
    {
        var r = await (await ClienteAsync()).GetAsync($"{Ruta}/999999");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("mp.no-encontrado", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-03: el especialista puede consultar")]
    public async Task Especialista_Puede_Consultar() =>
        Assert.Equal(HttpStatusCode.OK, (await (await ClienteAsync("especialista")).GetAsync(Ruta)).StatusCode);

    // ---------- RF-03 ----------

    [Fact(DisplayName = "RF-03/RN-14: editar deja el MP Modificado con usuario y fecha")]
    public async Task Editar_Deja_Modificado()
    {
        var http = await ClienteAsync();
        var mp = await PorCodigoAsync(http, 1);
        var r = await http.PutAsJsonAsync($"{Ruta}/{mp.GetProperty("id").GetInt32()}", Entrada(mp, descripcion: "Descripción corregida"));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Json(r);
        Assert.Equal("Modificado", json.GetProperty("estado").GetString());
        Assert.Equal("Descripción corregida", json.GetProperty("descripcion").GetString());
        Assert.Equal("planificador", json.GetProperty("modificadoPor").GetString());
        Assert.NotEqual(JsonValueKind.Null, json.GetProperty("modificadoEn").ValueKind);
    }

    [Fact(DisplayName = "RN-03: el especialista no puede editar (403)")]
    public async Task Especialista_No_Puede_Editar()
    {
        var mp = await PorCodigoAsync(await ClienteAsync(), 2);
        var r = await (await ClienteAsync("especialista")).PutAsJsonAsync($"{Ruta}/{mp.GetProperty("id").GetInt32()}", Entrada(mp));
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact(DisplayName = "RN-11: editar con descripción vacía da 400 con el campo")]
    public async Task Editar_Sin_Descripcion_Da_400()
    {
        var http = await ClienteAsync();
        var mp = await PorCodigoAsync(http, 3);
        var r = await http.PutAsJsonAsync($"{Ruta}/{mp.GetProperty("id").GetInt32()}", Entrada(mp, descripcion: ""));
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.True((await Json(r)).GetProperty("errors").TryGetProperty("Descripcion", out _));
    }

    [Theory(DisplayName = "RN-12: frecuencia 0 o 5 da 400")]
    [InlineData(0)]
    [InlineData(5)]
    public async Task Frecuencia_Fuera_Del_Catalogo_Da_400(int frecuencia)
    {
        var http = await ClienteAsync();
        var mp = await PorCodigoAsync(http, 3);
        var r = await http.PutAsJsonAsync($"{Ruta}/{mp.GetProperty("id").GetInt32()}", Entrada(mp, frecuencia: frecuencia));
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact(DisplayName = "RN-13: frecuencia mensual sin mes da 400; con mes se guarda")]
    public async Task Frecuencia_Mensual_Exige_Mes()
    {
        var http = await ClienteAsync();
        var mp = await PorCodigoAsync(http, 4);
        var id = mp.GetProperty("id").GetInt32();

        var sinMes = await http.PutAsJsonAsync($"{Ruta}/{id}", Entrada(mp, frecuencia: 3, sinMes: true));
        Assert.Equal(HttpStatusCode.BadRequest, sinMes.StatusCode);

        var conMes = await http.PutAsJsonAsync($"{Ruta}/{id}", Entrada(mp, frecuencia: 3, mes: 6));
        Assert.Equal(HttpStatusCode.OK, conMes.StatusCode);
        Assert.Equal(6, (await Json(conMes)).GetProperty("mes").GetInt32());
    }

    [Fact(DisplayName = "RN-11: una máquina inexistente da 400 mp.referencia-invalida")]
    public async Task Referencia_Inexistente_Da_400()
    {
        var http = await ClienteAsync();
        var mp = await PorCodigoAsync(http, 5);
        var entrada = new
        {
            descripcion = "x", maquinaId = 999999, sistemaId = (int?)null, equipoId = (int?)null,
            tipoTrabajoId = mp.GetProperty("tipoTrabajoId").GetInt32(), fuenteId = mp.GetProperty("fuenteId").GetInt32(),
            oportunidadId = mp.GetProperty("oportunidadId").GetInt32(), especialidadId = mp.GetProperty("especialidadId").GetInt32(),
            frecuencia = 1, valorFrecuencia = 1, mes = (byte?)null
        };
        var r = await http.PutAsJsonAsync($"{Ruta}/{mp.GetProperty("id").GetInt32()}", entrada);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("mp.referencia-invalida", (await Json(r)).GetProperty("codigo").GetString());
    }

    // ---------- RF-04 ----------

    [Fact(DisplayName = "RF-04/RN-15: aprobar pasa a Aprobado y aprobar de nuevo da 409")]
    public async Task Aprobar_Y_Reaprobar()
    {
        var http = await ClienteAsync();
        var id = (await PorCodigoAsync(http, 8)).GetProperty("id").GetInt32();

        var r = await http.PostAsync($"{Ruta}/{id}/aprobacion", null);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Json(r);
        Assert.Equal("Aprobado", json.GetProperty("estado").GetString());
        Assert.Equal("planificador", json.GetProperty("aprobadoPor").GetString());

        var otra = await http.PostAsync($"{Ruta}/{id}/aprobacion", null);
        Assert.Equal(HttpStatusCode.Conflict, otra.StatusCode);
        Assert.Equal("mp.ya-aprobado", (await Json(otra)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RF-04/RN-16: un MP Modificado se puede aprobar")]
    public async Task Modificado_Se_Puede_Aprobar()
    {
        var http = await ClienteAsync();
        var mp = await PorCodigoAsync(http, 13);
        Assert.Equal("Modificado", mp.GetProperty("estado").GetString());
        var r = await http.PostAsync($"{Ruta}/{mp.GetProperty("id").GetInt32()}/aprobacion", null);
        Assert.Equal("Aprobado", (await Json(r)).GetProperty("estado").GetString());
    }

    [Fact(DisplayName = "RN-03: el especialista no puede aprobar (403)")]
    public async Task Especialista_No_Puede_Aprobar()
    {
        var id = (await PorCodigoAsync(await ClienteAsync(), 9)).GetProperty("id").GetInt32();
        var r = await (await ClienteAsync("especialista")).PostAsync($"{Ruta}/{id}/aprobacion", null);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact(DisplayName = "RF-04: aprobar un MP inexistente da 404")]
    public async Task Aprobar_Inexistente_Da_404() =>
        Assert.Equal(HttpStatusCode.NotFound, (await (await ClienteAsync()).PostAsync($"{Ruta}/999999/aprobacion", null)).StatusCode);

    // ---------- RF-05 ----------

    private static string Manana(int dias) => DateOnly.FromDateTime(DateTime.Now).AddDays(dias).ToString("yyyy-MM-dd");

    [Fact(DisplayName = "RF-05/RN-24: programar una fecha la crea sin OT y actualiza las fechas del MP")]
    public async Task Programar_Fecha_Actualiza_El_Mp()
    {
        var http = await ClienteAsync();
        var id = (await PorCodigoAsync(http, 19)).GetProperty("id").GetInt32();

        var r = await http.PostAsJsonAsync($"{Ruta}/{id}/fechas", new { fechaInicio = Manana(2), fechaFin = Manana(4) });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        Assert.Contains($"{Ruta}/{id}", r.Headers.Location!.ToString());
        Assert.Equal(JsonValueKind.Null, (await Json(r)).GetProperty("numeroOt").ValueKind);

        var mp = await Json(await http.GetAsync($"{Ruta}/{id}"));
        Assert.Equal(Manana(2), mp.GetProperty("fechaInicio").GetString());
        Assert.Equal(Manana(2), mp.GetProperty("fechaProxima").GetString());
        Assert.Equal(Manana(4), mp.GetProperty("fechaUltima").GetString());
        Assert.Equal(1, mp.GetProperty("fechas").GetArrayLength());
    }

    [Fact(DisplayName = "RN-20: inicio igual a hoy da 400")]
    public async Task Inicio_Hoy_Da_400()
    {
        var http = await ClienteAsync();
        var id = (await PorCodigoAsync(http, 20)).GetProperty("id").GetInt32();
        var r = await http.PostAsJsonAsync($"{Ruta}/{id}/fechas", new { fechaInicio = Manana(0), fechaFin = Manana(1) });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("fecha.inicio-pasada", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-21: inicio mayor que fin da 400")]
    public async Task Inicio_Mayor_Que_Fin_Da_400()
    {
        var http = await ClienteAsync();
        var id = (await PorCodigoAsync(http, 20)).GetProperty("id").GetInt32();
        var r = await http.PostAsJsonAsync($"{Ruta}/{id}/fechas", new { fechaInicio = Manana(5), fechaFin = Manana(3) });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("fecha.rango-invalido", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-22: un MP sin aprobar da 409 mp.no-aprobado")]
    public async Task Mp_No_Aprobado_Da_409()
    {
        var http = await ClienteAsync();
        var id = (await PorCodigoAsync(http, 10)).GetProperty("id").GetInt32();
        var r = await http.PostAsJsonAsync($"{Ruta}/{id}/fechas", new { fechaInicio = Manana(2), fechaFin = Manana(2) });
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("mp.no-aprobado", (await Json(r)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-23: repetir el inicio da 409 fecha.duplicada")]
    public async Task Fecha_Repetida_Da_409()
    {
        var http = await ClienteAsync();
        var id = (await PorCodigoAsync(http, 21)).GetProperty("id").GetInt32();
        var cuerpo = new { fechaInicio = Manana(6), fechaFin = Manana(7) };
        Assert.Equal(HttpStatusCode.Created, (await http.PostAsJsonAsync($"{Ruta}/{id}/fechas", cuerpo)).StatusCode);
        var otra = await http.PostAsJsonAsync($"{Ruta}/{id}/fechas", cuerpo);
        Assert.Equal(HttpStatusCode.Conflict, otra.StatusCode);
        Assert.Equal("fecha.duplicada", (await Json(otra)).GetProperty("codigo").GetString());
    }

    [Fact(DisplayName = "RN-03: el especialista no puede programar fechas (403)")]
    public async Task Especialista_No_Puede_Programar()
    {
        var id = (await PorCodigoAsync(await ClienteAsync(), 22)).GetProperty("id").GetInt32();
        var r = await (await ClienteAsync("especialista")).PostAsJsonAsync($"{Ruta}/{id}/fechas", new { fechaInicio = Manana(9), fechaFin = Manana(9) });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact(DisplayName = "RN-01: sin token las operaciones de escritura dan 401")]
    public async Task Sin_Token_Da_401()
    {
        var http = fabrica.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsync($"{Ruta}/1/aprobacion", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync($"{Ruta}/1")).StatusCode);
    }
}
