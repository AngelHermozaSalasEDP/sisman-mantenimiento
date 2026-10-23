using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages;

/// <summary>Comportamiento común: perfil del usuario y acciones que muestran el error de la API en vez de fallar.</summary>
public abstract class PaginaBase(ApiCliente api) : PageModel
{
    protected ApiCliente Api { get; } = api;

    /// <summary>Solo el planificador ve las acciones de escritura (RN-03); la API igual las rechaza con 403.</summary>
    public bool EsPlanificador => User.IsInRole("Planificador");

    /// <summary>Mensaje de error de la carga de la página, si la API lo rechazó.</summary>
    public string? ErrorCarga { get; protected set; }

    /// <summary>Ejecuta una acción de escritura; el resultado (éxito o error de la API) se muestra al volver.</summary>
    protected async Task<IActionResult> AccionAsync(Func<Task<string>> accion, string volverA)
    {
        try
        {
            TempData["Exito"] = await accion();
        }
        catch (ApiException e)
        {
            TempData["Error"] = e.Mensaje;
        }
        return Redirect(volverA);
    }
}
