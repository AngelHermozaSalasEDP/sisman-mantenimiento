using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Api.Errores;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Application.Consultas;

namespace Sisman.Mantenimiento.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
[Authorize(Policy = Politicas.Consulta)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public class ConsultasController(ConsultasServicio servicio) : ControllerBase
{
    /// <summary>Catálogos para registrar o editar un MP: tipos de trabajo, fuentes, oportunidades y especialidades.</summary>
    [HttpGet("catalogos")]
    [ProducesResponseType<CatalogosDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogosDto>> Catalogos(CancellationToken ct) =>
        Ok(await servicio.CatalogosAsync(ct));

    /// <summary>Máquinas con su área, planta, sistemas y equipos.</summary>
    [HttpGet("maquinas")]
    [ProducesResponseType<IReadOnlyList<MaquinaResumen>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MaquinaResumen>>> Maquinas(CancellationToken ct) =>
        Ok(await servicio.MaquinasAsync(ct));

    /// <summary>
    /// Planning (RF-10, RN-43): cantidad de MP programados sin OT por día, entre dos fechas (hasta 92 días),
    /// con filtros opcionales de planta, área, máquina y especialidad.
    /// </summary>
    [HttpGet("planning")]
    [ProducesResponseType<PlanningDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<PlanningDto>> Planning(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta,
        [FromQuery] int? plantaId, [FromQuery] int? areaId, [FromQuery] int? maquinaId, [FromQuery] int? especialidadId,
        CancellationToken ct) =>
        Ok(await servicio.PlanningAsync(new FiltroPlanning(desde, hasta, plantaId, areaId, maquinaId, especialidadId), ct));
}
