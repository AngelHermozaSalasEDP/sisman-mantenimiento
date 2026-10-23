namespace Sisman.Mantenimiento.Web.Api;

// Copia mínima de los contratos JSON de la API. La web es un cliente HTTP más: no referencia
// los proyectos de la API, solo conoce lo que publica el OpenAPI.

public record LoginRespuesta(string Token, DateTime VenceEn, string Perfil);

public record Pagina<T>(IReadOnlyList<T> Items, int PaginaActual, int TamanoPagina, int Total)
{
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Total / (double)TamanoPagina));
}

public record FechaDto(int Id, DateOnly FechaInicio, DateOnly FechaFin, int? NumeroOt);

public record MpDto(
    int Id, string Codigo, string Estado, string Descripcion, int MaquinaId, int? SistemaId, int? EquipoId,
    int TipoTrabajoId, int FuenteId, int OportunidadId, int EspecialidadId,
    int Frecuencia, int ValorFrecuencia, byte? Mes,
    DateOnly? FechaInicio, DateOnly? FechaProxima, DateOnly? FechaUltima,
    string? ModificadoPor, DateTime? ModificadoEn, string? AprobadoPor, DateTime? AprobadoEn,
    IReadOnlyList<FechaDto> Fechas);

/// <summary>Lo que se envía al registrar (RF-01) o editar (RF-03) un MP.</summary>
public record MpEntrada(
    string Descripcion, int MaquinaId, int? SistemaId, int? EquipoId,
    int TipoTrabajoId, int FuenteId, int OportunidadId, int EspecialidadId,
    int Frecuencia, int ValorFrecuencia, byte? Mes);

public record CatalogoDto(int Id, string Categoria, string Nombre);

public record CatalogosDto(
    IReadOnlyList<CatalogoDto> TiposTrabajo, IReadOnlyList<CatalogoDto> Fuentes,
    IReadOnlyList<CatalogoDto> Oportunidades, IReadOnlyList<CatalogoDto> Especialidades);

public record EquipoResumen(int Id, string Nombre);
public record SistemaResumen(int Id, string Nombre, IReadOnlyList<EquipoResumen> Equipos);
public record MaquinaResumen(int Id, string Codigo, string Nombre, int AreaId, string Area, int PlantaId, string Planta, IReadOnlyList<SistemaResumen> Sistemas);

public record DiaPlanningDto(DateOnly Dia, int Cantidad);
public record PlanningDto(DateOnly Desde, DateOnly Hasta, int Total, IReadOnlyList<DiaPlanningDto> Dias);

public record Archivo(byte[] Contenido, string Tipo, string Nombre);

public record HistorialDto(int EstadoAnterior, int EstadoNuevo, string Usuario, DateTime Fecha);

public record OtDto(
    int Id, int Numero, string Tipo, string Prioridad, int Estado, string Descripcion,
    int MaquinaId, int MantenimientoPreventivoId, DateOnly FechaObjetivo,
    string CreadoPor, DateTime CreadoEn, string? ModificadoPor, DateTime? ModificadoEn,
    IReadOnlyList<int> TransicionesPermitidas, IReadOnlyList<HistorialDto> Historial);

public static class Textos
{
    public static string Estado(int codigo) => codigo switch
    {
        37 => "Solicitud",
        38 => "Anulación",
        39 => "En programación",
        40 => "Pendiente",
        41 => "Concluido",
        42 => "Concluido por anulación",
        _ => codigo.ToString()
    };

    public static string Frecuencia(int codigo) => codigo switch
    {
        1 => "Diario", 2 => "Semanal", 3 => "Mensual", 4 => "Anual", _ => codigo.ToString()
    };

    public static string Fecha(DateOnly? f) => f?.ToString("dd/MM/yyyy") ?? "—";

    public static string Momento(DateTime? m) => m?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "—";
}
