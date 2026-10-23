# 008 — Modelo, C4, ADR-003 a ADR-006, OpenAPI borrador y specs técnicas

- **Fecha:** 2026-10-23
- **Tarea / requisito:** Fase 2 del plan (modelo y ERD, C4, ADR, OpenAPI) y tarea C-14 (specs técnicas); RF-01 a RF-12
- **Tipo:** arquitectura y documentación
- **Herramienta:** Claude Code

## Prompt

El responsable pidió «sigue con los pasos pendientes» tras confirmar el recorte de alcance (RF-01, RF-06, RF-10 y RF-12 fuera de la implementación). Entrada: `specs/functional/*`, ADR-001 y ADR-002, y el calendario del documento 31.

## Respuesta relevante

El asistente propuso el modelo de dominio y el ERD (11 tablas, con restricciones que sostienen las reglas), el correlativo transaccional con `UPDLOCK`, dos diagramas C4, cuatro ADR (Clean Architecture, FluentValidation, ProblemDetails, JWT), un OpenAPI de 10 rutas y la tabla de endpoints con la columna «en la implementación».

## Cambio

`docs/arquitectura/modelo.md`, `c4-contexto.md`, `c4-contenedores.md`; `docs/adr/ADR-003` a `ADR-006`; `docs/api/openapi-borrador.yaml`; `specs/technical/stack-y-endpoints.md`; `docs/alcance.md` y `docs/trazabilidad.md` actualizados.

## Decisión

**Aceptado como borrador, con tres puntos que el responsable debe revisar:**

1. El documento 31 dice «RF-02/03 Consulta de MP»; en `requisitos.md` RF-03 es *Editar*. El asistente lo trató como editar (implementado). Si RF-03 queda fuera, se quita un endpoint.
2. Las transiciones de estado de la OT (RN-37) siguen **a validar** con un usuario; se escribieron como propuesta.
3. El motor de base de datos sigue sin confirmar con el instructor (consulta sin respuesta); se asume SQL Server según ADR-001.

**Corregido por el asistente al revisar:** el estado inicial del MP se escribió primero como «Registrado» y las historias dicen «Nuevo»; se unificó a «Nuevo». El OpenAPI se validó con un analizador YAML y se comprobó que ninguna referencia `$ref` estuviera rota.

## Revisión humana

Pendiente: el responsable debe aceptar los ADR (están como *Propuesta*) y revisar el modelo antes de crear las migraciones.
