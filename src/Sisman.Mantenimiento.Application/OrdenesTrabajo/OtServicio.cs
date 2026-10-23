using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Application.OrdenesTrabajo;

public record HistorialDto(int EstadoAnterior, int EstadoNuevo, string Usuario, DateTime Fecha);

public record OtDto(
    int Id, int Numero, string Tipo, string Prioridad, int Estado, string Descripcion,
    int MaquinaId, int? SistemaId, int? EquipoId, int TipoTrabajoId, int FuenteId, int OportunidadId, int EspecialidadId,
    int MantenimientoPreventivoId, DateOnly FechaObjetivo, string CreadoPor, DateTime CreadoEn,
    IReadOnlyList<HistorialDto> Historial)
{
    public static OtDto Desde(OrdenTrabajo o) => new(
        o.Id, o.Numero, o.Tipo.ToString(), o.Prioridad.ToString(), (int)o.Estado, o.Descripcion,
        o.MaquinaId, o.SistemaId, o.EquipoId, o.TipoTrabajoId, o.FuenteId, o.OportunidadId, o.EspecialidadId,
        o.MantenimientoPreventivoId, o.FechaObjetivo, o.CreadoPor, o.CreadoEn,
        o.Historial.OrderBy(h => h.Fecha).Select(h => new HistorialDto((int)h.EstadoAnterior, (int)h.EstadoNuevo, h.Usuario, h.Fecha)).ToList());
}

/// <summary>RF-07: generar una OT desde una fecha programada; detalle de una OT.</summary>
public class OtServicio(IOtRepositorio repositorio, IReloj reloj, IUsuarioActual usuario)
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
        OtDto.Desde(await repositorio.ObtenerAsync(id, ct)
            ?? throw DominioException.NoEncontrado("ot.no-encontrada", $"No existe la orden de trabajo {id}."));
}
