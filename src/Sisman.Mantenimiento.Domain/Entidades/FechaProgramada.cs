using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Domain.Entidades;

public class FechaProgramada
{
    public int Id { get; private set; }
    public int MantenimientoPreventivoId { get; private set; }
    public MantenimientoPreventivo? MantenimientoPreventivo { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public int? OrdenTrabajoId { get; private set; }
    public OrdenTrabajo? OrdenTrabajo { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public string CreadoPor { get; private set; } = "";

    private FechaProgramada() { }

    internal FechaProgramada(MantenimientoPreventivo mp, DateOnly inicio, DateOnly fin, string usuario, DateTime ahora)
    {
        MantenimientoPreventivo = mp;
        FechaInicio = inicio;
        FechaFin = fin;
        CreadoPor = usuario;
        CreadoEn = ahora;
    }

    public bool TieneOt => OrdenTrabajoId is not null || OrdenTrabajo is not null;

    /// <summary>RN-25: solo se elimina una fecha sin OT.</summary>
    public void ValidarEliminable()
    {
        if (TieneOt)
            throw DominioException.Conflicto("fecha.con-ot", "La fecha ya tiene una orden de trabajo.");
    }

    internal void EnlazarOt(OrdenTrabajo ot) { OrdenTrabajo = ot; }
}
