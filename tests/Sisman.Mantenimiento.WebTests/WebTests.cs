using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.WebTests;

/// <summary>API simulada: responde según la ruta, sin levantar la API real ni una base de datos.</summary>
public class ApiFalsa : HttpMessageHandler
{
    public const string ClaveBuena = "clave-buena";
    public List<string> Llamadas { get; } = [];

    private static HttpResponseMessage Json(HttpStatusCode estado, string cuerpo, string tipo = "application/json") =>
        new(estado) { Content = new StringContent(cuerpo, Encoding.UTF8, tipo) };

    private const string Mp = """
        {"id":7,"codigo":"MP-0000007","estado":"Nuevo","descripcion":"Lubricar rodamientos","maquinaId":1,"sistemaId":1,
         "frecuencia":2,"valorFrecuencia":1,"mes":null,"fechaInicio":null,"fechaProxima":null,"fechaUltima":null,
         "modificadoPor":null,"modificadoEn":null,"aprobadoPor":null,"aprobadoEn":null,"fechas":[]}
        """;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        var ruta = r.RequestUri!.PathAndQuery;
        Llamadas.Add($"{r.Method} {ruta} {(r.Headers.Authorization is null ? "sin-token" : "con-token")}");

        if (ruta == "/api/v1/auth/login")
        {
            var cuerpo = await r.Content!.ReadAsStringAsync(ct);
            if (!cuerpo.Contains(ClaveBuena))
                return Json(HttpStatusCode.Unauthorized, """{"status":401,"codigo":"auth.credenciales-invalidas","detail":"Usuario o clave incorrectos."}""", "application/problem+json");
            var perfil = cuerpo.Contains("especialista") ? "Especialista" : "Planificador";
            return Json(HttpStatusCode.OK, $$"""{"token":"t-{{perfil}}","venceEn":"{{DateTime.UtcNow.AddMinutes(30):O}}","perfil":"{{perfil}}"}""");
        }
        if (ruta.StartsWith("/api/v1/mantenimientos-preventivos?"))
            return Json(HttpStatusCode.OK, $$"""{"items":[{{Mp}}],"paginaActual":1,"tamanoPagina":50,"total":1}""");
        if (ruta == "/api/v1/mantenimientos-preventivos/7")
            return Json(HttpStatusCode.OK, Mp);
        if (ruta == "/api/v1/mantenimientos-preventivos/7/aprobacion")
            return Json(HttpStatusCode.Conflict, """{"status":409,"codigo":"mp.ya-aprobado","detail":"El MP-0000007 ya está aprobado."}""", "application/problem+json");
        if (ruta == "/api/v1/catalogos")
            return Json(HttpStatusCode.OK, """{"tiposTrabajo":[{"id":1,"categoria":"TipoTrabajo","nombre":"Lubricación"}],"fuentes":[{"id":2,"categoria":"FuenteInformacion","nombre":"Manual"}],"oportunidades":[{"id":3,"categoria":"OportunidadIntervencion","nombre":"Parada"}],"especialidades":[{"id":4,"categoria":"Especialidad","nombre":"Mecánica"}]}""");
        if (ruta == "/api/v1/maquinas")
            return Json(HttpStatusCode.OK, """[{"id":1,"codigo":"MAQ-001","nombre":"Prensa","areaId":1,"area":"Prensa","plantaId":1,"planta":"Planta","sistemas":[{"id":1,"nombre":"Hidráulico","equipos":[{"id":1,"nombre":"Bomba"}]}]}]""");
        if (ruta.StartsWith("/api/v1/planning?"))
        {
            var q = System.Web.HttpUtility.ParseQueryString(r.RequestUri.Query);
            var desde = DateOnly.Parse(q["desde"]!);
            var hasta = DateOnly.Parse(q["hasta"]!);
            var dias = string.Join(",", Enumerable.Range(0, hasta.DayNumber - desde.DayNumber + 1)
                .Select(i => $$"""{"dia":"{{desde.AddDays(i):yyyy-MM-dd}}","cantidad":{{(i == 9 ? 3 : 0)}}}"""));
            return Json(HttpStatusCode.OK, $$"""{"desde":"{{desde:yyyy-MM-dd}}","hasta":"{{hasta:yyyy-MM-dd}}","total":3,"dias":[{{dias}}]}""");
        }
        if (ruta.StartsWith("/api/v1/ordenes-trabajo/exportacion"))
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([0x50, 0x4B, 3, 4])
                {
                    Headers =
                    {
                        ContentType = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                        ContentDisposition = new("attachment") { FileName = "ordenes-trabajo.xlsx" }
                    }
                }
            };
        if (ruta == "/api/v1/mantenimientos-preventivos/99")
            return Json(HttpStatusCode.Unauthorized, "");
        return Json(HttpStatusCode.NotFound, """{"status":404,"detail":"No existe."}""", "application/problem+json");
    }
}

public class WebFactory : WebApplicationFactory<Program>
{
    public ApiFalsa Api { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Api:UrlBase", "http://api.falsa/");
        builder.ConfigureTestServices(s =>
            s.AddHttpClient<ApiCliente>().ConfigurePrimaryHttpMessageHandler(() => Api));
    }
}

public class WebTests(WebFactory fabrica) : IClassFixture<WebFactory>
{
    private HttpClient Cliente() =>
        fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    private static string Token(string html) =>
        Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient http, string usuario, string clave)
    {
        var form = await (await http.GetAsync("/login")).Content.ReadAsStringAsync();
        return await http.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Usuario"] = usuario, ["Clave"] = clave, ["__RequestVerificationToken"] = Token(form)
        }));
    }

    [Fact(DisplayName = "W-03: sin sesión, la lista de MP lleva al login")]
    public async Task Sin_Sesion_Va_Al_Login()
    {
        var r = await Cliente().GetAsync("/mp");
        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.Contains("/login", r.Headers.Location!.ToString());
    }

    [Fact(DisplayName = "W-03: credenciales inválidas muestran el aviso y no abren sesión")]
    public async Task Credenciales_Invalidas_Muestran_Aviso()
    {
        var http = Cliente();
        var r = await LoginAsync(http, "planificador", "mala");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("Usuario o clave incorrectos.", await r.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await http.GetAsync("/mp")).StatusCode);
    }

    [Fact(DisplayName = "W-03/W-05: login correcto abre la lista de MP y la web envía el token a la API")]
    public async Task Login_Correcto_Abre_La_Lista_Con_Token()
    {
        var http = Cliente();
        var r = await LoginAsync(http, "planificador", ApiFalsa.ClaveBuena);
        Assert.Equal("/mp", r.Headers.Location!.ToString());

        var lista = await http.GetAsync("/mp");
        Assert.Equal(HttpStatusCode.OK, lista.StatusCode);
        var html = await lista.Content.ReadAsStringAsync();
        Assert.Contains("MP-0000007", html);
        Assert.Contains("Lubricar rodamientos", html);
        Assert.Contains(fabrica.Api.Llamadas, l => l.StartsWith("GET /api/v1/mantenimientos-preventivos?") && l.EndsWith("con-token"));
    }

    [Fact(DisplayName = "W-10: el planificador ve «Aprobar» y el especialista no")]
    public async Task Botones_Segun_Perfil()
    {
        var plan = Cliente();
        await LoginAsync(plan, "planificador", ApiFalsa.ClaveBuena);
        Assert.Contains(">Aprobar<", await (await plan.GetAsync("/mp/7")).Content.ReadAsStringAsync());

        var esp = Cliente();
        await LoginAsync(esp, "especialista", ApiFalsa.ClaveBuena);
        var html = await (await esp.GetAsync("/mp/7")).Content.ReadAsStringAsync();
        Assert.Contains("MP-0000007", html);
        Assert.DoesNotContain(">Aprobar<", html);
    }

    [Fact(DisplayName = "W-06: un error de la API se muestra con su texto al volver a la página")]
    public async Task Error_De_La_Api_Se_Muestra()
    {
        var http = Cliente();
        await LoginAsync(http, "planificador", ApiFalsa.ClaveBuena);
        var detalle = await (await http.GetAsync("/mp/7")).Content.ReadAsStringAsync();

        var r = await http.PostAsync("/mp/7?handler=Aprobar",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = Token(detalle) }));
        Assert.Equal("/mp/7", r.Headers.Location!.ToString());

        var html = await (await http.GetAsync("/mp/7")).Content.ReadAsStringAsync();
        Assert.Contains("El MP-0000007 ya está aprobado.", html);
    }

    [Fact(DisplayName = "W-03: si la API rechaza el token, la web cierra la sesión y avisa que venció")]
    public async Task Token_Vencido_Vuelve_Al_Login()
    {
        var http = Cliente();
        await LoginAsync(http, "planificador", ApiFalsa.ClaveBuena);
        var r = await http.GetAsync("/mp/99");
        Assert.Equal(HttpStatusCode.Redirect, r.StatusCode);
        Assert.Equal("/login?vencida=1", r.Headers.Location!.ToString());
        Assert.Equal(HttpStatusCode.Redirect, (await http.GetAsync("/mp")).StatusCode);
    }

    [Fact(DisplayName = "W-02: una acción sin token antifalsificación se rechaza")]
    public async Task Post_Sin_Antiforgery_Se_Rechaza()
    {
        var http = Cliente();
        await LoginAsync(http, "planificador", ApiFalsa.ClaveBuena);
        var r = await http.PostAsync("/mp/7?handler=Aprobar", new FormUrlEncodedContent([]));
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact(DisplayName = "RF-10: el planning muestra el calendario del mes con las cantidades")]
    public async Task Planning_Muestra_Calendario()
    {
        var http = Cliente();
        await LoginAsync(http, "planificador", ApiFalsa.ClaveBuena);
        var r = await http.GetAsync("/planning?mes=2026-11");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var html = await r.Content.ReadAsStringAsync();
        Assert.Contains("Planning · Noviembre 2026", html);
        Assert.Contains("3 programados en el mes", html);
        Assert.Contains("<span class=\"cantidad\">3</span>", html);
        Assert.Contains(fabrica.Api.Llamadas, l => l.StartsWith("GET /api/v1/planning?desde=2026-11-01&hasta=2026-11-30"));
    }

    [Fact(DisplayName = "RF-10: el especialista no abre el planning y no se llama a la API")]
    public async Task Planning_Se_Cierra_Al_Especialista()
    {
        var http = Cliente();
        await LoginAsync(http, "especialista", ApiFalsa.ClaveBuena);
        Assert.Equal(HttpStatusCode.Redirect, (await http.GetAsync("/planning?mes=2026-11")).StatusCode);
        Assert.DoesNotContain(fabrica.Api.Llamadas, l => l.Contains("/api/v1/planning"));
    }

    [Fact(DisplayName = "RF-01: el planificador abre el formulario de alta con las listas; el especialista no")]
    public async Task Formulario_De_Alta_Segun_Perfil()
    {
        var plan = Cliente();
        await LoginAsync(plan, "planificador", ApiFalsa.ClaveBuena);
        var html = await (await plan.GetAsync("/mp/nuevo")).Content.ReadAsStringAsync();
        Assert.Contains("Nuevo mantenimiento preventivo", html);
        Assert.Contains("MAQ-001", html);
        Assert.Contains("Lubricación", html);

        var esp = Cliente();
        await LoginAsync(esp, "especialista", ApiFalsa.ClaveBuena);
        Assert.Equal(HttpStatusCode.Redirect, (await esp.GetAsync("/mp/nuevo")).StatusCode);
    }

    [Fact(DisplayName = "RF-12: exportar descarga el archivo que entrega la API")]
    public async Task Exportar_Descarga_El_Archivo()
    {
        var http = Cliente();
        await LoginAsync(http, "planificador", ApiFalsa.ClaveBuena);
        var r = await http.GetAsync("/ot?handler=Exportar&estado=40");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", r.Content.Headers.ContentType?.MediaType);
        Assert.Equal([0x50, 0x4B, 3, 4], await r.Content.ReadAsByteArrayAsync());
        Assert.Contains(fabrica.Api.Llamadas, l => l.StartsWith("GET /api/v1/ordenes-trabajo/exportacion") && l.Contains("estado=40"));
    }
}
