using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages.Mp;

public class DetalleModel(ApiCliente api) : PaginaBase(api)
{
    public MpDto? Mp { get; private set; }

    [BindProperty] public DateOnly? Inicio { get; set; }
    [BindProperty] public DateOnly? Fin { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            Mp = await Api.ObtenerMpAsync(id, ct);
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

    public Task<IActionResult> OnPostAprobarAsync(int id, CancellationToken ct) =>
        AccionAsync(async () =>
        {
            var mp = await Api.AprobarMpAsync(id, ct);
            return $"El {mp.Codigo} quedó aprobado.";
        }, $"/mp/{id}");

    public Task<IActionResult> OnPostProgramarAsync(int id, CancellationToken ct)
    {
        if (Inicio is null || Fin is null)
        {
            TempData["Error"] = "Indique la fecha de inicio y la de fin.";
            return Task.FromResult<IActionResult>(Redirect($"/mp/{id}"));
        }
        return AccionAsync(async () =>
        {
            var f = await Api.ProgramarFechaAsync(id, Inicio.Value, Fin.Value, ct);
            return $"Fecha programada: {Textos.Fecha(f.FechaInicio)} al {Textos.Fecha(f.FechaFin)}.";
        }, $"/mp/{id}");
    }

    public Task<IActionResult> OnPostEliminarFechaAsync(int id, int fechaId, CancellationToken ct) =>
        AccionAsync(async () =>
        {
            await Api.EliminarFechaAsync(id, fechaId, ct);
            return "Se eliminó la fecha programada.";
        }, $"/mp/{id}");

    public async Task<IActionResult> OnPostGenerarAsync(int id, int fechaId, CancellationToken ct)
    {
        try
        {
            var ot = await Api.GenerarOtAsync(fechaId, ct);
            TempData["Exito"] = $"Se generó la OT {ot.Numero}.";
            return Redirect($"/ot/{ot.Id}");
        }
        catch (ApiException e)
        {
            TempData["Error"] = e.Mensaje;
            return Redirect($"/mp/{id}");
        }
    }
}
