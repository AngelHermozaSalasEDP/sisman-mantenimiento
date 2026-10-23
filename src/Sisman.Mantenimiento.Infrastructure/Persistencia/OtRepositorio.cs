using System.Data;
using Microsoft.EntityFrameworkCore;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Infrastructure.Persistencia;

public class OtRepositorio(SismanDbContext db) : IOtRepositorio
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

    public Task<FechaProgramada?> ObtenerFechaParaGenerarAsync(int fechaId, CancellationToken ct) =>
        // UPDLOCK: dos solicitudes sobre la misma fecha se ponen en cola; la segunda ve la OT de la primera (RN-30).
        db.FechasProgramadas
            .FromSqlInterpolated($"SELECT * FROM [sisman].[FechaProgramada] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {fechaId}")
            .Include(f => f.MantenimientoPreventivo)
            .Include(f => f.OrdenTrabajo)
            .FirstOrDefaultAsync(ct);

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

    public void Agregar(OrdenTrabajo ot) => db.OrdenesTrabajo.Add(ot);

    public Task<OrdenTrabajo?> ObtenerAsync(int id, CancellationToken ct) =>
        db.OrdenesTrabajo.AsNoTracking().Include(o => o.Historial).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<OrdenTrabajo?> ObtenerParaModificarAsync(int id, CancellationToken ct) =>
        db.OrdenesTrabajo.Include(o => o.Historial).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyList<OrdenTrabajo> Items, int Total)> ConsultarAsync(FiltroOt f, CancellationToken ct)
    {
        var q = db.OrdenesTrabajo.AsNoTracking().AsQueryable();
        if (f.Numero is not null) q = q.Where(o => o.Numero == f.Numero);
        if (f.Tipo is not null) q = q.Where(o => o.Tipo == f.Tipo);
        if (f.Estado is not null) q = q.Where(o => o.Estado == f.Estado);
        if (f.FechaDesde is not null) q = q.Where(o => o.FechaObjetivo >= f.FechaDesde);
        if (f.FechaHasta is not null) q = q.Where(o => o.FechaObjetivo <= f.FechaHasta);

        var total = await q.CountAsync(ct);
        // RN-41: el número es entero, así que el orden es numérico (el original ordenaba como texto).
        var items = await q.OrderByDescending(o => o.Numero)
            .Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task GuardarAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw DominioException.Conflicto("concurrencia.conflicto", "El registro cambió mientras se editaba. Vuelva a consultarlo e intente de nuevo.");
        }
    }
}
