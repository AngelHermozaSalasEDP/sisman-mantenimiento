using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Sisman.Mantenimiento.Web.Api;

namespace Sisman.Mantenimiento.Web.Pages.Mp;

/// <summary>Alta (RF-01) y edición (RF-03) de un MP. Las reglas las valida la API; aquí solo se arma el formulario.</summary>
public class FormularioModel(ApiCliente api) : PaginaBase(api)
{
    public class Datos
    {
        [Required(ErrorMessage = "Ingrese la descripción."), StringLength(200)]
        public string Descripcion { get; set; } = "";
        [Range(1, int.MaxValue, ErrorMessage = "Elija la máquina.")] public int MaquinaId { get; set; }
        public int? SistemaId { get; set; }
        public int? EquipoId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Elija el tipo de trabajo.")] public int TipoTrabajoId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Elija la fuente de información.")] public int FuenteId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Elija la oportunidad de intervención.")] public int OportunidadId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Elija la especialidad.")] public int EspecialidadId { get; set; }
        [Range(1, 4)] public int Frecuencia { get; set; } = 2;
        [Range(1, 999, ErrorMessage = "El valor de la frecuencia debe ser mayor que cero.")] public int ValorFrecuencia { get; set; } = 1;
        public byte? Mes { get; set; }

        public MpEntrada AEntrada() => new(Descripcion, MaquinaId, SistemaId, EquipoId, TipoTrabajoId, FuenteId,
            OportunidadId, EspecialidadId, Frecuencia, ValorFrecuencia, Frecuencia is 3 or 4 ? Mes : null);
    }

    [BindProperty] public Datos Mp { get; set; } = new();

    public int? Id { get; private set; }
    public string? Codigo { get; private set; }
    public CatalogosDto? Catalogos { get; private set; }
    public IReadOnlyList<MaquinaResumen> Maquinas { get; private set; } = [];
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct)
    {
        if (!EsPlanificador) return Forbid();
        Id = id;
        if (id is not null)
        {
            try
            {
                var mp = await Api.ObtenerMpAsync(id.Value, ct);
                Codigo = mp.Codigo;
                Mp = new Datos
                {
                    Descripcion = mp.Descripcion, MaquinaId = mp.MaquinaId, SistemaId = mp.SistemaId, EquipoId = mp.EquipoId,
                    TipoTrabajoId = mp.TipoTrabajoId, FuenteId = mp.FuenteId, OportunidadId = mp.OportunidadId,
                    EspecialidadId = mp.EspecialidadId, Frecuencia = mp.Frecuencia, ValorFrecuencia = mp.ValorFrecuencia, Mes = mp.Mes
                };
            }
            catch (ApiException e) when (e.Estado == 404)
            {
                return NotFound();
            }
        }
        await CargarListasAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id, CancellationToken ct)
    {
        if (!EsPlanificador) return Forbid();
        Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                var mp = id is null
                    ? await Api.CrearMpAsync(Mp.AEntrada(), ct)
                    : await Api.EditarMpAsync(id.Value, Mp.AEntrada(), ct);
                TempData["Exito"] = id is null ? $"Se registró el {mp.Codigo}." : $"Se guardaron los cambios del {mp.Codigo}; quedó en estado {mp.Estado}.";
                return Redirect($"/mp/{mp.Id}");
            }
            catch (ApiException e)
            {
                Error = e.Mensaje;
            }
        }
        await CargarListasAsync(ct);
        return Page();
    }

    private async Task CargarListasAsync(CancellationToken ct)
    {
        Catalogos = await Api.CatalogosAsync(ct);
        Maquinas = await Api.MaquinasAsync(ct);
    }
}
