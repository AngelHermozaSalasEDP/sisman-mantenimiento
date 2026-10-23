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
