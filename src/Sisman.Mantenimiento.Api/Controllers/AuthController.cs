using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Application.Autenticacion;

namespace Sisman.Mantenimiento.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController(AutenticacionServicio servicio) : ControllerBase
{
    /// <summary>Inicia sesión (RF-11). El token vence a los 30 minutos (RN-02).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<ActionResult<LoginRespuesta>> Login(LoginSolicitud solicitud, CancellationToken ct) =>
        Ok(await servicio.IniciarSesionAsync(solicitud, ct));
}
