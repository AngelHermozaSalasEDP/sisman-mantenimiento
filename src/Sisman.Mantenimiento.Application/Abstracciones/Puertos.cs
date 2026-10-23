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
