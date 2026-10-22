# Requisitos del módulo

Estado: **borrador v0.2** (historias en `historias.md`, casos de uso en `casos-de-uso.md` y reglas en `reglas.md`). Se congela al terminar la Fase 1. Lo que no esté aquí va a «mejoras pendientes».

Origen de cada requisito: análisis del sistema original (`reglas-negocio`, `inventario-funcional`) y validación con un usuario real (2026-10-20). Se citan con la etiqueta **Origen**.

Prioridad (MoSCoW): **M** imprescindible · **S** deseable · **C** si hay tiempo.

## Requisitos funcionales

| ID | Requisito | Prioridad | Actor | Origen |
|---|---|:-:|---|---|
| RF-01 | **Registrar un MP** con código correlativo (`MP-0000001`), descripción, máquina/sistema/equipo, frecuencia, tipo de OT, fuente de información, oportunidad de intervención y especialidad. El código se asigna automáticamente | M | Planificador | MP-1, MP-4 |
| RF-02 | **Consultar y buscar MP** por código, tipo y rango de fechas, con paginación | M | Planificador | Inventario funcional |
| RF-03 | **Editar un MP**, registrando quién y cuándo lo modificó. Un MP editado queda en estado «modificado» | M | Planificador | MP-3 |
| RF-04 | **Aprobar un MP.** Solo un MP aprobado admite programar fechas | M | Planificador | MP-5 (a validar) |
| RF-05 | **Programar una fecha** para un MP: una fecha de inicio y fin. La fecha de inicio debe ser **posterior a hoy**; al crearla se actualizan las fechas del MP | M | Planificador | FE-1, FE-3, FE-4, FE-8 |
| RF-06 | **Eliminar una fecha programada** que todavía no tiene OT | S | Planificador | Inventario funcional |
| RF-07 | **Generar una OT desde una fecha programada.** Nace con prioridad *Alta* y estado *Pendiente*; su número viene de un correlativo **dentro de una transacción**; la fecha queda enlazada a la OT | M | Planificador | OT-1, OT-4, OT-8, hallazgo 1 |
| RF-08 | **Consultar y buscar OT** por número, tipo, estado y rango de fechas, con paginación en el servidor | M | Planificador, Especialista | Inventario funcional |
| RF-09 | **Cambiar el estado de una OT** entre *Solicitud, Anulación, En programación, Pendiente, Concluido, Concluido por anulación*. Se registra quién y cuándo | M | Planificador | OT-7, OT-10 |
| RF-10 | **Ver el planning:** cantidad de MP programados por día, por planta, área o máquina y especialidad | S | Planificador | FE-6 |
| RF-11 | **Iniciar sesión** y obtener un token; todas las operaciones exigen autenticación | M | Todos | Seguridad |
| RF-12 | **Exportar la lista de OT a Excel** con los filtros aplicados | C | Planificador | Validación con usuario |

## Requisitos no funcionales

| ID | Requisito | Medida |
|---|---|---|
| RNF-01 | **Rendimiento.** La consulta de OT responde paginada | Respuesta de la página de OT en menos de 2 s y menos de 200 KB con 26 000 OT |
| RNF-02 | **Integridad.** Dos usuarios que generan OT al mismo tiempo no obtienen el mismo número; el número de OT es único por restricción de la base | Prueba concurrente con 20 solicitudes simultáneas sin duplicados |
| RNF-03 | **Seguridad.** Autenticación JWT; entradas validadas; consultas parametrizadas; ningún secreto en el repositorio ni en el historial | Búsqueda de secretos sin hallazgos; ninguna consulta con texto concatenado |
| RNF-04 | **Errores consistentes.** Toda falla se devuelve como `ProblemDetails` sin detalles internos, y nunca se informa éxito si la operación falló | Pruebas de error por endpoint |
| RNF-05 | **Reproducibilidad.** Desde un clon limpio se levanta la base con datos de ejemplo, se compila y se prueba con comandos documentados | `docker compose up` y `dotnet test` sin pasos ocultos |
| RNF-06 | **Auditoría y trazabilidad.** Cada cambio guarda usuario y fecha; cada solicitud lleva un identificador de correlación en el registro | Revisión de registros en las pruebas de integración |

## Reglas de negocio

Están numeradas `RN-xx` en `reglas.md`, con su origen y su prueba prevista.

## Pendiente de validar con un usuario

- Efecto exacto de aprobar un MP (RF-04).
- Flujo real de estados de la OT y reapertura de una OT concluida (RF-09).
- Si la creación de MP se hace en este sistema o en otro (RF-01): el usuario dijo que se hace en el «sistema antiguo».
