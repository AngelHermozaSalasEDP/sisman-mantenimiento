using FluentValidation;

namespace Sisman.Mantenimiento.Application.MantenimientosPreventivos;

/// <summary>RN-11, RN-12 y RN-13: forma de la entrada (ADR-004).</summary>
public class MpEntradaValidator : AbstractValidator<MpEntrada>
{
    public MpEntradaValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaquinaId).GreaterThan(0);
        RuleFor(x => x.SistemaId).GreaterThan(0).When(x => x.SistemaId is not null);
        RuleFor(x => x.EquipoId).GreaterThan(0).When(x => x.EquipoId is not null);
        RuleFor(x => x.TipoTrabajoId).GreaterThan(0);
        RuleFor(x => x.FuenteId).GreaterThan(0);
        RuleFor(x => x.OportunidadId).GreaterThan(0);
        RuleFor(x => x.EspecialidadId).GreaterThan(0);
        RuleFor(x => x.Frecuencia).InclusiveBetween(1, 4).WithMessage("La frecuencia debe ser 1 (diario), 2 (semanal), 3 (mensual) o 4 (anual).");
        RuleFor(x => x.ValorFrecuencia).GreaterThan(0);
        RuleFor(x => x.Mes).NotNull().InclusiveBetween((byte)1, (byte)12)
            .When(x => x.Frecuencia is 3 or 4)
            .WithMessage("La frecuencia mensual o anual exige un mes entre 1 y 12.");
    }
}

/// <summary>RN-21; la regla «posterior a hoy» (RN-20) y las de estado las aplica el dominio con el reloj inyectado.</summary>
public class FechaEntradaValidator : AbstractValidator<FechaEntrada>
{
    public FechaEntradaValidator()
    {
        RuleFor(x => x.FechaInicio).NotEqual(default(DateOnly)).WithMessage("La fecha de inicio es obligatoria.");
        RuleFor(x => x.FechaFin).NotEqual(default(DateOnly)).WithMessage("La fecha de fin es obligatoria.");
    }
}

/// <summary>RN-40: paginación en el servidor, 50 por defecto y 200 como máximo.</summary>
public class FiltroMpValidator : AbstractValidator<Abstracciones.FiltroMp>
{
    public FiltroMpValidator()
    {
        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TamanoPagina).InclusiveBetween(1, 200).WithMessage("El tamaño de página debe estar entre 1 y 200.");
        RuleFor(x => x.Codigo).MaximumLength(10);
        RuleFor(x => x).Must(x => x.FechaDesde is null || x.FechaHasta is null || x.FechaDesde <= x.FechaHasta)
            .WithName("FechaDesde").WithMessage("La fecha desde no puede ser posterior a la fecha hasta.");
    }
}
