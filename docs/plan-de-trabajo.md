# Plan de trabajo del proyecto integrador: SISMAN Mantenimiento

# 1. Objetivo y supuestos

Entregar, como proyecto integrador del curso *Agentic Software Development (.NET 10)*, **una API REST del flujo mantenimiento preventivo (MP) → fecha programada → orden de trabajo (OT)**, especificada primero, con trazabilidad, pruebas, evidencia de uso de IA y presentación.

| Supuesto | Valor | Estado |
|---|---|---|
| Fecha de entrega y de presentación | — | **Por confirmar** con el instructor |
| Individual o grupal | — | **Por confirmar** |
| Tema y alcance (modernización de un sistema real, un módulo) | ADR-002 | Aceptado por el responsable; **por confirmar** con el instructor |
| Acceso a datos | EF Core con esquema propio en SQL Server local o LocalDB (ADR-001) | Aceptado por el responsable |
| Base de partida | `docs/alcance.md`, `specs/functional/requisitos.md` v0.1 (12 RF, 6 RNF), 6 evidencias de IA | Hecho el 2026-10-22 |
| Estimación | **26 a 33 días-persona** (±30 %) | Ver sección 4 |

Este plan es la ruta que sigue el curso: *especificar → diseñar → implementar → probar → evidenciar*.

# 2. Punto de partida (2026-10-22)

| Fase del curso | Estado | Qué falta |
|---|---|---|
| 0 Alcance | Casi cerrada | Respuesta del instructor, Docker disponible, rotar la clave expuesta del repositorio de modernización |
| 1 Especificación | Parcial: alcance y requisitos en borrador | Historias, casos de uso, reglas `RN-xx`, specs técnicas, matriz completa |
| 2 Arquitectura | ADR-001 y ADR-002 | Modelo de dominio, ERD, C4, otros 2 ADR, OpenAPI borrador |
| 3 a 5 | No empezadas | Todo el código y las pruebas |
| 6 Evidencia de IA | 6 entradas | Llegar a 10 a 15, con capturas, y mantenerlas al día |
| 7 a 9 | No empezadas | README final, documento técnico, presentación, revisión |

## 2.1 Entorno de desarrollo

| Herramienta | Estado | Acción |
|---|---|---|
| .NET SDK 10 | Instalado (10.0.402) | — |
| `dotnet-ef` | Instalado en la versión 6.0.8 | **Actualizar a la versión 10** (`dotnet tool update -g dotnet-ef`) |
| **Docker** | Descartado el 2026-10-26 (ADR-007) | Sin Docker: SQL Server local o LocalDB |
| SQL Server | Hay instancias locales de SQL Server en el equipo | Se pueden usar como plan B para desarrollo; no se tocan sin indicarlo, y las pruebas de integración deben correr contra un contenedor |

# 3. Plan por fases

Estados: ☐ pendiente · ◐ en curso · ☑ hecho. Los días son por persona. «Peso» es el criterio de la nota que protege la fase.

## Fase 0 — Cierre del alcance (1 día)

| ID | Tarea | Entregable | Días | Depende de |
|---|---|---|---:|---|
| C-01 | **Rotar la clave `sa`** expuesta en el repositorio de modernización (la hace TI) | Confirmación de TI | 0,25 + espera | — |
| C-02 | Enviar la consulta al instructor y registrar las respuestas | `docs/consulta-instructor.md` con respuestas | 0,25 | — |
| C-03 | Cancelada (ADR-007): Docker no se usa en el proyecto | Captura en `docs/ia/capturas/` | 0,5 | — |
| C-04 | Congelar el alcance con la respuesta del instructor | `docs/alcance.md` v1.0 | 0,25 | C-02 |

**Hecho cuando:** el instructor respondió, Docker corre y el alcance quedó congelado.

## Fase 1 — Especificación SDD (4 a 5 días) · peso 25 %

| ID | Tarea | Entregable | Días | Depende de |
|---|---|---|---:|---|
| C-10 | Historias de usuario con criterios Given/When/Then para los 12 RF (RF `M` primero) | `specs/functional/historias.md` | 1,5 | C-04 |
| C-11 | Casos de uso de los RF de prioridad `M` (9 de 12) | `specs/functional/casos-de-uso.md` | 1 | C-10 |
| C-12 | Reglas de negocio y validaciones `RN-xx`, cada una con su origen y su prueba prevista | `specs/functional/reglas.md` | 1 | C-10 |
| C-13 | Validar con un usuario los 3 puntos abiertos: efecto de aprobar un MP, flujo de estados de la OT, reapertura de una OT concluida | Acta breve en `specs/functional/` | 0,5 + espera | C-12 |
| C-14 | Specs técnicas: stack, endpoints previstos, modelo de datos, formato de errores, seguridad | `specs/technical/` | 1,5 | C-12 |
| C-15 | Matriz de trazabilidad: RF → historia → caso de uso → endpoint | `docs/trazabilidad.md` | 0,5 | C-11, C-14 |

**Hecho cuando:** cada RF tiene historia con criterios verificables y aparece en la matriz.

**Endpoints previstos** (a refinar en C-14):

| Recurso | Operaciones | RF |
|---|---|---|
| `/api/v1/auth/login` | POST | RF-11 |
| `/api/v1/mantenimientos-preventivos` | POST, GET (lista y filtros), GET por id, PUT | RF-01, RF-02, RF-03 |
| `/api/v1/mantenimientos-preventivos/{id}/aprobacion` | POST | RF-04 |
| `/api/v1/mantenimientos-preventivos/{id}/fechas` | POST, DELETE | RF-05, RF-06 |
| `/api/v1/fechas/{id}/ordenes-trabajo` | POST | RF-07 |
| `/api/v1/ordenes-trabajo` | GET (lista, filtros, paginación), GET por id | RF-08 |
| `/api/v1/ordenes-trabajo/{id}/estado` | PUT | RF-09 |
| `/api/v1/planning` | GET | RF-10 |
| `/api/v1/ordenes-trabajo/exportacion` | GET (Excel) | RF-12 |

## Fase 2 — Arquitectura y diseño (2 días) · peso 20 %

| ID | Tarea | Entregable | Días |
|---|---|---|---:|
| C-20 | Modelo de dominio y ERD | `docs/arquitectura/modelo.md` | 0,5 |
| C-21 | Diagramas C4 (contexto y contenedores) | `docs/arquitectura/c4-*.md` | 0,5 |
| C-22 | ADR-003 validación (FluentValidation), ADR-004 errores (ProblemDetails), ADR-005 autenticación (JWT), ADR-006 número de OT (transacción y unicidad) | `docs/adr/` (6 en total) | 1 |
| C-23 | OpenAPI borrador | `docs/api/openapi-borrador.yaml` | por C-14 |

**Hecho cuando:** hay 4 o más ADR y el diseño es coherente con las specs.

## Fase 3 — Implementación base (3 a 4 días)

| ID | Tarea | Entregable | Días |
|---|---|---|---:|
| C-30 | Solución .NET 10 con Domain, Application, Infrastructure, Api y Tests | `src/` compilable | 0,5 |
| C-31 | Entidades, reglas de dominio y puertos (MP, fecha, OT, estados, correlativo) | `Domain` | 1 |
| C-32 | EF Core: `DbContext`, configuraciones, migración inicial, restricción de unicidad del número de OT, datos de ejemplo | `Infrastructure` | 1,5 |
| C-33 | Cancelada (ADR-007): arranque con SQL Server local o LocalDB en el README | `README.md` | 0,5 |
| C-34 | Manejo global de errores (ProblemDetails), identificador de correlación | `Api` | 0,5 |

**Hecho cuando:** la migración y `dotnet build` corren desde cero siguiendo el README (sin Docker, ADR-007).

## Fase 4 — Funcionalidad por incrementos (7 a 9 días) · pesos 25 % + parte del 20 %

Se repite por cada RF: spec → prompt con la spec → revisar el diff → prueba → commit `tipo(RF-xx)` → evidencia de IA → matriz.

| Orden | RF | Contenido | Días |
|---:|---|---|---:|
| 1 | RF-11 | Login y JWT (base de todo) | 1 |
| 2 | RF-01, 02, 03 | Alta, consulta y edición de MP, con código correlativo y auditoría | 1,5 |
| 3 | RF-04 | Aprobación | 0,5 |
| 4 | RF-05, 06 | Programar y eliminar fechas (inicio posterior a hoy) | 1 |
| 5 | RF-07 | **Generar OT con número transaccional y único** | 1,5 |
| 6 | RF-08 | Consulta paginada de OT | 1 |
| 7 | RF-09 | Cambio de estado | 1 |
| 8 | RF-10 | Planning | 0,5 |
| 9 | RF-12 | Exportación a Excel (si hay tiempo) | 0,5 |
| — | OpenAPI/Swagger con descripciones y ejemplos | Todos los endpoints | 0,5 |

**Hecho cuando:** todos los RF de prioridad `M` funcionan y están marcados en la matriz.

## Fase 5 — Pruebas y calidad (3 a 4 días) · pesos 10 % + calidad del 20 %

| ID | Tarea | Entregable | Días |
|---|---|---|---:|
| C-50 | Pruebas unitarias de dominio y casos de uso (xUnit) | `tests/*.UnitTests` | 1,5 |
| C-51 | Pruebas de integración de la API con `WebApplicationFactory` y un SQL Server real en contenedor; **prueba concurrente del número de OT** (20 solicitudes simultáneas, sin duplicados) | `tests/*.IntegrationTests` | 1,5 |
| C-52 | Un caso de prueba por criterio de aceptación; matriz actualizada | `docs/trazabilidad.md` | 0,5 |
| C-53 | Revisión de código y búsqueda de secretos en archivos e historial | `docs/calidad.md` | 0,5 |
| C-54 | Colección de peticiones (archivo HTTP) | `docs/api/` | 0,25 |

**Hecho cuando:** pruebas en verde y cada RF enlazado a al menos una prueba.

## Fase 6 — Evidencia de IA (continua, unos 2 días en total) · peso 15 %

| Regla | Detalle |
|---|---|
| Cuándo | Al terminar cada tarea (5 minutos) |
| Qué | Prompt, respuesta, diff, **aceptado, corregido o rechazado y por qué**, revisión humana |
| Meta | 10 a 15 entradas variadas: código, refactor, debugging, pruebas, arquitectura, documentación (hoy hay 6) |
| Capturas | `docs/ia/capturas/` (las toma el responsable) |
| Cierre | `docs/ia/resumen.md` con dónde ayudó, errores corregidos y aprendizajes |

## Fase 7 — Documentación final (1 a 2 días)

README completo (resumen, stack, ejecución, pruebas, credenciales de ejemplo, evidencia de IA), documento técnico (se apoya en el análisis previo del sistema), OpenAPI definitivo exportado.

## Fase 8 — Presentación y ensayo (2 días) · peso 5 %

PPT de 10 a 15 minutos (problema 1′ · specs y trazabilidad 2′ · arquitectura 2′ · IA y revisión 2′ · demo 4′ · aprendizajes 1′), demo con datos de ejemplo, plan B con capturas o video, ensayo cronometrado y preguntas esperadas («¿por qué esta decisión?», «explica este código»).

## Fase 9 — Revisión final (1 día)

- ☐ Clonar en carpeta limpia y seguir el README: compila y ejecuta sin pasos ocultos.
- ☐ Todas las pruebas pasan.
- ☐ OpenAPI cubre los endpoints principales.
- ☐ Matriz de trazabilidad completa.
- ☐ Evidencias de IA con revisión humana visible.
- ☐ Búsqueda de secretos en archivos e historial.
- ☐ Presentación y demo ensayadas.
- ☐ Entrega en el medio indicado (por confirmar).

# 4. Esfuerzo y calendario

## 4.1 Totales

| Fase | Días (mín. a máx.) |
|---|---:|
| 0 | 1 |
| 1 | 4 a 5 |
| 2 | 2 |
| 3 | 3 a 4 |
| 4 | 7 a 9 |
| 5 | 3 a 4 |
| 6 (continua) | 2 a 3 |
| 7 | 1 a 2 |
| 8 | 2 |
| 9 | 1 |
| **Total** | **26 a 33** |

Esta estimación cuenta también la evidencia de IA y la prueba concurrente del número de OT.

## 4.2 Escenarios (sin fecha de entrega confirmada)

| Escenario | Duración aproximada |
|---|---|
| 1 persona al 100 % | 6 a 7 semanas |
| 1 persona al 70 % | 8 a 9 semanas |
| 2 personas al 70 % | 4 a 5 semanas |

## 4.3 Si hay menos tiempo: qué recortar

Como pide el plan de trabajo del curso, **se reduce alcance, no se omiten specs ni pruebas**. Orden de recorte:

1. RF-12 (exportación), prioridad `C`.
2. RF-10 (planning), prioridad `S`.
3. RF-06 (eliminar fecha), prioridad `S`.
4. RF-09: solo las transiciones principales.
5. RF-01 (alta de MP): el usuario dijo que el alta se hace en otro sistema; puede cargarse desde datos de ejemplo.

Nunca se recortan: RF-05, RF-07 (el núcleo y la razón de ser del módulo), RF-08, RF-11 y la prueba concurrente.

## 4.4 Cronograma con base de 6 semanas

| Semana | Foco | Hito | Peso que protege |
|---|---|---|---|
| 1 | Fase 0 y Fase 1 | Alcance congelado; historias, reglas y matriz | Specs 25 % |
| 2 | Fases 1 y 2 | Specs técnicas, modelo, C4, 4 o más ADR, OpenAPI borrador | Specs 25 % + Arquitectura 20 % |
| 3 | Fase 3 y primeros RF | Solución y migración corriendo; RF-11, RF-01 a 04 | Funcionalidad 25 % |
| 4 | Fase 4 | RF-05 a RF-09 con pruebas | Funcionalidad + Pruebas 10 % |
| 5 | Fases 5, 6 y 7 | Integración, concurrencia, calidad, evidencias de IA, README | Pruebas + IA 15 % |
| 6 | Fases 8 y 9 | Documento técnico, PPT, ensayo, revisión final | Presentación 5 % |

# 5. Riesgos y mitigaciones

| Riesgo | Efecto | Mitigación |
|---|---|---|
| El instructor no acepta una modernización o un repositorio con material previo | Rehacer el tema | Preguntar antes de construir (C-02) |
| Docker no se puede instalar (permisos o licencia de la empresa) | No hay pruebas de integración con SQL Server real | Plan B con una instancia local de SQL Server |
| Alcance excesivo | No se cierra | Congelar RF en C-04; orden de recorte (4.3) |
| «Vibe coding» por prisa | Pierde el peso de specs y trazabilidad | Regla: no se pide código sin una spec o historia referenciada |
| Evidencias de IA dejadas para el final | Pierde el 15 % | Registrar al terminar cada tarea |
| Código que no se sabe explicar | Falla en la presentación | Revisar cada diff y poder justificar cada ADR |
| Secretos | Falla un criterio explícito | `.gitignore`, ejemplo de configuración, búsqueda en la Fase 9. El repositorio nuevo no contiene archivos del sistema original |
| Falla de la demo | — | Datos de ejemplo, plan B con video y capturas |
| La prueba concurrente del correlativo es difícil de estabilizar | Prueba intermitente | Restricción de unicidad en la base más reintento controlado; documentarlo en ADR-006 |
| Validación con el usuario no disponible a tiempo (C-13) | Reglas sin confirmar | Marcarlas como supuestos en las specs y documentarlo |

# 6. Rutina por tarea

1. Identificar el RF o la historia.
2. Dar a la IA la spec como contexto.
3. Revisar y corregir el diff.
4. Prueba asociada.
5. Commit `tipo(RF-xx): descripción`.
6. Entrada en `docs/ia/`.
7. Actualizar la matriz de trazabilidad.

# 7. Qué necesito de usted

| # | Acción | Cuándo |
|---|---|---|
| 1 | Pedir a TI la rotación de la clave `sa` (C-01) | Hoy |
| 2 | Revisar y enviar la consulta al instructor (C-02) | Hoy |
| 3 | Decidir cómo habilitar Docker (C-03) | Resuelto: no se usa Docker (ADR-007), 2026-10-26 |
| 4 | Indicar si el trabajo es individual o grupal y la fecha de entrega, cuando el instructor responda | Cuando responda |
| 5 | Conseguir 30 minutos con un usuario para validar C-13 | Semana 1 |
| 6 | Tomar las capturas de pantalla para las evidencias | Continuo |

*Historial de versiones: v1.0 — plan del proyecto integrador (2026-10-22).*
