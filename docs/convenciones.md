# Convenciones de trabajo

## Commits

Formato: `tipo(ámbito): descripción en imperativo`. El ámbito es el requisito, historia o decisión que cubre el cambio.

| Tipo | Uso | Ejemplo |
|---|---|---|
| `feat` | funcionalidad nueva | `feat(RF-07): generar OT con correlativo transaccional` |
| `fix` | corrección | `fix(RF-05): rechazar fecha de inicio anterior a hoy` |
| `test` | pruebas | `test(RF-07): prueba concurrente del correlativo` |
| `docs` | documentación y specs | `docs(RF-01): historias de usuario del alta de MP` |
| `refactor` | cambio interno sin cambiar comportamiento | `refactor(RF-08): extraer filtro de OT` |
| `chore` | herramientas, configuración | `chore(docker): compose con SQL Server` |
| `adr` | decisión de arquitectura | `adr(ADR-001): usar EF Core` |

Todo commit lleva, al final, la línea de coautoría cuando intervino un asistente de IA.

## Flujo por tarea

1. Identificar el requisito o la historia.
2. Dar a la IA la especificación como contexto; **no se pide código sin spec**.
3. Revisar y corregir el diff.
4. Escribir o ajustar la prueba asociada.
5. Commit vinculado al requisito.
6. Entrada en `docs/ia/` (qué se pidió, qué se aceptó, qué se corrigió).
7. Actualizar `docs/trazabilidad.md`.

## Reglas

- **Sin secretos.** Ni claves, ni tokens, ni cadenas de conexión reales. Se usan variables de entorno o *user-secrets*, y `appsettings.example.json` con marcadores.
- **Una rama por etapa** y revisión antes de integrar a `main`.
- **Nada de historial reescrito** sin acuerdo explícito.
- **Alcance congelado** al final de la Fase 1: lo extra va a mejoras pendientes.
- Los nombres del dominio van en español (como los usan los usuarios); los de infraestructura, en inglés.
