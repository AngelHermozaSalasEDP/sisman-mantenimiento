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

public record FiltroMp(
    string? Codigo, int? TipoTrabajoId, EstadoMp? Estado, DateOnly? FechaDesde, DateOnly? FechaHasta,
    int Pagina, int TamanoPagina);

public interface IOtRepositorio
{
    /// <summary>
    /// Ejecuta la operación en una transacción. Si algo falla, se deshace todo, incluido el avance
    /// del correlativo (RN-33). Si la base pide reintentar, se repite la operación completa.
    /// </summary>
    Task<T> EnTransaccionAsync<T>(Func<CancellationToken, Task<T>> operacion, CancellationToken ct);

    /// <summary>Lee la fecha con su MP y su OT, bloqueando la fila hasta el fin de la transacción.</summary>
    Task<FechaProgramada?> ObtenerFechaParaGenerarAsync(int fechaId, CancellationToken ct);

    /// <summary>Avanza el correlativo y devuelve el nuevo valor; la fila queda bloqueada hasta el fin de la transacción (RN-34).</summary>
    Task<int> SiguienteNumeroAsync(string correlativo, CancellationToken ct);

    void Agregar(OrdenTrabajo ot);

    Task<OrdenTrabajo?> ObtenerAsync(int id, CancellationToken ct);

    /// <summary>Devuelve la OT con su historial, lista para modificarla (RF-09).</summary>
    Task<OrdenTrabajo?> ObtenerParaModificarAsync(int id, CancellationToken ct);

    Task<(IReadOnlyList<OrdenTrabajo> Items, int Total)> ConsultarAsync(FiltroOt filtro, CancellationToken ct);

    Task GuardarAsync(CancellationToken ct);
}

public record FiltroOt(
    int? Numero, TipoOt? Tipo, EstadoOt? Estado, DateOnly? FechaDesde, DateOnly? FechaHasta,
    int Pagina, int TamanoPagina);

public interface IMpRepositorio
{
    /// <summary>Devuelve el MP con sus fechas (y el número de OT de cada una), listo para modificarlo.</summary>
    Task<MantenimientoPreventivo?> ObtenerConFechasAsync(int id, CancellationToken ct);

    Task<(IReadOnlyList<MantenimientoPreventivo> Items, int Total)> ConsultarAsync(FiltroMp filtro, CancellationToken ct);

    /// <summary>Falla con 400 si una máquina, sistema, equipo o catálogo indicado no existe (RN-11).</summary>
    Task ValidarReferenciasAsync(DatosMp datos, CancellationToken ct);

    /// <summary>Guarda los cambios; una edición concurrente o un duplicado se informan como 409.</summary>
    Task GuardarAsync(CancellationToken ct);
}
