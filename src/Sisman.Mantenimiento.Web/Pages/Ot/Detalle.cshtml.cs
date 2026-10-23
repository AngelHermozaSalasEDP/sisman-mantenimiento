using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages.Ot;

public class DetalleModel(ApiCliente api) : PaginaBase(api)
{
    public OtDto? Ot { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            Ot = await Api.ObtenerOtAsync(id, ct);
        }
        catch (ApiException e) when (e.Estado == 404)
        {
            return NotFound();
        }
        catch (ApiException e)
        {
            ErrorCarga = e.Mensaje;
        }
        return Page();
    }

    public Task<IActionResult> OnPostEstadoAsync(int id, int estado, CancellationToken ct) =>
        AccionAsync(async () =>
        {
            var ot = await Api.CambiarEstadoOtAsync(id, estado, ct);
            return $"La OT {ot.Numero} pasó a «{Textos.Estado(ot.Estado)}».";
        }, $"/ot/{id}");
}
