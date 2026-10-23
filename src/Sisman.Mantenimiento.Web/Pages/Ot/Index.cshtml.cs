using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages.Ot;

public class IndexModel(ApiCliente api) : PaginaBase(api)
{
    [BindProperty(SupportsGet = true)] public int? Numero { get; set; }
    [BindProperty(SupportsGet = true)] public int? Estado { get; set; }
    [BindProperty(SupportsGet = true, Name = "pagina")] public int NumeroPagina { get; set; } = 1;

    public Pagina<OtDto>? Resultado { get; private set; }

    public static readonly int[] Estados = [37, 38, 39, 40, 41, 42];

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Resultado = await Api.ConsultarOtAsync(Numero, Estado, Math.Max(1, NumeroPagina), ct);
        }
        catch (ApiException e)
        {
            ErrorCarga = e.Mensaje;
        }
    }

    /// <summary>RF-12: descarga el Excel con los filtros de la pantalla.</summary>
    public async Task<IActionResult> OnGetExportarAsync(CancellationToken ct)
    {
        try
        {
            var archivo = await Api.ExportarOtAsync(Numero, Estado, ct);
            return File(archivo.Contenido, archivo.Tipo, archivo.Nombre);
        }
        catch (ApiException e)
        {
            TempData["Error"] = e.Mensaje;
            return Redirect(Enlace(1));
        }
    }

    public string Enlace(int pagina) => $"/ot?pagina={pagina}&numero={Numero}&estado={Estado}";
}
