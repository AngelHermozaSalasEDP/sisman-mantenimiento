using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Api.Errores;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Application.MantenimientosPreventivos;
using Sisman.Mantenimiento.Application.OrdenesTrabajo;
using Sisman.Mantenimiento.Domain;

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

    /// <summary>
    /// Consulta OT con filtros y paginación en el servidor (RF-08). Orden por número descendente (RN-41);
    /// 50 por página por defecto, máximo 200 (RN-40). El estado es el código 37 a 42.
    /// </summary>
    [HttpGet("ordenes-trabajo")]
    [Authorize(Policy = Politicas.Consulta)]
    [ProducesResponseType<Pagina<OtDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<Pagina<OtDto>>> Consultar(
        [FromQuery] int? numero, [FromQuery] TipoOt? tipo, [FromQuery] int? estado,
        [FromQuery] DateOnly? fechaDesde, [FromQuery] DateOnly? fechaHasta,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 50, CancellationToken ct = default) =>
        Ok(await servicio.ConsultarAsync(
            new FiltroOt(numero, tipo, (EstadoOt?)estado, fechaDesde, fechaHasta, pagina, tamanoPagina), ct));

    /// <summary>Detalle de una OT con su historial de estados.</summary>
    [HttpGet("ordenes-trabajo/{id:int}", Name = nameof(ObtenerOt))]
    [Authorize(Policy = Politicas.Consulta)]
    [ProducesResponseType<OtDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<OtDto>> ObtenerOt(int id, CancellationToken ct) =>
        Ok(await servicio.ObtenerAsync(id, ct));

    /// <summary>
    /// Cambia el estado de una OT (RF-09). Solo las transiciones permitidas (RN-37); registra quién y cuándo (RN-38).
    /// Solo Planificador.
    /// </summary>
    [HttpPut("ordenes-trabajo/{id:int}/estado")]
    [Authorize(Policy = Politicas.Planificador)]
    [ProducesResponseType<OtDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<OtDto>> CambiarEstado(int id, CambioEstadoEntrada entrada, CancellationToken ct) =>
        Ok(await servicio.CambiarEstadoAsync(id, entrada, ct));
}
