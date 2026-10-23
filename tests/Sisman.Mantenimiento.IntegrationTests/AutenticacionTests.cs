using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sisman.Mantenimiento.IntegrationTests;

public class AutenticacionTests(ApiFactory fabrica) : IClassFixture<ApiFactory>
{
    private const string Ruta = "/api/v1/auth/login";

    private static async Task<JsonElement> Json(HttpResponseMessage r) =>
        JsonSerializer.Deserialize<JsonElement>(await r.Content.ReadAsStringAsync());

    [Fact(DisplayName = "RF-11: login correcto devuelve token y perfil")]
    public async Task Login_Correcto_Devuelve_Token()
    {
        var r = await fabrica.CreateClient().PostAsJsonAsync(Ruta, new { usuario = "planificador", clave = ApiFactory.ClavePlanificador });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Json(r);
        Assert.False(string.IsNullOrEmpty(json.GetProperty("token").GetString()));
        Assert.Equal("Planificador", json.GetProperty("perfil").GetString());
        Assert.True(r.Headers.Contains("X-Correlation-Id"));
    }

    [Fact(DisplayName = "RN-02: el token vence a los 30 minutos")]
    public async Task Token_Vence_A_Los_30_Minutos()
    {
        var antes = DateTime.UtcNow;
        var r = await fabrica.CreateClient().PostAsJsonAsync(Ruta, new { usuario = "especialista", clave = ApiFactory.ClaveEspecialista });
        var vence = (await Json(r)).GetProperty("venceEn").GetDateTime().ToUniversalTime();
        Assert.InRange(vence - antes, TimeSpan.FromMinutes(29), TimeSpan.FromMinutes(31));
    }

    [Theory(DisplayName = "RN-50: credenciales inválidas dan 401 ProblemDetails sin revelar el motivo")]
    [InlineData("planificador", "incorrecta")]
    [InlineData("no-existe", "cualquiera")]
    public async Task Credenciales_Invalidas_Dan_401(string usuario, string clave)
    {
        var r = await fabrica.CreateClient().PostAsJsonAsync(Ruta, new { usuario, clave });
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        Assert.Equal("application/problem+json", r.Content.Headers.ContentType?.MediaType);
        var json = await Json(r);
        Assert.Equal("auth.credenciales-invalidas", json.GetProperty("codigo").GetString());
        Assert.Equal("Usuario o clave incorrectos.", json.GetProperty("detail").GetString());
    }

    [Fact(DisplayName = "RNF-04: entrada vacía da 400 con los campos con error")]
    public async Task Entrada_Vacia_Da_400()
    {
        var r = await fabrica.CreateClient().PostAsJsonAsync(Ruta, new { usuario = "", clave = "" });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        var json = await Json(r);
        Assert.Equal("validacion.error", json.GetProperty("codigo").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty("Usuario", out _));
    }

    [Theory(DisplayName = "RN-01: sin token toda ruta protegida da 401")]
    [InlineData("/api/v1/ordenes-trabajo")]
    [InlineData("/api/v1/mantenimientos-preventivos")]
    public async Task Sin_Token_Da_401(string ruta)
    {
        var r = await fabrica.CreateClient().GetAsync(ruta);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact(DisplayName = "RN-54: se respeta el X-Correlation-Id del cliente")]
    public async Task Respeta_Correlacion_Del_Cliente()
    {
        var cliente = fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Correlation-Id", "abc-123");
        var r = await cliente.PostAsJsonAsync(Ruta, new { usuario = "x", clave = "y" });
        Assert.Equal("abc-123", r.Headers.GetValues("X-Correlation-Id").Single());
        Assert.Equal("abc-123", (await Json(r)).GetProperty("correlationId").GetString());
    }
}
