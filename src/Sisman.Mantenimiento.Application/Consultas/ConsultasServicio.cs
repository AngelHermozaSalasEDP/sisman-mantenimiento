using FluentValidation;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain;

namespace Sisman.Mantenimiento.Application.Consultas;

public record CatalogoDto(int Id, string Categoria, string Nombre);

public record CatalogosDto(
    IReadOnlyList<CatalogoDto> TiposTrabajo, IReadOnlyList<CatalogoDto> Fuentes,
    IReadOnlyList<CatalogoDto> Oportunidades, IReadOnlyList<CatalogoDto> Especialidades);

public record DiaPlanningDto(DateOnly Dia, int Cantidad);

public record PlanningDto(DateOnly Desde, DateOnly Hasta, int Total, IReadOnlyList<DiaPlanningDto> Dias);

/// <summary>Un mes y algo: el planning es una vista de calendario, no un reporte de un año entero.</summary>
public class FiltroPlanningValidator : AbstractValidator<FiltroPlanning>
{
    public const int MaximoDias = 92;

    public FiltroPlanningValidator()
    {
        RuleFor(x => x.Desde).NotEqual(default(DateOnly)).WithMessage("La fecha desde es obligatoria.");
        RuleFor(x => x.Hasta).NotEqual(default(DateOnly)).WithMessage("La fecha hasta es obligatoria.");
        RuleFor(x => x).Must(x => x.Desde <= x.Hasta)
            .WithName("Desde").WithMessage("La fecha desde no puede ser posterior a la fecha hasta.");
        RuleFor(x => x).Must(x => x.Hasta.DayNumber - x.Desde.DayNumber < MaximoDias)
            .When(x => x.Desde <= x.Hasta)
            .WithName("Hasta").WithMessage($"El rango del planning admite hasta {MaximoDias} días.");
    }
}

/// <summary>Catálogos para los formularios (RF-01, RF-03) y planning (RF-10).</summary>
public class ConsultasServicio(ICatalogoRepositorio catalogos, IPlanningRepositorio planning, IValidator<FiltroPlanning> validaPlanning)
{
    public async Task<CatalogosDto> CatalogosAsync(CancellationToken ct)
    {
        var todos = await catalogos.CatalogosAsync(ct);
        IReadOnlyList<CatalogoDto> De(CategoriaCatalogo c) => todos
            .Where(x => x.Categoria == c).OrderBy(x => x.Nombre)
            .Select(x => new CatalogoDto(x.Id, x.Categoria.ToString(), x.Nombre)).ToList();
        return new CatalogosDto(
            De(CategoriaCatalogo.TipoTrabajo), De(CategoriaCatalogo.FuenteInformacion),
            De(CategoriaCatalogo.OportunidadIntervencion), De(CategoriaCatalogo.Especialidad));
    }

    public Task<IReadOnlyList<MaquinaResumen>> MaquinasAsync(CancellationToken ct) => catalogos.MaquinasAsync(ct);

    /// <summary>RF-10 (RN-43): MP programados sin OT por día, con los días vacíos en cero.</summary>
    public async Task<PlanningDto> PlanningAsync(FiltroPlanning filtro, CancellationToken ct)
    {
        await validaPlanning.ValidateAndThrowAsync(filtro, ct);
        var conteo = (await planning.ContarAsync(filtro, ct)).ToDictionary(x => x.Dia, x => x.Cantidad);
        var dias = Enumerable.Range(0, filtro.Hasta.DayNumber - filtro.Desde.DayNumber + 1)
            .Select(i => filtro.Desde.AddDays(i))
            .Select(d => new DiaPlanningDto(d, conteo.GetValueOrDefault(d)))
            .ToList();
        return new PlanningDto(filtro.Desde, filtro.Hasta, dias.Sum(d => d.Cantidad), dias);
    }
}
