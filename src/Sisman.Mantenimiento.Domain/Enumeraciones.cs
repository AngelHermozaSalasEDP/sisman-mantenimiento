namespace Sisman.Mantenimiento.Domain;

public enum Frecuencia : byte { Diario = 1, Semanal = 2, Mensual = 3, Anual = 4 }

public enum EstadoMp : byte { Nuevo = 1, Modificado = 2, Aprobado = 3 }

/// <summary>Estados de la OT; los valores son los códigos del sistema original (RN-36).</summary>
public enum EstadoOt : byte
{
    Solicitud = 37,
    Anulacion = 38,
    EnProgramacion = 39,
    Pendiente = 40,
    Concluido = 41,
    ConcluidoPorAnulacion = 42
}

public enum TipoOt : byte { Preventivo = 1 }

public enum PrioridadOt : byte { Alta = 1 }

public enum Perfil : byte { Planificador = 1, Especialista = 2 }

public enum CategoriaCatalogo : byte
{
    TipoTrabajo = 1,
    FuenteInformacion = 2,
    OportunidadIntervencion = 3,
    Especialidad = 4
}
