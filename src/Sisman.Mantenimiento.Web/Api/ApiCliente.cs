using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sisman.Mantenimiento.Web.Api;

/// <summary>Error que la API devolvió como ProblemDetails; <see cref="Detalle"/> es el texto para el usuario.</summary>
public class ApiException(int estado, string codigo, string detalle, IReadOnlyDictionary<string, string[]>? errores)
    : Exception(detalle)
{
    public int Estado { get; } = estado;
    public string Codigo { get; } = codigo;
    public string Detalle { get; } = detalle;
    public IReadOnlyDictionary<string, string[]> Errores { get; } = errores ?? new Dictionary<string, string[]>();

    /// <summary>Texto completo: el detalle y, si hay, los mensajes por campo.</summary>
    public string Mensaje => Errores.Count == 0 ? Detalle : $"{Detalle} {string.Join(" ", Errores.SelectMany(e => e.Value))}";
}

/// <summary>El token venció o fue rechazado: la web cierra la sesión y vuelve al login.</summary>
public class SesionVencidaException() : Exception("La sesión venció.");

/// <summary>Agrega el token de la sesión a cada llamada.</summary>
public class TokenHandler(IHttpContextAccessor accesor) : DelegatingHandler
{
    public const string ClaimToken = "api_token";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = accesor.HttpContext?.User.FindFirst(ClaimToken)?.Value;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return base.SendAsync(request, ct);
    }
}

/// <summary>Cliente tipado de la API REST de SISMAN Mantenimiento (/api/v1).</summary>
public class ApiCliente(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<LoginRespuesta> IniciarSesionAsync(string usuario, string clave, CancellationToken ct) =>
        EnviarAsync<LoginRespuesta>(HttpMethod.Post, "api/v1/auth/login", new { usuario, clave }, ct, esLogin: true);

    public Task<Pagina<MpDto>> ConsultarMpAsync(string? codigo, string? estado, int pagina, CancellationToken ct) =>
        EnviarAsync<Pagina<MpDto>>(HttpMethod.Get,
            $"api/v1/mantenimientos-preventivos?pagina={pagina}" + Param("codigo", codigo) + Param("estado", estado), null, ct);

    public Task<MpDto> ObtenerMpAsync(int id, CancellationToken ct) =>
        EnviarAsync<MpDto>(HttpMethod.Get, $"api/v1/mantenimientos-preventivos/{id}", null, ct);

    public Task<MpDto> AprobarMpAsync(int id, CancellationToken ct) =>
        EnviarAsync<MpDto>(HttpMethod.Post, $"api/v1/mantenimientos-preventivos/{id}/aprobacion", null, ct);

    public Task<FechaDto> ProgramarFechaAsync(int mpId, DateOnly inicio, DateOnly fin, CancellationToken ct) =>
        EnviarAsync<FechaDto>(HttpMethod.Post, $"api/v1/mantenimientos-preventivos/{mpId}/fechas",
            new { fechaInicio = inicio, fechaFin = fin }, ct);

    public Task<OtDto> GenerarOtAsync(int fechaId, CancellationToken ct) =>
        EnviarAsync<OtDto>(HttpMethod.Post, $"api/v1/fechas/{fechaId}/ordenes-trabajo", null, ct);

    public Task<Pagina<OtDto>> ConsultarOtAsync(int? numero, int? estado, int pagina, CancellationToken ct) =>
        EnviarAsync<Pagina<OtDto>>(HttpMethod.Get,
            $"api/v1/ordenes-trabajo?pagina={pagina}" + Param("numero", numero?.ToString()) + Param("estado", estado?.ToString()), null, ct);

    public Task<OtDto> ObtenerOtAsync(int id, CancellationToken ct) =>
        EnviarAsync<OtDto>(HttpMethod.Get, $"api/v1/ordenes-trabajo/{id}", null, ct);

    public Task<OtDto> CambiarEstadoOtAsync(int id, int estado, CancellationToken ct) =>
        EnviarAsync<OtDto>(HttpMethod.Put, $"api/v1/ordenes-trabajo/{id}/estado", new { estado }, ct);

    private static string Param(string nombre, string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? "" : $"&{nombre}={Uri.EscapeDataString(valor.Trim())}";

    private async Task<T> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo, CancellationToken ct, bool esLogin = false)
    {
        using var solicitud = new HttpRequestMessage(metodo, ruta);
        if (cuerpo is not null) solicitud.Content = JsonContent.Create(cuerpo, options: Json);

        using var respuesta = await http.SendAsync(solicitud, ct);
        if (respuesta.IsSuccessStatusCode)
            return (await respuesta.Content.ReadFromJsonAsync<T>(Json, ct))!;

        // En el login, el 401 significa credenciales inválidas; en lo demás, token vencido.
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized && !esLogin)
            throw new SesionVencidaException();

        throw await LeerProblemaAsync(respuesta, ct);
    }

    private static async Task<ApiException> LeerProblemaAsync(HttpResponseMessage r, CancellationToken ct)
    {
        var estado = (int)r.StatusCode;
        try
        {
            var p = await r.Content.ReadFromJsonAsync<JsonElement>(Json, ct);
            string Texto(string nombre) => p.TryGetProperty(nombre, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : "";
            Dictionary<string, string[]>? errores = null;
            if (p.TryGetProperty("errors", out var e) && e.ValueKind == JsonValueKind.Object)
                errores = e.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.EnumerateArray().Select(m => m.GetString() ?? "").ToArray());
            var detalle = Texto("detail");
            if (detalle == "") detalle = estado == 403 ? "Su perfil no permite esta operación." : "La operación no se pudo completar.";
            return new ApiException(estado, Texto("codigo"), detalle, errores);
        }
        catch (JsonException)
        {
            return new ApiException(estado, "", estado == 403 ? "Su perfil no permite esta operación." : "La operación no se pudo completar.", null);
        }
    }
}
