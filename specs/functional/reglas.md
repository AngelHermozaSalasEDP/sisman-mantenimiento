# Reglas de negocio y validaciones

Estado: **borrador v0.1**. Cada regla indica su **origen** y la **prueba prevista**. Los códigos del tipo `MP-3`, `FE-4`, `OT-7` y «hallazgo n» son etiquetas del análisis del sistema original (fuera de este repositorio).

Marcas: **[heredada]** reproduce el comportamiento del sistema original · **[corrige]** arregla un defecto del original · **[nueva]** no existía · **[a validar]** depende de la confirmación de un usuario (tarea C-13 del plan).

## Seguridad (RF-11)

| ID | Regla | Origen | Prueba prevista |
|---|---|---|---|
| RN-01 | Toda operación, salvo el inicio de sesión, exige un token válido; sin token responde 401 | [corrige] el original dejaba abiertas 9 de 11 pantallas | Cada endpoint sin token → 401 |
| RN-02 | El token vence a los 30 minutos | [nueva] | Token vencido → 401 |
| RN-03 | Hay dos perfiles: **Planificador** (todas las operaciones) y **Especialista** (solo consultas: RF-02, RF-08) | Validación con el usuario: el especialista registra avance, el planificador programa | Especialista que intenta crear una OT → 403 |

## Mantenimiento preventivo (RF-01 a RF-04)

| ID | Regla | Origen | Prueba prevista |
|---|---|---|---|
| RN-10 | El código del MP es `MP-` más 7 dígitos (`MP-0000001`), lo asigna el sistema, es correlativo y único | [heredada] MP-4 | Dos altas seguidas → códigos consecutivos y distintos |
| RN-11 | Obligatorios: descripción (hasta 200 caracteres), máquina, tipo de trabajo, fuente de información, oportunidad de intervención, especialidad y frecuencia | [heredada] MP-1; límites [nueva] | Alta sin descripción → 400 con el campo indicado |
| RN-12 | La frecuencia tiene **un solo catálogo**: 1 Diario, 2 Semanal, 3 Mensual, 4 Anual, con un valor entero mayor que cero | [corrige] FE-7: el alta y la lectura del original usaban códigos distintos | Alta con frecuencia 0 o 5 → 400 |
| RN-13 | Si la frecuencia es mensual o anual, el **mes** es obligatorio y se guarda | [corrige] MP-2: el original lo descartaba | Alta mensual → el mes se devuelve en la consulta |
| RN-14 | Editar un MP lo deja en estado **Modificado** y registra quién y cuándo. No se conserva la versión anterior | [heredada] MP-3 | Edición → estado «Modificado», usuario y fecha |
| RN-15 | Aprobar un MP lo pasa a **Aprobado** y registra quién y cuándo. Un MP ya aprobado no se aprueba de nuevo (409) | [heredada] MP-5; no cambia las fechas ya programadas [decidido 2026-10-26, D5] | Aprobar dos veces → 409 |
| RN-16 | Un MP **Modificado** se puede aprobar de nuevo | [decidido 2026-10-26: sí] | Edición y aprobación → estado «Aprobado» |

## Fechas programadas (RF-05, RF-06)

| ID | Regla | Origen | Prueba prevista |
|---|---|---|---|
| RN-20 | La fecha de inicio debe ser **posterior a hoy**; lo valida el servidor | [corrige] FE-8: el original lo validaba solo en el navegador | Inicio igual o anterior a hoy → 400 |
| RN-21 | La fecha de inicio no puede ser posterior a la de fin | [nueva] | Inicio mayor que fin → 400 |
| RN-22 | Solo se programa fechas para un MP **Aprobado** | MP-5 [a validar] | MP no aprobado → 409 |
| RN-23 | No se programan dos fechas con el mismo inicio para el mismo MP | [nueva] [decidido 2026-10-26: sí] | Segunda fecha igual → 409 |
| RN-24 | La fecha nace **sin OT**. Al crearla, el MP actualiza su fecha de inicio y la próxima con el inicio, y su fecha última con el fin | [heredada] FE-1, FE-4 | Alta de fecha → MP con esas fechas |
| RN-25 | Solo se elimina una fecha que **no tiene OT** | [heredada] estado 0 de FE-5 | Eliminar una fecha con OT → 409 |

## Órdenes de trabajo (RF-07, RF-09)

| ID | Regla | Origen | Prueba prevista |
|---|---|---|---|
| RN-30 | Una OT se genera desde una fecha **sin OT** y de un MP **Aprobado** | [heredada] FE-3 | Fecha con OT → 409 |
| RN-31 | La OT nace con prioridad **Alta**, estado **Pendiente**, tipo **Preventivo**; copia del MP la descripción, máquina, sistema, equipo, tipo de trabajo, fuente, oportunidad y especialidad | [heredada] OT-1, OT-8 | OT generada con esos valores |
| RN-32 | El número de OT es un entero positivo **único**, tomado de un correlativo | [heredada] OT-4; el original lo limitaba a 5 dígitos [corrige] | Restricción de unicidad en la base |
| RN-33 | Tomar el número, crear la OT, avanzar el correlativo y enlazar la fecha ocurre en **una transacción**: si algo falla, no queda nada a medias | [corrige] hallazgo 1 | Falla simulada en el último paso → ningún cambio |
| RN-34 | Con 20 solicitudes simultáneas se obtienen 20 números distintos y consecutivos | [corrige] hallazgo 1 | Prueba concurrente (RNF-02) |
| RN-35 | La fecha objetivo de la OT es el inicio de la fecha programada; la fecha queda enlazada (número de OT y estado «con OT») | [heredada] OT-3, FE-3 | Consulta de la fecha tras generar |
| RN-36 | Estados de la OT: 37 Solicitud, 38 Anulación, 39 En programación, 40 Pendiente, 41 Concluido, 42 Concluido por anulación | [heredada] OT-7 | Catálogo de estados |
| RN-37 | **Transiciones permitidas** (propuesta): Solicitud → En programación → Pendiente → Concluido; Pendiente → Anulación → Concluido por anulación; **Concluido → Pendiente** (reapertura). Cualquier otra → 409 | OT-10 (el original no tiene reglas) y la validación con el usuario sobre la reapertura. **[decidido 2026-10-26: la reapertura se permite]** | Una prueba por transición permitida y una por prohibida |
| RN-38 | Cada cambio de estado registra quién y cuándo | [nueva] | Historial del estado |

## Consulta (RF-02, RF-08, RF-10)

| ID | Regla | Origen | Prueba prevista |
|---|---|---|---|
| RN-40 | Las listas se paginan en el servidor: 50 por página por defecto, máximo 200 | [corrige] el original devolvía 26 000 filas (1,2 MB) | Pedir 500 → 400 o se limita a 200 |
| RN-41 | Las OT se ordenan por número **numérico** descendente | [corrige] hallazgo 6: el original ordenaba como texto | OT 9 000 antes que 999 |
| RN-42 | Los filtros se combinan (número, tipo, estado, rango de fechas) | [heredada] | Consulta con dos filtros |
| RN-43 | El planning cuenta solo las fechas **sin OT**, por día, con filtros de planta, área, máquina y especialidad | [heredada] FE-6 | Contar días de un mes |

## Errores y datos (transversales)

| ID | Regla | Origen | Prueba prevista |
|---|---|---|---|
| RN-50 | Todo error se devuelve como `ProblemDetails` (RFC 9457) con un código estable y sin detalles internos | [corrige] el original mostraba el texto de la excepción | Cualquier error 4xx/5xx |
| RN-51 | Nunca se informa éxito si la operación falló | [corrige] hallazgo 12 | Falla de base de datos → 500, no 200 |
| RN-52 | Las fechas viajan en formato ISO 8601 y no dependen de la cultura del servidor | [corrige] hallazgo 4 | Servidor con otra cultura → mismas fechas |
| RN-53 | Cada alta y cada cambio guardan usuario y fecha (UTC) | [nueva] RNF-06 | Revisión del registro |
| RN-54 | Cada solicitud lleva un identificador de correlación que aparece en el registro | [nueva] RNF-06 | Cabecera `X-Correlation-Id` |
