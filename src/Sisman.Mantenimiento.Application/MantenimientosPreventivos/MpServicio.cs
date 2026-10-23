using FluentValidation;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Application.MantenimientosPreventivos;

/// <summary>Casos de uso RF-01 a RF-06 sobre el mantenimiento preventivo.</summary>
public class MpServicio(
    IMpRepositorio repositorio,
    ITransacciones transacciones,
    IReloj reloj,
    IUsuarioActual usuario,
    IValidator<MpEntrada> validaEntrada,
    IValidator<FechaEntrada> validaFecha,
    IValidator<FiltroMp> validaFiltro)
{
    public const string CorrelativoMp = "MP";

    /// <summary>
    /// RF-01 (RN-10 a RN-13): registra un MP en estado Nuevo. El código MP-nnnnnnn sale del correlativo
    /// dentro de una transacción, igual que el número de OT.
    /// </summary>
    public async Task<MpDto> CrearAsync(MpEntrada entrada, CancellationToken ct)
    {
        await validaEntrada.ValidateAndThrowAsync(entrada, ct);
        var datos = entrada.ADatos();
        await repositorio.ValidarReferenciasAsync(datos, ct);

        return await transacciones.EnTransaccionAsync(async t =>
        {
            var numero = await transacciones.SiguienteNumeroAsync(CorrelativoMp, t);
            var mp = MantenimientoPreventivo.Crear(numero, datos, usuario.Nombre, reloj.UtcAhora);
            repositorio.Agregar(mp);
            await repositorio.GuardarAsync(t);
            return MpDto.Desde(mp, conFechas: true);
        }, ct);
    }

    /// <summary>RF-02: consulta con filtros y paginación en el servidor.</summary>
    public async Task<Pagina<MpDto>> ConsultarAsync(FiltroMp filtro, CancellationToken ct)
    {
        await validaFiltro.ValidateAndThrowAsync(filtro, ct);
        var (items, total) = await repositorio.ConsultarAsync(filtro, ct);
        return new Pagina<MpDto>(items.Select(m => MpDto.Desde(m, conFechas: false)).ToList(), filtro.Pagina, filtro.TamanoPagina, total);
    }

    /// <summary>RF-02: detalle con sus fechas programadas.</summary>
    public async Task<MpDto> ObtenerAsync(int id, CancellationToken ct) =>
        MpDto.Desde(await CargarAsync(id, ct), conFechas: true);

    /// <summary>RF-03 (RN-14): edita el MP y lo deja en Modificado.</summary>
    public async Task<MpDto> EditarAsync(int id, MpEntrada entrada, CancellationToken ct)
    {
        await validaEntrada.ValidateAndThrowAsync(entrada, ct);
        var mp = await CargarAsync(id, ct);
        var datos = entrada.ADatos();
        await repositorio.ValidarReferenciasAsync(datos, ct);

        mp.Modificar(datos, usuario.Nombre, reloj.UtcAhora);
        await repositorio.GuardarAsync(ct);
        return MpDto.Desde(mp, conFechas: true);
    }

    /// <summary>RF-04 (RN-15, RN-16).</summary>
    public async Task<MpDto> AprobarAsync(int id, CancellationToken ct)
    {
        var mp = await CargarAsync(id, ct);
        mp.Aprobar(usuario.Nombre, reloj.UtcAhora);
        await repositorio.GuardarAsync(ct);
        return MpDto.Desde(mp, conFechas: true);
    }

    /// <summary>RF-05 (RN-20 a RN-24).</summary>
    public async Task<(int MpId, FechaDto Fecha)> ProgramarFechaAsync(int id, FechaEntrada entrada, CancellationToken ct)
    {
        await validaFecha.ValidateAndThrowAsync(entrada, ct);
        var mp = await CargarAsync(id, ct);
        var fecha = mp.ProgramarFecha(entrada.FechaInicio, entrada.FechaFin, reloj.Hoy, usuario.Nombre, reloj.UtcAhora);
        await repositorio.GuardarAsync(ct);
        return (mp.Id, FechaDto.Desde(fecha));
    }

    /// <summary>RF-06 (RN-25): elimina una fecha que todavía no tiene OT.</summary>
    public async Task EliminarFechaAsync(int id, int fechaId, CancellationToken ct)
    {
        var mp = await CargarAsync(id, ct);
        var fecha = mp.QuitarFecha(fechaId);
        repositorio.EliminarFecha(fecha);
        await repositorio.GuardarAsync(ct);
    }

    private async Task<MantenimientoPreventivo> CargarAsync(int id, CancellationToken ct) =>
        await repositorio.ObtenerConFechasAsync(id, ct)
            ?? throw DominioException.NoEncontrado("mp.no-encontrado", $"No existe el mantenimiento preventivo {id}.");
}
