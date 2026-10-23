using System.Security.Claims;
using Sisman.Mantenimiento.Application.Abstracciones;

namespace Sisman.Mantenimiento.Api.Errores;

/// <summary>Nombre del usuario del token, para registrar quién hizo cada cambio (RN-53).</summary>
public class UsuarioActual(IHttpContextAccessor accesor) : IUsuarioActual
{
    public string Nombre =>
        accesor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
        ?? throw new InvalidOperationException("No hay un usuario autenticado en la solicitud.");
}
