using Microsoft.EntityFrameworkCore;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain.Entidades;

namespace Sisman.Mantenimiento.Infrastructure.Persistencia;

public class CatalogoRepositorio(SismanDbContext db) : ICatalogoRepositorio
{
    public async Task<IReadOnlyList<Catalogo>> CatalogosAsync(CancellationToken ct) =>
        await db.Catalogos.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<MaquinaResumen>> MaquinasAsync(CancellationToken ct)
    {
        // Cuatro consultas pequeñas y un armado en memoria: los catálogos de planta son de decenas de filas.
        var maquinas = await (from m in db.Maquinas.AsNoTracking()
                              join a in db.Areas on m.AreaId equals a.Id
                              join p in db.Plantas on a.PlantaId equals p.Id
                              orderby m.Codigo
                              select new { m.Id, m.Codigo, m.Nombre, m.AreaId, Area = a.Nombre, a.PlantaId, Planta = p.Nombre })
                             .ToListAsync(ct);
        var sistemas = await db.Sistemas.AsNoTracking().OrderBy(s => s.Nombre).ToListAsync(ct);
        var equipos = await db.Equipos.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync(ct);

        return maquinas.Select(m => new MaquinaResumen(m.Id, m.Codigo, m.Nombre, m.AreaId, m.Area, m.PlantaId, m.Planta,
            sistemas.Where(s => s.MaquinaId == m.Id)
                .Select(s => new SistemaResumen(s.Id, s.Nombre,
                    equipos.Where(e => e.SistemaId == s.Id).Select(e => new EquipoResumen(e.Id, e.Nombre)).ToList()))
                .ToList()))
            .ToList();
    }
}

public class PlanningRepositorio(SismanDbContext db) : IPlanningRepositorio
{
    public async Task<IReadOnlyList<(DateOnly Dia, int Cantidad)>> ContarAsync(FiltroPlanning f, CancellationToken ct)
    {
        // RN-43: solo cuentan las fechas que todavía no tienen OT.
        var q = from fecha in db.FechasProgramadas.AsNoTracking()
                join mp in db.MantenimientosPreventivos on fecha.MantenimientoPreventivoId equals mp.Id
                join m in db.Maquinas on mp.MaquinaId equals m.Id
                join a in db.Areas on m.AreaId equals a.Id
                where fecha.OrdenTrabajoId == null && fecha.FechaInicio >= f.Desde && fecha.FechaInicio <= f.Hasta
                select new { fecha.FechaInicio, mp.MaquinaId, mp.EspecialidadId, m.AreaId, a.PlantaId };

        if (f.PlantaId is not null) q = q.Where(x => x.PlantaId == f.PlantaId);
        if (f.AreaId is not null) q = q.Where(x => x.AreaId == f.AreaId);
        if (f.MaquinaId is not null) q = q.Where(x => x.MaquinaId == f.MaquinaId);
        if (f.EspecialidadId is not null) q = q.Where(x => x.EspecialidadId == f.EspecialidadId);

        var filas = await q.GroupBy(x => x.FechaInicio)
            .Select(g => new { Dia = g.Key, Cantidad = g.Count() })
            .ToListAsync(ct);
        return filas.Select(x => (x.Dia, x.Cantidad)).ToList();
    }
}
