using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;

namespace Sisman.Mantenimiento.Application.Abstracciones;

/// <summary>Reloj inyectable: permite probar «posterior a hoy» (RN-20) sin esperar al día siguiente.</summary>
public interface IReloj
{
    DateTime UtcAhora { get; }
    DateOnly Hoy { get; }
}

public interface IUsuarioRepositorio
{
    Task<Usuario?> BuscarPorNombreAsync(string nombreUsuario, CancellationToken ct);
}

public interface IHasheadorClave
{
    bool Verificar(Usuario usuario, string clave);
}

public record TokenEmitido(string Token, DateTime VenceEn);

public interface IGeneradorToken
{
    TokenEmitido Generar(Usuario usuario);
}

/// <summary>Usuario autenticado de la solicitud actual (nombre para la auditoría, RN-53).</summary>
public interface IUsuarioActual
{
    string Nombre { get; }
}

/// <summary>Transacciones y correlativos compartidos por los casos de uso que numeran (MP y OT).</summary>
public interface ITransacciones
{
    /// <summary>
    /// Ejecuta la operación en una transacción. Si algo falla, se deshace todo, incluido el avance
    /// del correlativo (RN-33). Si la base pide reintentar, se repite la operación completa.
    /// </summary>
    Task<T> EnTransaccionAsync<T>(Func<CancellationToken, Task<T>> operacion, CancellationToken ct);

    /// <summary>Avanza el correlativo y devuelve el nuevo valor; la fila queda bloqueada hasta el fin de la transacción (RN-34).</summary>
    Task<int> SiguienteNumeroAsync(string correlativo, CancellationToken ct);
}

public record FiltroMp(
    string? Codigo, int? TipoTrabajoId, EstadoMp? Estado, DateOnly? FechaDesde, DateOnly? FechaHasta,
    int Pagina, int TamanoPagina);

public interface IMpRepositorio
{
    /// <summary>Devuelve el MP con sus fechas (y el número de OT de cada una), listo para modificarlo.</summary>
    Task<MantenimientoPreventivo?> ObtenerConFechasAsync(int id, CancellationToken ct);

    Task<(IReadOnlyList<MantenimientoPreventivo> Items, int Total)> ConsultarAsync(FiltroMp filtro, CancellationToken ct);

    /// <summary>Falla con 400 si una máquina, sistema, equipo o catálogo indicado no existe (RN-11).</summary>
    Task ValidarReferenciasAsync(DatosMp datos, CancellationToken ct);

    void Agregar(MantenimientoPreventivo mp);

    void EliminarFecha(FechaProgramada fecha);

    /// <summary>Guarda los cambios; una edición concurrente o un duplicado se informan como 409.</summary>
    Task GuardarAsync(CancellationToken ct);
}

public record FiltroOt(
    int? Numero, TipoOt? Tipo, EstadoOt? Estado, DateOnly? FechaDesde, DateOnly? FechaHasta,
    int Pagina, int TamanoPagina);

public interface IOtRepositorio
{
    /// <summary>Lee la fecha con su MP y su OT, bloqueando la fila hasta el fin de la transacción.</summary>
    Task<FechaProgramada?> ObtenerFechaParaGenerarAsync(int fechaId, CancellationToken ct);

    void Agregar(OrdenTrabajo ot);

    Task<OrdenTrabajo?> ObtenerAsync(int id, CancellationToken ct);

    /// <summary>Devuelve la OT con su historial, lista para modificarla (RF-09).</summary>
    Task<OrdenTrabajo?> ObtenerParaModificarAsync(int id, CancellationToken ct);

    Task<(IReadOnlyList<OrdenTrabajo> Items, int Total)> ConsultarAsync(FiltroOt filtro, CancellationToken ct);

    /// <summary>RF-12: todas las OT del filtro, sin paginar, hasta <paramref name="maximo"/> filas.</summary>
    Task<IReadOnlyList<OrdenTrabajo>> ConsultarParaExportarAsync(FiltroOt filtro, int maximo, CancellationToken ct);

    Task GuardarAsync(CancellationToken ct);
}

/// <summary>RF-12: genera el archivo Excel de una lista de OT.</summary>
public interface IExportadorOt
{
    byte[] Exportar(IReadOnlyList<OrdenTrabajo> ordenes);
}

public interface ICatalogoRepositorio
{
    Task<IReadOnlyList<Catalogo>> CatalogosAsync(CancellationToken ct);

    Task<IReadOnlyList<MaquinaResumen>> MaquinasAsync(CancellationToken ct);
}

public record EquipoResumen(int Id, string Nombre);
public record SistemaResumen(int Id, string Nombre, IReadOnlyList<EquipoResumen> Equipos);
public record MaquinaResumen(int Id, string Codigo, string Nombre, int AreaId, string Area, int PlantaId, string Planta, IReadOnlyList<SistemaResumen> Sistemas);

public record FiltroPlanning(DateOnly Desde, DateOnly Hasta, int? PlantaId, int? AreaId, int? MaquinaId, int? EspecialidadId);

public interface IPlanningRepositorio
{
    /// <summary>RN-43: cantidad de fechas programadas sin OT por día de inicio, con los filtros.</summary>
    Task<IReadOnlyList<(DateOnly Dia, int Cantidad)>> ContarAsync(FiltroPlanning filtro, CancellationToken ct);
}
