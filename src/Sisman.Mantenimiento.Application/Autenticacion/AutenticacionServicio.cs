using FluentValidation;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain;

namespace Sisman.Mantenimiento.Application.Autenticacion;

public record LoginSolicitud(string Usuario, string Clave);

public record LoginRespuesta(string Token, DateTime VenceEn, Perfil Perfil);

/// <summary>Credenciales inválidas; la API responde 401 sin revelar si el usuario existe (ADR-006).</summary>
public class CredencialesInvalidasException()
    : Exception("Usuario o clave incorrectos.");

public class LoginValidator : AbstractValidator<LoginSolicitud>
{
    public LoginValidator()
    {
        RuleFor(x => x.Usuario).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Clave).NotEmpty().MaximumLength(100);
    }
}

/// <summary>RF-11: inicia sesión y emite un token que vence a los 30 minutos (RN-02).</summary>
public class AutenticacionServicio(
    IUsuarioRepositorio usuarios,
    IHasheadorClave hasheador,
    IGeneradorToken tokens,
    IValidator<LoginSolicitud> validador)
{
    public async Task<LoginRespuesta> IniciarSesionAsync(LoginSolicitud solicitud, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(solicitud, ct);

        var usuario = await usuarios.BuscarPorNombreAsync(solicitud.Usuario, ct);
        if (usuario is null || !usuario.Activo || !hasheador.Verificar(usuario, solicitud.Clave))
            throw new CredencialesInvalidasException();

        var emitido = tokens.Generar(usuario);
        return new LoginRespuesta(emitido.Token, emitido.VenceEn, usuario.Perfil);
    }
}
