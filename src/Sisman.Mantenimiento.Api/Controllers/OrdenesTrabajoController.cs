using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Api.Errores;
using Sisman.Mantenimiento.Application.OrdenesTrabajo;

namespace Sisman.Mantenimiento.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public class OrdenesTrabajoController(OtServicio servicio) : ControllerBase
{
    /// <summary>
    /// Genera una OT desde una fecha programada (RF-07). Nace Alta, Pendiente y Preventivo (RN-31); su número
    /// sale de un correlativo dentro de una transacción (RN-33, RN-34). Solo Planificador.
    /// </summary>
    [HttpPost("fechas/{fechaId:int}/ordenes-trabajo")]
    [Authorize(Policy = Politicas.Planificador)]
    [ProducesResponseType<OtDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<OtDto>> Generar(int fechaId, CancellationToken ct)
    {
        var ot = await servicio.GenerarAsync(fechaId, ct);
        return CreatedAtRoute(nameof(ObtenerOt), new { id = ot.Id }, ot);
    }

    /// <summary>Detalle de una OT con su historial de estados.</summary>
    [HttpGet("ordenes-trabajo/{id:int}", Name = nameof(ObtenerOt))]
    [Authorize(Policy = Politicas.Consulta)]
    [ProducesResponseType<OtDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<OtDto>> ObtenerOt(int id, CancellationToken ct) =>
        Ok(await servicio.ObtenerAsync(id, ct));
}
