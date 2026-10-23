using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Infrastructure.Persistencia;

public class MpRepositorio(SismanDbContext db) : IMpRepositorio
{
    public Task<MantenimientoPreventivo?> ObtenerConFechasAsync(int id, CancellationToken ct) =>
        db.MantenimientosPreventivos
            .Include(m => m.Fechas).ThenInclude(f => f.OrdenTrabajo)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<(IReadOnlyList<MantenimientoPreventivo> Items, int Total)> ConsultarAsync(FiltroMp f, CancellationToken ct)
    {
        var q = db.MantenimientosPreventivos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.Codigo)) q = q.Where(m => m.Codigo.Contains(f.Codigo.Trim()));
        if (f.TipoTrabajoId is not null) q = q.Where(m => m.TipoTrabajoId == f.TipoTrabajoId);
        if (f.Estado is not null) q = q.Where(m => m.Estado == f.Estado);
        if (f.FechaDesde is not null) q = q.Where(m => m.FechaInicio >= f.FechaDesde);
        if (f.FechaHasta is not null) q = q.Where(m => m.FechaInicio <= f.FechaHasta);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(m => m.Id)
            .Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task ValidarReferenciasAsync(DatosMp d, CancellationToken ct)
    {
        if (!await db.Maquinas.AnyAsync(m => m.Id == d.MaquinaId, ct))
            throw Invalida("la máquina", d.MaquinaId);
        if (d.SistemaId is not null && !await db.Sistemas.AnyAsync(s => s.Id == d.SistemaId && s.MaquinaId == d.MaquinaId, ct))
            throw Invalida("el sistema (o no pertenece a la máquina)", d.SistemaId.Value);
        if (d.EquipoId is not null && !await db.Equipos.AnyAsync(e => e.Id == d.EquipoId && e.SistemaId == d.SistemaId, ct))
            throw Invalida("el equipo (o no pertenece al sistema)", d.EquipoId.Value);

        await ExigirCatalogoAsync(d.TipoTrabajoId, CategoriaCatalogo.TipoTrabajo, "el tipo de trabajo", ct);
        await ExigirCatalogoAsync(d.FuenteId, CategoriaCatalogo.FuenteInformacion, "la fuente de información", ct);
        await ExigirCatalogoAsync(d.OportunidadId, CategoriaCatalogo.OportunidadIntervencion, "la oportunidad de intervención", ct);
        await ExigirCatalogoAsync(d.EspecialidadId, CategoriaCatalogo.Especialidad, "la especialidad", ct);
    }

    public void Agregar(MantenimientoPreventivo mp) => db.MantenimientosPreventivos.Add(mp);

    public void EliminarFecha(FechaProgramada fecha) => db.FechasProgramadas.Remove(fecha);

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
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Segunda defensa de RN-23 si dos solicitudes programan el mismo inicio a la vez.
            throw DominioException.Conflicto("fecha.duplicada", "Ya existe una fecha programada con ese inicio.");
        }
    }

    private async Task ExigirCatalogoAsync(int id, CategoriaCatalogo categoria, string nombre, CancellationToken ct)
    {
        if (!await db.Catalogos.AnyAsync(c => c.Id == id && c.Categoria == categoria, ct))
            throw Invalida(nombre, id);
    }

    private static DominioException Invalida(string que, int id) =>
        DominioException.Validacion("mp.referencia-invalida", $"No existe {que} indicado ({id}).");
}
