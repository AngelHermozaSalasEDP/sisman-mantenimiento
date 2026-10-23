using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Api.Errores;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Application.MantenimientosPreventivos;
using Sisman.Mantenimiento.Domain;

namespace Sisman.Mantenimiento.Api.Controllers;

[ApiController]
[Route("api/v1/mantenimientos-preventivos")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public class MantenimientosPreventivosController(MpServicio servicio) : ControllerBase
{
    /// <summary>Registra un MP en estado Nuevo con código correlativo MP-nnnnnnn (RF-01, RN-10 a RN-13). Solo Planificador.</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.Planificador)]
    [ProducesResponseType<MpDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public async Task<ActionResult<MpDto>> Crear(MpEntrada entrada, CancellationToken ct)
    {
        var mp = await servicio.CrearAsync(entrada, ct);
        return CreatedAtRoute(nameof(Obtener), new { id = mp.Id }, mp);
    }

    /// <summary>Elimina una fecha programada que todavía no tiene OT (RF-06, RN-25). Solo Planificador.</summary>
    [HttpDelete("{id:int}/fechas/{fechaId:int}")]
    [Authorize(Policy = Politicas.Planificador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> EliminarFecha(int id, int fechaId, CancellationToken ct)
    {
        await servicio.EliminarFechaAsync(id, fechaId, ct);
        return NoContent();
    }

    /// <summary>Consulta MP con filtros y paginación (RF-02). Por defecto 50 por página, máximo 200 (RN-40).</summary>
    [HttpGet]
    [Authorize(Policy = Politicas.Consulta)]
    [ProducesResponseType<Pagina<MpDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<Pagina<MpDto>>> Consultar(
        [FromQuery] string? codigo, [FromQuery] int? tipoTrabajoId, [FromQuery] EstadoMp? estado,
        [FromQuery] DateOnly? fechaDesde, [FromQuery] DateOnly? fechaHasta,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 50, CancellationToken ct = default) =>
        Ok(await servicio.ConsultarAsync(
            new FiltroMp(codigo, tipoTrabajoId, estado, fechaDesde, fechaHasta, pagina, tamanoPagina), ct));

    /// <summary>Detalle de un MP con sus fechas programadas (RF-02).</summary>
    [HttpGet("{id:int}", Name = nameof(Obtener))]
    [Authorize(Policy = Politicas.Consulta)]
    [ProducesResponseType<MpDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<MpDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await servicio.ObtenerAsync(id, ct));

    /// <summary>Edita un MP y lo deja en estado Modificado (RF-03, RN-14). Solo Planificador.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Planificador)]
    [ProducesResponseType<MpDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<MpDto>> Editar(int id, MpEntrada entrada, CancellationToken ct) =>
        Ok(await servicio.EditarAsync(id, entrada, ct));

    /// <summary>Aprueba un MP (RF-04, RN-15 y RN-16). Solo Planificador.</summary>
    [HttpPost("{id:int}/aprobacion")]
    [Authorize(Policy = Politicas.Planificador)]
    [ProducesResponseType<MpDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<MpDto>> Aprobar(int id, CancellationToken ct) =>
        Ok(await servicio.AprobarAsync(id, ct));

    /// <summary>Programa una fecha para un MP aprobado (RF-05, RN-20 a RN-24). Solo Planificador.</summary>
    [HttpPost("{id:int}/fechas")]
    [Authorize(Policy = Politicas.Planificador)]
    [ProducesResponseType<FechaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<FechaDto>> ProgramarFecha(int id, FechaEntrada entrada, CancellationToken ct)
    {
        var (mpId, fecha) = await servicio.ProgramarFechaAsync(id, entrada, ct);
        return CreatedAtRoute(nameof(Obtener), new { id = mpId }, fecha);
    }
}
