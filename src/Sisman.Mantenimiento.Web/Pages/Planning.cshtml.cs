using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages;

/// <summary>RF-10: calendario del mes con la cantidad de MP programados sin OT por día.</summary>
public class PlanningModel(ApiCliente api) : PaginaBase(api)
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-PE");

    /// <summary>Mes en formato yyyy-MM; por defecto el actual.</summary>
    [BindProperty(SupportsGet = true)] public string? Mes { get; set; }
    [BindProperty(SupportsGet = true)] public int? PlantaId { get; set; }
    [BindProperty(SupportsGet = true)] public int? AreaId { get; set; }
    [BindProperty(SupportsGet = true)] public int? MaquinaId { get; set; }
    [BindProperty(SupportsGet = true)] public int? EspecialidadId { get; set; }

    public DateOnly Primero { get; private set; }
    public PlanningDto? Resultado { get; private set; }
    public IReadOnlyList<MaquinaResumen> Maquinas { get; private set; } = [];
    public CatalogosDto? Catalogos { get; private set; }

    public string TituloMes => Cultura.TextInfo.ToTitleCase(Primero.ToString("MMMM yyyy", Cultura));

    public async Task OnGetAsync(CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        Primero = DateOnly.TryParseExact(Mes, "yyyy-MM", out var m) ? m : new DateOnly(hoy.Year, hoy.Month, 1);
        Mes = Primero.ToString("yyyy-MM");
        try
        {
            Maquinas = await Api.MaquinasAsync(ct);
            Catalogos = await Api.CatalogosAsync(ct);
            Resultado = await Api.PlanningAsync(Primero, Primero.AddMonths(1).AddDays(-1), PlantaId, AreaId, MaquinaId, EspecialidadId, ct);
        }
        catch (ApiException e)
        {
            ErrorCarga = e.Mensaje;
        }
    }

    public string EnlaceMes(int desplazamiento) =>
        $"/planning?mes={Primero.AddMonths(desplazamiento):yyyy-MM}&plantaId={PlantaId}&areaId={AreaId}&maquinaId={MaquinaId}&especialidadId={EspecialidadId}";

    /// <summary>Semanas del mes, de lunes a domingo; null en las celdas fuera del mes.</summary>
    public IEnumerable<DiaPlanningDto?[]> Semanas()
    {
        if (Resultado is null) yield break;
        var inicio = ((int)Primero.DayOfWeek + 6) % 7; // lunes = 0
        var celdas = Enumerable.Repeat<DiaPlanningDto?>(null, inicio).Concat(Resultado.Dias).ToList();
        while (celdas.Count % 7 != 0) celdas.Add(null);
        for (var i = 0; i < celdas.Count; i += 7) yield return celdas.GetRange(i, 7).ToArray();
    }
}
