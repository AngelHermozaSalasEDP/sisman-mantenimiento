using FluentValidation;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Application.MantenimientosPreventivos;
using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Application.OrdenesTrabajo;

public record HistorialDto(int EstadoAnterior, int EstadoNuevo, string Usuario, DateTime Fecha);

public record OtDto(
    int Id, int Numero, string Tipo, string Prioridad, int Estado, string Descripcion,
    int MaquinaId, int? SistemaId, int? EquipoId, int TipoTrabajoId, int FuenteId, int OportunidadId, int EspecialidadId,
    int MantenimientoPreventivoId, DateOnly FechaObjetivo, string CreadoPor, DateTime CreadoEn,
    string? ModificadoPor, DateTime? ModificadoEn,
    IReadOnlyList<HistorialDto> Historial)
{
    public static OtDto Desde(OrdenTrabajo o, bool conHistorial = true) => new(
        o.Id, o.Numero, o.Tipo.ToString(), o.Prioridad.ToString(), (int)o.Estado, o.Descripcion,
        o.MaquinaId, o.SistemaId, o.EquipoId, o.TipoTrabajoId, o.FuenteId, o.OportunidadId, o.EspecialidadId,
        o.MantenimientoPreventivoId, o.FechaObjetivo, o.CreadoPor, o.CreadoEn,
        o.ModificadoPor, o.ModificadoEn,
        conHistorial
            ? o.Historial.OrderBy(h => h.Fecha).ThenBy(h => h.Id)
                .Select(h => new HistorialDto((int)h.EstadoAnterior, (int)h.EstadoNuevo, h.Usuario, h.Fecha)).ToList()
            : []);
}

/// <summary>El estado viaja como el código del sistema original, 37 a 42 (RN-36).</summary>
public record CambioEstadoEntrada(int Estado);

public class CambioEstadoValidator : AbstractValidator<CambioEstadoEntrada>
{
    public CambioEstadoValidator()
    {
        RuleFor(x => x.Estado).InclusiveBetween(37, 42)
            .WithMessage("El estado debe ser 37 (Solicitud), 38 (Anulación), 39 (En programación), 40 (Pendiente), 41 (Concluido) o 42 (Concluido por anulación).");
    }
}

/// <summary>RN-40: 50 por página por defecto y 200 como máximo.</summary>
public class FiltroOtValidator : AbstractValidator<FiltroOt>
{
    public FiltroOtValidator()
    {
        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TamanoPagina).InclusiveBetween(1, 200).WithMessage("El tamaño de página debe estar entre 1 y 200.");
        RuleFor(x => x.Numero).GreaterThan(0).When(x => x.Numero is not null);
        RuleFor(x => x.Estado).IsInEnum().When(x => x.Estado is not null)
            .WithMessage("El estado debe ser un código entre 37 y 42.");
        RuleFor(x => x).Must(x => x.FechaDesde is null || x.FechaHasta is null || x.FechaDesde <= x.FechaHasta)
            .WithName("FechaDesde").WithMessage("La fecha desde no puede ser posterior a la fecha hasta.");
    }
}

/// <summary>RF-07 (generar), RF-08 (consultar) y RF-09 (cambiar estado) sobre la orden de trabajo.</summary>
public class OtServicio(
    IOtRepositorio repositorio,
    IReloj reloj,
    IUsuarioActual usuario,
    IValidator<FiltroOt> validaFiltro,
    IValidator<CambioEstadoEntrada> validaCambio)
{
    public const string CorrelativoOt = "OT";

    /// <summary>
    /// RN-30 a RN-35. Tomar el número, crear la OT, avanzar el correlativo y enlazar la fecha ocurre en una
    /// sola transacción: dos solicitudes a la vez reciben números distintos, y una falla no deja nada a medias.
    /// </summary>
    public Task<OtDto> GenerarAsync(int fechaId, CancellationToken ct) =>
        repositorio.EnTransaccionAsync(async t =>
        {
            var fecha = await repositorio.ObtenerFechaParaGenerarAsync(fechaId, t)
                ?? throw DominioException.NoEncontrado("fecha.no-encontrada", $"No existe la fecha programada {fechaId}.");
            var mp = fecha.MantenimientoPreventivo!;

            // Las reglas se comprueban antes de tomar el número, para no consumirlo en un rechazo.
            if (fecha.TieneOt)
                throw DominioException.Conflicto("fecha.con-ot", "La fecha ya tiene una orden de trabajo.");

            var numero = await repositorio.SiguienteNumeroAsync(CorrelativoOt, t);
            var ot = OrdenTrabajo.Generar(mp, fecha, numero, usuario.Nombre, reloj.UtcAhora);
            repositorio.Agregar(ot);
            await repositorio.GuardarAsync(t);
            return OtDto.Desde(ot);
        }, ct);

    public async Task<OtDto> ObtenerAsync(int id, CancellationToken ct) =>
        OtDto.Desde(await repositorio.ObtenerAsync(id, ct) ?? throw NoEncontrada(id));

    /// <summary>RF-08 (RN-40 a RN-42): consulta paginada en el servidor, por número descendente.</summary>
    public async Task<Pagina<OtDto>> ConsultarAsync(FiltroOt filtro, CancellationToken ct)
    {
        await validaFiltro.ValidateAndThrowAsync(filtro, ct);
        var (items, total) = await repositorio.ConsultarAsync(filtro, ct);
        return new Pagina<OtDto>(items.Select(o => OtDto.Desde(o, conHistorial: false)).ToList(), filtro.Pagina, filtro.TamanoPagina, total);
    }

    /// <summary>RF-09 (RN-37, RN-38): aplica una transición permitida y la registra en el historial.</summary>
    public async Task<OtDto> CambiarEstadoAsync(int id, CambioEstadoEntrada entrada, CancellationToken ct)
    {
        await validaCambio.ValidateAndThrowAsync(entrada, ct);
        var ot = await repositorio.ObtenerParaModificarAsync(id, ct) ?? throw NoEncontrada(id);
        ot.CambiarEstado((EstadoOt)entrada.Estado, usuario.Nombre, reloj.UtcAhora);
        await repositorio.GuardarAsync(ct);
        return OtDto.Desde(ot);
    }

    private static DominioException NoEncontrada(int id) =>
        DominioException.NoEncontrado("ot.no-encontrada", $"No existe la orden de trabajo {id}.");
}
