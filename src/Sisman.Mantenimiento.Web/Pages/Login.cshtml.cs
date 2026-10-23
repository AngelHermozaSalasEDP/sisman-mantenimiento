using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages;

public class LoginModel(ApiCliente api) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Ingrese el usuario.")]
    public string Usuario { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Ingrese la clave."), DataType(DataType.Password)]
    public string Clave { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? Error { get; private set; }
    public bool Vencida { get; private set; }

    public void OnGet(bool vencida = false) => Vencida = vencida;

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var r = await api.IniciarSesionAsync(Usuario.Trim(), Clave, ct);
            var identidad = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, Usuario.Trim()),
                new Claim(ClaimTypes.Role, r.Perfil),
                new Claim(TokenHandler.ClaimToken, r.Token)
            ], CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad),
                new AuthenticationProperties { ExpiresUtc = r.VenceEn, IsPersistent = false });

            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/mp");
        }
        catch (ApiException e)
        {
            Error = e.Estado == 401 ? "Usuario o clave incorrectos." : e.Mensaje;
            return Page();
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con la API. Verifique que esté en ejecución.";
            return Page();
        }
    }
}
