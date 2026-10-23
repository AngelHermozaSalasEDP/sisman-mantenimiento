using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sisman.Mantenimiento.Web.Pages;

/// <summary>La portada lleva a la lista de mantenimientos preventivos.</summary>
public class IndexModel : PageModel
{
    public IActionResult OnGet() => Redirect("/mp");
}
