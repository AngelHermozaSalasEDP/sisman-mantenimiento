using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Domain.Entidades;

public class OrdenTrabajo
{
    private readonly List<OrdenTrabajoHistorial> _historial = [];

    /// <summary>RN-37 (a validar con un usuario): transiciones permitidas.</summary>
    private static readonly Dictionary<EstadoOt, EstadoOt[]> Transiciones = new()
    {
        [EstadoOt.Solicitud] = [EstadoOt.EnProgramacion],
        [EstadoOt.EnProgramacion] = [EstadoOt.Pendiente],
        [EstadoOt.Pendiente] = [EstadoOt.Concluido, EstadoOt.Anulacion],
        [EstadoOt.Anulacion] = [EstadoOt.ConcluidoPorAnulacion],
        [EstadoOt.Concluido] = [EstadoOt.Pendiente],
        [EstadoOt.ConcluidoPorAnulacion] = []
    };

    public int Id { get; private set; }
    public int Numero { get; private set; }
    public TipoOt Tipo { get; private set; }
    public PrioridadOt Prioridad { get; private set; }
    public EstadoOt Estado { get; private set; }
    public string Descripcion { get; private set; } = "";
    public int MaquinaId { get; private set; }
    public int? SistemaId { get; private set; }
    public int? EquipoId { get; private set; }
    public int TipoTrabajoId { get; private set; }
    public int FuenteId { get; private set; }
    public int OportunidadId { get; private set; }
    public int EspecialidadId { get; private set; }
    public int MantenimientoPreventivoId { get; private set; }
    public DateOnly FechaObjetivo { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public string CreadoPor { get; private set; } = "";
    public DateTime? ModificadoEn { get; private set; }
    public string? ModificadoPor { get; private set; }
    public byte[] Version { get; private set; } = [];

    public IReadOnlyCollection<OrdenTrabajoHistorial> Historial => _historial;

    /// <summary>Estados a los que se puede pasar desde el actual (RN-37); la interfaz los ofrece sin repetir la regla.</summary>
    public IReadOnlyList<EstadoOt> TransicionesPermitidas =>
        Transiciones.TryGetValue(Estado, out var permitidos) ? permitidos : [];

    private OrdenTrabajo() { }

    /// <summary>RN-30, RN-31 y RN-35: genera la OT desde una fecha sin OT de un MP aprobado.</summary>
    public static OrdenTrabajo Generar(MantenimientoPreventivo mp, FechaProgramada fecha, int numero, string usuario, DateTime ahora)
    {
        if (fecha.TieneOt)
            throw DominioException.Conflicto("fecha.con-ot", "La fecha ya tiene una orden de trabajo.");
        if (mp.Estado != EstadoMp.Aprobado)
            throw DominioException.Conflicto("mp.no-aprobado", $"El {mp.Codigo} no está aprobado.");
        if (numero <= 0)
            throw DominioException.Validacion("ot.numero-invalido", "El número de OT debe ser positivo.");

        var ot = new OrdenTrabajo
        {
            Numero = numero,
            Tipo = TipoOt.Preventivo,
            Prioridad = PrioridadOt.Alta,
            Estado = EstadoOt.Pendiente,
            Descripcion = mp.Descripcion,
            MaquinaId = mp.MaquinaId,
            SistemaId = mp.SistemaId,
            EquipoId = mp.EquipoId,
            TipoTrabajoId = mp.TipoTrabajoId,
            FuenteId = mp.FuenteId,
            OportunidadId = mp.OportunidadId,
            EspecialidadId = mp.EspecialidadId,
            MantenimientoPreventivoId = mp.Id,
            FechaObjetivo = fecha.FechaInicio,
            CreadoEn = ahora,
            CreadoPor = usuario
        };
        fecha.EnlazarOt(ot);
        return ot;
    }

    /// <summary>RN-37 y RN-38.</summary>
    public void CambiarEstado(EstadoOt nuevo, string usuario, DateTime ahora)
    {
        if (!Transiciones.TryGetValue(Estado, out var permitidos) || !permitidos.Contains(nuevo))
            throw DominioException.Conflicto("ot.transicion-invalida", $"No se puede pasar de {Estado} a {nuevo}.");

        _historial.Add(new OrdenTrabajoHistorial(this, Estado, nuevo, usuario, ahora));
        Estado = nuevo;
        ModificadoEn = ahora;
        ModificadoPor = usuario;
    }
}

public class OrdenTrabajoHistorial
{
    public int Id { get; private set; }
    public int OrdenTrabajoId { get; private set; }
    public EstadoOt EstadoAnterior { get; private set; }
    public EstadoOt EstadoNuevo { get; private set; }
    public string Usuario { get; private set; } = "";
    public DateTime Fecha { get; private set; }

    private OrdenTrabajoHistorial() { }

    internal OrdenTrabajoHistorial(OrdenTrabajo ot, EstadoOt anterior, EstadoOt nuevo, string usuario, DateTime fecha)
    {
        OrdenTrabajoId = ot.Id;
        EstadoAnterior = anterior;
        EstadoNuevo = nuevo;
        Usuario = usuario;
        Fecha = fecha;
    }
}
