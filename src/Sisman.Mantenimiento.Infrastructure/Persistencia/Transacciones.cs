using System.Data;
using Microsoft.EntityFrameworkCore;
using Sisman.Mantenimiento.Application.Abstracciones;

namespace Sisman.Mantenimiento.Infrastructure.Persistencia;

public class Transacciones(SismanDbContext db) : ITransacciones
{
    public async Task<T> EnTransaccionAsync<T>(Func<CancellationToken, Task<T>> operacion, CancellationToken ct)
    {
        // Con reintentos habilitados, la transacción debe ir dentro de la estrategia de ejecución:
        // si la base pide reintentar, se repite la operación completa desde cero.
        var estrategia = db.Database.CreateExecutionStrategy();
        return await estrategia.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            var resultado = await operacion(ct);
            await tx.CommitAsync(ct);
            return resultado;
        });
    }

    public async Task<int> SiguienteNumeroAsync(string correlativo, CancellationToken ct)
    {
        // Parametrizado (RNF-03). La fila del correlativo queda bloqueada hasta el COMMIT o el ROLLBACK.
        var valores = await db.Database.SqlQuery<int>($"""
            UPDATE [sisman].[Correlativo] WITH (UPDLOCK, ROWLOCK)
               SET [UltimoValor] = [UltimoValor] + 1
            OUTPUT inserted.[UltimoValor]
             WHERE [Nombre] = {correlativo}
            """).ToListAsync(ct);

        return valores.Count == 1
            ? valores[0]
            : throw new InvalidOperationException($"No existe el correlativo «{correlativo}».");
    }
}
