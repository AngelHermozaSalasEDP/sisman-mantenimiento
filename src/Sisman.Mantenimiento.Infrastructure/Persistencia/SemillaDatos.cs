using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Infrastructure.Seguridad;

namespace Sisman.Mantenimiento.Infrastructure.Persistencia;

public class SemillaOpciones
{
    public const string Seccion = "Semilla";
    public bool Habilitada { get; set; }
    public string ClavePlanificador { get; set; } = "";
    public string ClaveEspecialista { get; set; } = "";
}

/// <summary>
/// Datos de ejemplo ficticios (no provienen del sistema original). Es idempotente:
/// si ya hay usuarios no hace nada.
/// </summary>
public class SemillaDatos(SismanDbContext db, HasheadorClave hasheador, IReloj reloj, IOptions<SemillaOpciones> opciones)
{
    private const string Sistema = "semilla";

    public async Task EjecutarAsync(CancellationToken ct = default)
    {
        var o = opciones.Value;
        if (!o.Habilitada || await db.Usuarios.AnyAsync(ct)) return;
        if (string.IsNullOrWhiteSpace(o.ClavePlanificador) || string.IsNullOrWhiteSpace(o.ClaveEspecialista))
            throw new InvalidOperationException("La semilla exige Semilla:ClavePlanificador y Semilla:ClaveEspecialista.");

        AgregarUsuarios(o);
        AgregarCatalogos();
        await db.SaveChangesAsync(ct);

        await AgregarMaquinasAsync(ct);
        await AgregarMantenimientosAsync(ct);
        await AgregarOrdenesAsync(ct);
    }

    private void AgregarUsuarios(SemillaOpciones o)
    {
        foreach (var (nombre, perfil, clave) in new[]
        {
            ("planificador", Perfil.Planificador, o.ClavePlanificador),
            ("especialista", Perfil.Especialista, o.ClaveEspecialista)
        })
        {
            var u = new Usuario { NombreUsuario = nombre, Perfil = perfil };
            u.ClaveHash = hasheador.Hashear(u, clave);
            db.Usuarios.Add(u);
        }
    }

    private void AgregarCatalogos()
    {
        void Lote(CategoriaCatalogo c, params string[] nombres) =>
            db.Catalogos.AddRange(nombres.Select(n => new Catalogo { Categoria = c, Nombre = n }));

        Lote(CategoriaCatalogo.TipoTrabajo, "Lubricación", "Inspección", "Limpieza", "Ajuste", "Cambio de repuesto");
        Lote(CategoriaCatalogo.FuenteInformacion, "Manual del fabricante", "Historial de fallas", "Experiencia del operador");
        Lote(CategoriaCatalogo.OportunidadIntervencion, "Máquina detenida", "Cambio de turno", "Parada programada");
        Lote(CategoriaCatalogo.Especialidad, "Mecánica", "Eléctrica", "Electrónica", "Instrumentación");
        db.Correlativos.AddRange(new Correlativo { Nombre = "OT" }, new Correlativo { Nombre = "MP" });
    }

    private async Task AgregarMaquinasAsync(CancellationToken ct)
    {
        var planta = new Planta { Nombre = "Planta de impresión" };
        db.Plantas.Add(planta);
        await db.SaveChangesAsync(ct);

        var areas = new[] { "Prensa", "Acabado", "Servicios" }
            .Select(n => new Area { PlantaId = planta.Id, Nombre = n }).ToList();
        db.Areas.AddRange(areas);
        await db.SaveChangesAsync(ct);

        for (var i = 1; i <= 8; i++)
        {
            var m = new Maquina { AreaId = areas[i % areas.Count].Id, Codigo = $"MAQ-{i:D3}", Nombre = $"Máquina de ejemplo {i}" };
            db.Maquinas.Add(m);
            await db.SaveChangesAsync(ct);
            var s = new Sistema { MaquinaId = m.Id, Nombre = "Sistema hidráulico" };
            db.Sistemas.Add(s);
            await db.SaveChangesAsync(ct);
            db.Equipos.Add(new Equipo { SistemaId = s.Id, Nombre = "Bomba principal" });
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task AgregarMantenimientosAsync(CancellationToken ct)
    {
        var maquinas = await db.Maquinas.OrderBy(m => m.Id).ToListAsync(ct);
        var cat = await db.Catalogos.ToListAsync(ct);
        int Primero(CategoriaCatalogo c, int salto) =>
            cat.Where(x => x.Categoria == c).OrderBy(x => x.Id).ElementAt(salto % cat.Count(x => x.Categoria == c)).Id;
        var correlativo = await db.Correlativos.SingleAsync(c => c.Nombre == "MP", ct);
        var ahora = reloj.UtcAhora;
        var hoy = reloj.Hoy;
        var frecuencias = new[] { Frecuencia.Diario, Frecuencia.Semanal, Frecuencia.Mensual, Frecuencia.Anual };

        for (var i = 0; i < 30; i++)
        {
            var f = frecuencias[i % 4];
            var maquina = maquinas[i % maquinas.Count];
            var datos = new DatosMp(
                $"Mantenimiento preventivo de ejemplo {i + 1}", maquina.Id,
                await db.Sistemas.Where(s => s.MaquinaId == maquina.Id).Select(s => (int?)s.Id).FirstAsync(ct),
                null,
                Primero(CategoriaCatalogo.TipoTrabajo, i), Primero(CategoriaCatalogo.FuenteInformacion, i),
                Primero(CategoriaCatalogo.OportunidadIntervencion, i), Primero(CategoriaCatalogo.Especialidad, i),
                f, 1 + i % 3, f is Frecuencia.Mensual or Frecuencia.Anual ? (byte)(1 + i % 12) : null);

            correlativo.UltimoValor++;
            var mp = MantenimientoPreventivo.Crear(correlativo.UltimoValor, datos, Sistema, ahora);
            if (i >= 12 && i < 18) mp.Modificar(datos with { Descripcion = datos.Descripcion + " (editado)" }, Sistema, ahora);
            if (i >= 18) mp.Aprobar(Sistema, ahora);
            if (i >= 22) mp.ProgramarFecha(hoy.AddDays(3 + i), hoy.AddDays(3 + i), hoy, Sistema, ahora);
            db.MantenimientosPreventivos.Add(mp);
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task AgregarOrdenesAsync(CancellationToken ct)
    {
        var correlativo = await db.Correlativos.SingleAsync(c => c.Nombre == "OT", ct);
        var mps = await db.MantenimientosPreventivos.Include(m => m.Fechas)
            .Where(m => m.Fechas.Any()).OrderBy(m => m.Id).Take(3).ToListAsync(ct);
        foreach (var mp in mps)
        {
            correlativo.UltimoValor++;
            OrdenTrabajo.Generar(mp, mp.Fechas.First(), correlativo.UltimoValor, Sistema, reloj.UtcAhora);
        }
        await db.SaveChangesAsync(ct);
    }
}
