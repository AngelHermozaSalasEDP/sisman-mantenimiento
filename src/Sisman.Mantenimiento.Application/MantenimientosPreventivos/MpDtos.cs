using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;

namespace Sisman.Mantenimiento.Application.MantenimientosPreventivos;

/// <summary>Datos que el planificador envía para editar un MP (RF-03). La frecuencia viaja como código 1 a 4 (RN-12).</summary>
public record MpEntrada(
    string Descripcion, int MaquinaId, int? SistemaId, int? EquipoId,
    int TipoTrabajoId, int FuenteId, int OportunidadId, int EspecialidadId,
    int Frecuencia, int ValorFrecuencia, byte? Mes)
{
    public DatosMp ADatos() => new(
        Descripcion, MaquinaId, SistemaId, EquipoId, TipoTrabajoId, FuenteId, OportunidadId, EspecialidadId,
        (Frecuencia)Frecuencia, ValorFrecuencia, Mes);
}

public record FechaEntrada(DateOnly FechaInicio, DateOnly FechaFin);

public record FechaDto(int Id, DateOnly FechaInicio, DateOnly FechaFin, int? NumeroOt)
{
    public static FechaDto Desde(FechaProgramada f) => new(f.Id, f.FechaInicio, f.FechaFin, f.OrdenTrabajo?.Numero);
}

public record MpDto(
    int Id, string Codigo, string Estado, string Descripcion, int MaquinaId, int? SistemaId, int? EquipoId,
    int TipoTrabajoId, int FuenteId, int OportunidadId, int EspecialidadId,
    int Frecuencia, int ValorFrecuencia, byte? Mes,
    DateOnly? FechaInicio, DateOnly? FechaProxima, DateOnly? FechaUltima,
    string? ModificadoPor, DateTime? ModificadoEn, string? AprobadoPor, DateTime? AprobadoEn,
    IReadOnlyList<FechaDto> Fechas)
{
    public static MpDto Desde(MantenimientoPreventivo m, bool conFechas) => new(
        m.Id, m.Codigo, m.Estado.ToString(), m.Descripcion, m.MaquinaId, m.SistemaId, m.EquipoId,
        m.TipoTrabajoId, m.FuenteId, m.OportunidadId, m.EspecialidadId,
        (int)m.Frecuencia, m.ValorFrecuencia, m.Mes,
        m.FechaInicio, m.FechaProxima, m.FechaUltima,
        m.ModificadoPor, m.ModificadoEn, m.AprobadoPor, m.AprobadoEn,
        conFechas ? m.Fechas.OrderBy(f => f.FechaInicio).Select(FechaDto.Desde).ToList() : []);
}

public record Pagina<T>(IReadOnlyList<T> Items, int PaginaActual, int TamanoPagina, int Total);
