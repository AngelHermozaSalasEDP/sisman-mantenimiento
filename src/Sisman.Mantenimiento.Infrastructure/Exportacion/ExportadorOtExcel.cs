using ClosedXML.Excel;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;

namespace Sisman.Mantenimiento.Infrastructure.Exportacion;

/// <summary>RF-12: hoja «Órdenes de trabajo» con una fila por OT, en el orden de la consulta.</summary>
public class ExportadorOtExcel : IExportadorOt
{
    private static readonly string[] Encabezados =
        ["Número", "Estado", "Prioridad", "Tipo", "Descripción", "Fecha objetivo", "MP (id)", "Máquina (id)", "Creada por", "Creada (UTC)", "Último cambio por", "Último cambio (UTC)"];

    public byte[] Exportar(IReadOnlyList<OrdenTrabajo> ordenes)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Órdenes de trabajo");

        for (var c = 0; c < Encabezados.Length; c++)
            hoja.Cell(1, c + 1).Value = Encabezados[c];
        hoja.Row(1).Style.Font.Bold = true;

        var fila = 2;
        foreach (var o in ordenes)
        {
            hoja.Cell(fila, 1).Value = o.Numero;
            hoja.Cell(fila, 2).Value = NombresEstado.De(o.Estado);
            hoja.Cell(fila, 3).Value = o.Prioridad.ToString();
            hoja.Cell(fila, 4).Value = o.Tipo.ToString();
            hoja.Cell(fila, 5).Value = o.Descripcion;
            hoja.Cell(fila, 6).Value = o.FechaObjetivo.ToDateTime(TimeOnly.MinValue);
            hoja.Cell(fila, 6).Style.DateFormat.Format = "dd/mm/yyyy";
            hoja.Cell(fila, 7).Value = o.MantenimientoPreventivoId;
            hoja.Cell(fila, 8).Value = o.MaquinaId;
            hoja.Cell(fila, 9).Value = o.CreadoPor;
            hoja.Cell(fila, 10).Value = o.CreadoEn;
            hoja.Cell(fila, 11).Value = o.ModificadoPor ?? "";
            if (o.ModificadoEn is { } m) hoja.Cell(fila, 12).Value = m;
            fila++;
        }
        hoja.Column(10).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
        hoja.Column(12).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
        hoja.SheetView.FreezeRows(1);
        hoja.Columns().AdjustToContents(1, Math.Min(fila, 200));

        using var salida = new MemoryStream();
        libro.SaveAs(salida);
        return salida.ToArray();
    }
}
