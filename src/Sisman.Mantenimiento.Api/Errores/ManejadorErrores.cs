using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Application.Autenticacion;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Api.Errores;

public static class Politicas
{
    public const string Planificador = "Planificador";
    public const string Consulta = "Consulta";
}

/// <summary>
/// Traduce excepciones a ProblemDetails (RFC 9457) con un código estable y sin detalles internos (ADR-005).
/// Nunca devuelve 2xx si la operación falló (RN-51).
/// </summary>
public class ManejadorErrores(ILogger<ManejadorErrores> log) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (estado, codigo, titulo, detalle) = ex switch
        {
            ValidationException v => (400, "validacion.error", "Entrada inválida", "Uno o más campos no son válidos."),
            CredencialesInvalidasException c => (401, "auth.credenciales-invalidas", "Credenciales inválidas", c.Message),
            DominioException d => (d.Tipo switch
            {
                TipoError.Validacion => 400,
                TipoError.NoEncontrado => 404,
                _ => 409
            }, d.Codigo, d.Tipo == TipoError.Conflicto ? "Conflicto" : d.Tipo == TipoError.NoEncontrado ? "No encontrado" : "Entrada inválida", d.Message),
            _ => (500, "error.interno", "Error interno", "Ocurrió un error inesperado. Indique el identificador de correlación al soporte.")
        };

        if (estado >= 500)
            log.LogError(ex, "Error no controlado");
        else
            log.LogInformation("Solicitud rechazada ({Codigo}): {Mensaje}", codigo, ex.Message);

        var problema = new ProblemDetails
        {
            Type = $"https://sisman.example/errores/{codigo}",
            Title = titulo,
            Status = estado,
            Detail = detalle
        };
        problema.Extensions["codigo"] = codigo;
        problema.Extensions["traceId"] = ctx.TraceIdentifier;
        problema.Extensions["correlationId"] = ctx.Items[CorrelacionMiddleware.Clave]?.ToString();
        if (ex is ValidationException ve)
            problema.Extensions["errors"] = ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        ctx.Response.StatusCode = estado;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(problema, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
