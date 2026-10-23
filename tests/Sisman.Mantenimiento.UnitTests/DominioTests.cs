using Sisman.Mantenimiento.Domain;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Domain.Excepciones;

namespace Sisman.Mantenimiento.UnitTests;

public class DominioTests
{
    private static readonly DateTime Ahora = new(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Hoy = new(2026, 10, 23);

    private static DatosMp Datos(Frecuencia f = Frecuencia.Semanal, byte? mes = null, string descripcion = "Lubricar rodamientos") =>
        new(descripcion, 1, null, null, 1, 1, 1, 1, f, 1, mes);

    private static MantenimientoPreventivo MpAprobado()
    {
        var mp = MantenimientoPreventivo.Crear(1, Datos(), "ana", Ahora);
        mp.Aprobar("ana", Ahora);
        return mp;
    }

    private static string Codigo(Action accion) =>
        Assert.Throws<DominioException>(accion).Codigo;

    [Fact(DisplayName = "RN-10: el código es MP- más 7 dígitos")]
    public void Codigo_Del_Mp_Tiene_Siete_Digitos() =>
        Assert.Equal("MP-0000042", MantenimientoPreventivo.Crear(42, Datos(), "ana", Ahora).Codigo);

    [Fact(DisplayName = "RN-11: descripción vacía se rechaza")]
    public void Descripcion_Vacia_Se_Rechaza() =>
        Assert.Equal("mp.descripcion-invalida", Codigo(() => MantenimientoPreventivo.Crear(1, Datos(descripcion: " "), "ana", Ahora)));

    [Fact(DisplayName = "RN-12: frecuencia fuera del catálogo se rechaza")]
    public void Frecuencia_Fuera_Del_Catalogo_Se_Rechaza() =>
        Assert.Equal("mp.frecuencia-invalida", Codigo(() => MantenimientoPreventivo.Crear(1, Datos((Frecuencia)5), "ana", Ahora)));

    [Theory(DisplayName = "RN-13: frecuencia mensual o anual exige mes")]
    [InlineData(Frecuencia.Mensual)]
    [InlineData(Frecuencia.Anual)]
    public void Frecuencia_Mensual_O_Anual_Exige_Mes(Frecuencia f) =>
        Assert.Equal("mp.mes-obligatorio", Codigo(() => MantenimientoPreventivo.Crear(1, Datos(f), "ana", Ahora)));

    [Fact(DisplayName = "RN-13: el mes se conserva en frecuencia mensual")]
    public void Mes_Se_Conserva_En_Mensual() =>
        Assert.Equal((byte)6, MantenimientoPreventivo.Crear(1, Datos(Frecuencia.Mensual, 6), "ana", Ahora).Mes);

    [Fact(DisplayName = "RN-14: editar deja el MP en Modificado con usuario y fecha")]
    public void Editar_Deja_Modificado()
    {
        var mp = MantenimientoPreventivo.Crear(1, Datos(), "ana", Ahora);
        mp.Modificar(Datos(descripcion: "Otra"), "luis", Ahora.AddHours(1));
        Assert.Equal(EstadoMp.Modificado, mp.Estado);
        Assert.Equal("luis", mp.ModificadoPor);
        Assert.Equal(Ahora.AddHours(1), mp.ModificadoEn);
    }

    [Fact(DisplayName = "RN-15: aprobar dos veces da conflicto")]
    public void Aprobar_Dos_Veces_Da_Conflicto() =>
        Assert.Equal("mp.ya-aprobado", Codigo(() => MpAprobado().Aprobar("ana", Ahora)));

    [Fact(DisplayName = "RN-16: un MP modificado se puede aprobar de nuevo")]
    public void Modificado_Se_Puede_Aprobar()
    {
        var mp = MpAprobado();
        mp.Modificar(Datos(descripcion: "Cambio"), "ana", Ahora);
        mp.Aprobar("ana", Ahora);
        Assert.Equal(EstadoMp.Aprobado, mp.Estado);
    }

    [Fact(DisplayName = "RN-20: inicio igual a hoy se rechaza")]
    public void Inicio_Hoy_Se_Rechaza() =>
        Assert.Equal("fecha.inicio-pasada", Codigo(() => MpAprobado().ProgramarFecha(Hoy, Hoy, Hoy, "ana", Ahora)));

    [Fact(DisplayName = "RN-21: inicio mayor que fin se rechaza")]
    public void Inicio_Mayor_Que_Fin_Se_Rechaza() =>
        Assert.Equal("fecha.rango-invalido", Codigo(() => MpAprobado().ProgramarFecha(Hoy.AddDays(5), Hoy.AddDays(2), Hoy, "ana", Ahora)));

    [Fact(DisplayName = "RN-22: MP no aprobado no admite fechas")]
    public void Mp_No_Aprobado_No_Admite_Fechas() =>
        Assert.Equal("mp.no-aprobado", Codigo(() =>
            MantenimientoPreventivo.Crear(1, Datos(), "ana", Ahora).ProgramarFecha(Hoy.AddDays(1), Hoy.AddDays(1), Hoy, "ana", Ahora)));

    [Fact(DisplayName = "RN-23: no se repite el inicio de una fecha")]
    public void Fecha_Repetida_Da_Conflicto()
    {
        var mp = MpAprobado();
        mp.ProgramarFecha(Hoy.AddDays(3), Hoy.AddDays(3), Hoy, "ana", Ahora);
        Assert.Equal("fecha.duplicada", Codigo(() => mp.ProgramarFecha(Hoy.AddDays(3), Hoy.AddDays(4), Hoy, "ana", Ahora)));
    }

    [Fact(DisplayName = "RN-24: programar actualiza las fechas del MP y la fecha nace sin OT")]
    public void Programar_Actualiza_Fechas_Del_Mp()
    {
        var mp = MpAprobado();
        var f = mp.ProgramarFecha(Hoy.AddDays(3), Hoy.AddDays(5), Hoy, "ana", Ahora);
        Assert.False(f.TieneOt);
        Assert.Equal(Hoy.AddDays(3), mp.FechaInicio);
        Assert.Equal(Hoy.AddDays(3), mp.FechaProxima);
        Assert.Equal(Hoy.AddDays(5), mp.FechaUltima);
    }

    [Fact(DisplayName = "RN-30/31/35: la OT nace Pendiente, Alta y Preventivo, con la fecha enlazada")]
    public void Generar_Ot_Copia_Datos_Y_Enlaza()
    {
        var mp = MpAprobado();
        var f = mp.ProgramarFecha(Hoy.AddDays(3), Hoy.AddDays(3), Hoy, "ana", Ahora);
        var ot = OrdenTrabajo.Generar(mp, f, 7, "ana", Ahora);
        Assert.Equal(7, ot.Numero);
        Assert.Equal(EstadoOt.Pendiente, ot.Estado);
        Assert.Equal(PrioridadOt.Alta, ot.Prioridad);
        Assert.Equal(TipoOt.Preventivo, ot.Tipo);
        Assert.Equal(mp.Descripcion, ot.Descripcion);
        Assert.Equal(Hoy.AddDays(3), ot.FechaObjetivo);
        Assert.True(f.TieneOt);
    }

    [Fact(DisplayName = "RN-30: una fecha con OT no genera otra")]
    public void Fecha_Con_Ot_No_Genera_Otra()
    {
        var mp = MpAprobado();
        var f = mp.ProgramarFecha(Hoy.AddDays(3), Hoy.AddDays(3), Hoy, "ana", Ahora);
        OrdenTrabajo.Generar(mp, f, 1, "ana", Ahora);
        Assert.Equal("fecha.con-ot", Codigo(() => OrdenTrabajo.Generar(mp, f, 2, "ana", Ahora)));
        Assert.Equal("fecha.con-ot", Codigo(() => f.ValidarEliminable()));
    }

    private static OrdenTrabajo OtPendiente()
    {
        var mp = MpAprobado();
        var f = mp.ProgramarFecha(Hoy.AddDays(3), Hoy.AddDays(3), Hoy, "ana", Ahora);
        return OrdenTrabajo.Generar(mp, f, 1, "ana", Ahora);
    }

    [Theory(DisplayName = "RN-37: transiciones permitidas desde Pendiente")]
    [InlineData(EstadoOt.Concluido)]
    [InlineData(EstadoOt.Anulacion)]
    public void Transiciones_Permitidas_Desde_Pendiente(EstadoOt nuevo)
    {
        var ot = OtPendiente();
        ot.CambiarEstado(nuevo, "luis", Ahora);
        Assert.Equal(nuevo, ot.Estado);
        Assert.Single(ot.Historial);
    }

    [Fact(DisplayName = "RN-37: reapertura Concluido → Pendiente")]
    public void Reapertura_Se_Permite()
    {
        var ot = OtPendiente();
        ot.CambiarEstado(EstadoOt.Concluido, "luis", Ahora);
        ot.CambiarEstado(EstadoOt.Pendiente, "luis", Ahora);
        Assert.Equal(EstadoOt.Pendiente, ot.Estado);
        Assert.Equal(2, ot.Historial.Count);
    }

    [Theory(DisplayName = "RN-37: transiciones prohibidas dan conflicto")]
    [InlineData(EstadoOt.Solicitud)]
    [InlineData(EstadoOt.EnProgramacion)]
    [InlineData(EstadoOt.ConcluidoPorAnulacion)]
    public void Transiciones_Prohibidas_Desde_Pendiente(EstadoOt nuevo) =>
        Assert.Equal("ot.transicion-invalida", Codigo(() => OtPendiente().CambiarEstado(nuevo, "luis", Ahora)));

    [Fact(DisplayName = "RN-38: el historial guarda usuario y fecha")]
    public void Historial_Guarda_Usuario_Y_Fecha()
    {
        var ot = OtPendiente();
        ot.CambiarEstado(EstadoOt.Concluido, "luis", Ahora.AddDays(1));
        var h = ot.Historial.Single();
        Assert.Equal((EstadoOt.Pendiente, EstadoOt.Concluido, "luis", Ahora.AddDays(1)),
            (h.EstadoAnterior, h.EstadoNuevo, h.Usuario, h.Fecha));
    }

    [Fact(DisplayName = "RN-25: quitar una fecha sin OT la saca del MP; una inexistente da no encontrada")]
    public void Quitar_Fecha()
    {
        var mp = MpAprobado();
        mp.ProgramarFecha(Hoy.AddDays(3), Hoy.AddDays(3), Hoy, "ana", Ahora);
        var f = mp.Fechas.Single();
        Assert.Same(f, mp.QuitarFecha(f.Id));
        Assert.Empty(mp.Fechas);
        Assert.Equal("fecha.no-encontrada", Codigo(() => mp.QuitarFecha(12345)));
    }

    [Fact(DisplayName = "RN-37: el mensaje de transición prohibida usa nombres legibles")]
    public void Mensaje_De_Transicion_Legible()
    {
        var ex = Assert.Throws<DominioException>(() => OtPendiente().CambiarEstado(EstadoOt.EnProgramacion, "luis", Ahora));
        Assert.Equal("No se puede pasar de «Pendiente» a «En programación».", ex.Message);
    }
}
