using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages.Mp;

public class IndexModel(ApiCliente api) : PaginaBase(api)
{
    [BindProperty(SupportsGet = true)] public string? Codigo { get; set; }
    [BindProperty(SupportsGet = true)] public string? Estado { get; set; }
    [BindProperty(SupportsGet = true, Name = "pagina")] public int NumeroPagina { get; set; } = 1;

    public Pagina<MpDto>? Resultado { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Resultado = await Api.ConsultarMpAsync(Codigo, Estado, Math.Max(1, NumeroPagina), ct);
        }
        catch (ApiException e)
        {
            ErrorCarga = e.Mensaje;
        }
    }

    /// <summary>Enlace a otra página conservando los filtros.</summary>
    public string Enlace(int pagina) =>
        $"/mp?pagina={pagina}&codigo={Uri.EscapeDataString(Codigo ?? "")}&estado={Uri.EscapeDataString(Estado ?? "")}";
}
