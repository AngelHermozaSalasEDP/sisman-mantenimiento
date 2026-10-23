using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.Domain.Entidades;

public class MantenimientoPreventivo
{
    private readonly List<FechaProgramada> _fechas = [];

    public int Id { get; private set; }
    public string Codigo { get; private set; } = "";
    public string Descripcion { get; private set; } = "";
    public int MaquinaId { get; private set; }
    public int? SistemaId { get; private set; }
    public int? EquipoId { get; private set; }
    public int TipoTrabajoId { get; private set; }
    public int FuenteId { get; private set; }
    public int OportunidadId { get; private set; }
    public int EspecialidadId { get; private set; }
    public Frecuencia Frecuencia { get; private set; }
    public int ValorFrecuencia { get; private set; }
    public byte? Mes { get; private set; }
    public EstadoMp Estado { get; private set; }
    public DateOnly? FechaInicio { get; private set; }
    public DateOnly? FechaProxima { get; private set; }
    public DateOnly? FechaUltima { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public string CreadoPor { get; private set; } = "";
    public DateTime? ModificadoEn { get; private set; }
    public string? ModificadoPor { get; private set; }
    public DateTime? AprobadoEn { get; private set; }
    public string? AprobadoPor { get; private set; }
    public byte[] Version { get; private set; } = [];

    public IReadOnlyCollection<FechaProgramada> Fechas => _fechas;

    private MantenimientoPreventivo() { }

    public static string FormatearCodigo(int numero) => $"MP-{numero:D7}";

    public static MantenimientoPreventivo Crear(int numero, DatosMp datos, string usuario, DateTime ahora)
    {
        var mp = new MantenimientoPreventivo
        {
            Codigo = FormatearCodigo(numero),
            Estado = EstadoMp.Nuevo,
            CreadoEn = ahora,
            CreadoPor = usuario
        };
        mp.Asignar(datos);
        return mp;
    }

    /// <summary>RN-14: editar deja el MP en Modificado y registra quién y cuándo.</summary>
    public void Modificar(DatosMp datos, string usuario, DateTime ahora)
    {
        Asignar(datos);
        Estado = EstadoMp.Modificado;
        ModificadoEn = ahora;
        ModificadoPor = usuario;
    }

    /// <summary>RN-15 y RN-16: un MP aprobado no se aprueba de nuevo; uno modificado sí.</summary>
    public void Aprobar(string usuario, DateTime ahora)
    {
        if (Estado == EstadoMp.Aprobado)
            throw DominioException.Conflicto("mp.ya-aprobado", $"El {Codigo} ya está aprobado.");
        Estado = EstadoMp.Aprobado;
        AprobadoEn = ahora;
        AprobadoPor = usuario;
    }

    /// <summary>RN-20 a RN-24.</summary>
    public FechaProgramada ProgramarFecha(DateOnly inicio, DateOnly fin, DateOnly hoy, string usuario, DateTime ahora)
    {
        if (inicio <= hoy)
            throw DominioException.Validacion("fecha.inicio-pasada", "La fecha de inicio debe ser posterior a hoy.");
        if (inicio > fin)
            throw DominioException.Validacion("fecha.rango-invalido", "La fecha de inicio no puede ser posterior a la de fin.");
        if (Estado != EstadoMp.Aprobado)
            throw DominioException.Conflicto("mp.no-aprobado", $"El {Codigo} no está aprobado.");
        if (_fechas.Any(f => f.FechaInicio == inicio))
            throw DominioException.Conflicto("fecha.duplicada", "Ya existe una fecha programada con ese inicio.");

        var fecha = new FechaProgramada(this, inicio, fin, usuario, ahora);
        _fechas.Add(fecha);
        FechaInicio = inicio;
        FechaProxima = inicio;
        FechaUltima = fin;
        return fecha;
    }

    private void Asignar(DatosMp d)
    {
        if (string.IsNullOrWhiteSpace(d.Descripcion) || d.Descripcion.Length > 200)
            throw DominioException.Validacion("mp.descripcion-invalida", "La descripción es obligatoria y admite hasta 200 caracteres.");
        if (!Enum.IsDefined(d.Frecuencia) || d.ValorFrecuencia <= 0)
            throw DominioException.Validacion("mp.frecuencia-invalida", "La frecuencia debe estar en el catálogo y su valor ser mayor que cero.");
        var exigeMes = d.Frecuencia is Frecuencia.Mensual or Frecuencia.Anual;
        if (exigeMes && (d.Mes is null || d.Mes < 1 || d.Mes > 12))
            throw DominioException.Validacion("mp.mes-obligatorio", "La frecuencia mensual o anual exige un mes entre 1 y 12.");

        Descripcion = d.Descripcion.Trim();
        MaquinaId = d.MaquinaId;
        SistemaId = d.SistemaId;
        EquipoId = d.EquipoId;
        TipoTrabajoId = d.TipoTrabajoId;
        FuenteId = d.FuenteId;
        OportunidadId = d.OportunidadId;
        EspecialidadId = d.EspecialidadId;
        Frecuencia = d.Frecuencia;
        ValorFrecuencia = d.ValorFrecuencia;
        Mes = exigeMes ? d.Mes : null;
    }
}

public record DatosMp(
    string Descripcion, int MaquinaId, int? SistemaId, int? EquipoId,
    int TipoTrabajoId, int FuenteId, int OportunidadId, int EspecialidadId,
    Frecuencia Frecuencia, int ValorFrecuencia, byte? Mes);
