# Modelo de dominio y ERD

Estado: **borrador v0.1** (2026-10-23). Esquema propio del módulo, creado por migraciones de EF Core (ADR-001). No es una copia de la base del sistema original.

## 1. Modelo de dominio

Agregados (cada uno se modifica solo a través de su raíz):

| Agregado | Raíz | Contenido | Reglas que protege |
|---|---|---|---|
| Mantenimiento preventivo | `MantenimientoPreventivo` | Datos del MP, estado, auditoría y sus `FechaProgramada` | RN-10 a RN-16, RN-20 a RN-25 |
| Orden de trabajo | `OrdenTrabajo` | Datos copiados del MP, estado e historial de estados | RN-30 a RN-38 |
| Correlativo | `Correlativo` | Último valor por nombre (`OT`, `MP`) | RN-10, RN-32 a RN-34 |
| Usuario | `Usuario` | Nombre, hash de clave, perfil | RN-01 a RN-03 |
| Catálogos | `Catalogo`, `Planta`, `Area`, `Maquina`, `Sistema`, `Equipo` | Datos de apoyo, solo lectura para la API | RN-11 |

Objetos de valor y enumeraciones: `CodigoMp` (`MP-` + 7 dígitos), `Frecuencia` (1 Diario, 2 Semanal, 3 Mensual, 4 Anual), `EstadoMp` (Nuevo, Modificado, Aprobado), `EstadoOt` (37 a 42, RN-36), `Perfil` (Planificador, Especialista).

Comportamiento en el dominio (no en los controladores):

- `MantenimientoPreventivo.Aprobar(usuario, ahora)`, `Modificar(...)`, `ProgramarFecha(inicio, fin, hoy, usuario)` → valida RN-20 a RN-24 y actualiza las fechas del MP (RN-24).
- `FechaProgramada.EnlazarOt(numero)` → falla si ya tiene OT (RN-30).
- `OrdenTrabajo.Generar(mp, fecha, numero, usuario, ahora)` → fábrica que copia los datos (RN-31, RN-35).
- `OrdenTrabajo.CambiarEstado(nuevo, usuario, ahora)` → aplica la tabla de transiciones (RN-37) y agrega el historial (RN-38).

## 2. ERD

```mermaid
erDiagram
    USUARIO {
        int Id PK
        nvarchar NombreUsuario UK
        nvarchar ClaveHash
        tinyint Perfil
        bit Activo
    }
    CATALOGO {
        int Id PK
        tinyint Categoria
        nvarchar Nombre
    }
    PLANTA { int Id PK
        nvarchar Nombre }
    AREA { int Id PK
        int PlantaId FK
        nvarchar Nombre }
    MAQUINA { int Id PK
        int AreaId FK
        nvarchar Codigo UK
        nvarchar Nombre }
    SISTEMA { int Id PK
        int MaquinaId FK
        nvarchar Nombre }
    EQUIPO { int Id PK
        int SistemaId FK
        nvarchar Nombre }
    MANTENIMIENTO_PREVENTIVO {
        int Id PK
        nvarchar Codigo UK
        nvarchar Descripcion
        int MaquinaId FK
        int SistemaId FK
        int EquipoId FK
        int TipoTrabajoId FK
        int FuenteId FK
        int OportunidadId FK
        int EspecialidadId FK
        tinyint Frecuencia
        int ValorFrecuencia
        tinyint Mes
        tinyint Estado
        date FechaInicio
        date FechaProxima
        date FechaUltima
        datetime2 CreadoEn
        nvarchar CreadoPor
        datetime2 ModificadoEn
        nvarchar ModificadoPor
        datetime2 AprobadoEn
        nvarchar AprobadoPor
        rowversion Version
    }
    FECHA_PROGRAMADA {
        int Id PK
        int MantenimientoPreventivoId FK
        date FechaInicio
        date FechaFin
        int OrdenTrabajoId FK
        datetime2 CreadoEn
        nvarchar CreadoPor
    }
    ORDEN_TRABAJO {
        int Id PK
        int Numero UK
        tinyint Tipo
        tinyint Prioridad
        tinyint Estado
        nvarchar Descripcion
        int MaquinaId FK
        int SistemaId FK
        int EquipoId FK
        int TipoTrabajoId FK
        int FuenteId FK
        int OportunidadId FK
        int EspecialidadId FK
        int MantenimientoPreventivoId FK
        date FechaObjetivo
        datetime2 CreadoEn
        nvarchar CreadoPor
        datetime2 ModificadoEn
        nvarchar ModificadoPor
        rowversion Version
    }
    ORDEN_TRABAJO_HISTORIAL {
        int Id PK
        int OrdenTrabajoId FK
        tinyint EstadoAnterior
        tinyint EstadoNuevo
        nvarchar Usuario
        datetime2 Fecha
    }
    CORRELATIVO {
        nvarchar Nombre PK
        int UltimoValor
    }

    PLANTA ||--o{ AREA : tiene
    AREA ||--o{ MAQUINA : tiene
    MAQUINA ||--o{ SISTEMA : tiene
    SISTEMA ||--o{ EQUIPO : tiene
    MAQUINA ||--o{ MANTENIMIENTO_PREVENTIVO : "se mantiene con"
    CATALOGO ||--o{ MANTENIMIENTO_PREVENTIVO : clasifica
    MANTENIMIENTO_PREVENTIVO ||--o{ FECHA_PROGRAMADA : programa
    FECHA_PROGRAMADA |o--o| ORDEN_TRABAJO : genera
    MANTENIMIENTO_PREVENTIVO ||--o{ ORDEN_TRABAJO : origina
    ORDEN_TRABAJO ||--o{ ORDEN_TRABAJO_HISTORIAL : registra
```

`CATALOGO.Categoria` distingue cuatro listas: 1 Tipo de trabajo, 2 Fuente de información, 3 Oportunidad de intervención, 4 Especialidad. Una sola tabla evita cuatro tablas idénticas.

## 3. Restricciones que garantizan las reglas

| Restricción | Regla / RNF |
|---|---|
| `UNIQUE (ORDEN_TRABAJO.Numero)` | RN-32, RNF-02 |
| `UNIQUE (MANTENIMIENTO_PREVENTIVO.Codigo)` | RN-10 |
| `UNIQUE (FECHA_PROGRAMADA.MantenimientoPreventivoId, FechaInicio)` | RN-23 |
| `UNIQUE (FECHA_PROGRAMADA.OrdenTrabajoId)` filtrado `WHERE OrdenTrabajoId IS NOT NULL` | Una OT nace de una sola fecha (RN-30) |
| `CHECK (Frecuencia BETWEEN 1 AND 4)`, `CHECK (ValorFrecuencia > 0)` | RN-12 |
| `CHECK (Frecuencia NOT IN (3,4) OR Mes BETWEEN 1 AND 12)` | RN-13 |
| `CHECK (FechaInicio <= FechaFin)` en `FECHA_PROGRAMADA` | RN-21 |
| `rowversion` en MP y OT | Dos ediciones a la vez → 409 en vez de pisarse |
| Claves foráneas en todas las relaciones | Integridad que el sistema original no tenía (4 tablas sin clave primaria) |
| Índice `IX_OT_Estado_Numero (Estado, Numero DESC)` | RN-40, RN-41, RNF-01 |

## 4. Correlativo transaccional (RN-33, RN-34)

Se toma el número en la misma transacción que crea la OT y enlaza la fecha:

```sql
UPDATE Correlativo WITH (UPDLOCK, ROWLOCK)
   SET UltimoValor = UltimoValor + 1
OUTPUT inserted.UltimoValor
 WHERE Nombre = 'OT';
```

La instrucción bloquea la fila hasta el `COMMIT`: 20 solicitudes simultáneas se ponen en cola y reciben números distintos y consecutivos. Si algo falla antes del `COMMIT`, el `ROLLBACK` devuelve el número y no quedan datos a medias. La restricción `UNIQUE` sobre `Numero` es la segunda defensa. Se ejecuta con `ExecuteSqlInterpolated` (parametrizado), no con texto concatenado (RNF-03).

## 5. Datos de ejemplo (seed)

2 usuarios (un planificador y un especialista, claves de prueba documentadas en el README), 1 planta, 3 áreas, 8 máquinas con sus sistemas y equipos, los 4 catálogos, y unos 30 MP en estados variados (la alta de MP queda fuera de la implementación, RF-01) para poder probar RF-02 a RF-09.
