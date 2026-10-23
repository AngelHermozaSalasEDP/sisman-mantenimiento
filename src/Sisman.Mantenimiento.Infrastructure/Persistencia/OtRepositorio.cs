using Microsoft.EntityFrameworkCore;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Infrastructure.Persistencia;

public class OtRepositorio(SismanDbContext db) : IOtRepositorio
{
    public Task<FechaProgramada?> ObtenerFechaParaGenerarAsync(int fechaId, CancellationToken ct) =>
        // UPDLOCK: dos solicitudes sobre la misma fecha se ponen en cola; la segunda ve la OT de la primera (RN-30).
        db.FechasProgramadas
            .FromSqlInterpolated($"SELECT * FROM [sisman].[FechaProgramada] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {fechaId}")
            .Include(f => f.MantenimientoPreventivo)
            .Include(f => f.OrdenTrabajo)
            .FirstOrDefaultAsync(ct);

    public void Agregar(OrdenTrabajo ot) => db.OrdenesTrabajo.Add(ot);

    public Task<OrdenTrabajo?> ObtenerAsync(int id, CancellationToken ct) =>
        db.OrdenesTrabajo.AsNoTracking().Include(o => o.Historial).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<OrdenTrabajo?> ObtenerParaModificarAsync(int id, CancellationToken ct) =>
        db.OrdenesTrabajo.Include(o => o.Historial).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyList<OrdenTrabajo> Items, int Total)> ConsultarAsync(FiltroOt f, CancellationToken ct)
    {
        var q = Filtrar(f);
        var total = await q.CountAsync(ct);
        // RN-41: el número es entero, así que el orden es numérico (el original ordenaba como texto).
        var items = await q.OrderByDescending(o => o.Numero)
            .Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<OrdenTrabajo>> ConsultarParaExportarAsync(FiltroOt f, int maximo, CancellationToken ct) =>
        await Filtrar(f).OrderByDescending(o => o.Numero).Take(maximo).ToListAsync(ct);

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

    private IQueryable<OrdenTrabajo> Filtrar(FiltroOt f)
    {
        var q = db.OrdenesTrabajo.AsNoTracking().AsQueryable();
        if (f.Numero is not null) q = q.Where(o => o.Numero == f.Numero);
        if (f.Tipo is not null) q = q.Where(o => o.Tipo == f.Tipo);
        if (f.Estado is not null) q = q.Where(o => o.Estado == f.Estado);
        if (f.FechaDesde is not null) q = q.Where(o => o.FechaObjetivo >= f.FechaDesde);
        if (f.FechaHasta is not null) q = q.Where(o => o.FechaObjetivo <= f.FechaHasta);
        return q;
    }
}
