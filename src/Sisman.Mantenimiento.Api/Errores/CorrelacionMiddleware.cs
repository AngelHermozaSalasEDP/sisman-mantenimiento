using Serilog.Context;

namespace Sisman.Mantenimiento.Api.Errores;

/// <summary>RN-54: acepta o genera X-Correlation-Id, lo devuelve y lo pone en cada línea del registro.</summary>
public class CorrelacionMiddleware(RequestDelegate siguiente)
{
    public const string Cabecera = "X-Correlation-Id";
    public const string Clave = "CorrelationId";

    public async Task InvokeAsync(HttpContext ctx)
    {
        var id = ctx.Request.Headers[Cabecera].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(id) || id.Length > 64) id = Guid.NewGuid().ToString("N");

        ctx.Items[Clave] = id;
        ctx.Response.OnStarting(() =>
        {
            ctx.Response.Headers[Cabecera] = id;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(Clave, id))
            await siguiente(ctx);
    }
}
