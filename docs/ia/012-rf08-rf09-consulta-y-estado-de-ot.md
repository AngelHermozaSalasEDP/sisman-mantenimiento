# 012 — RF-08 y RF-09: consulta paginada de OT y cambio de estado

- **Fecha:** 2026-10-23
- **Tarea / requisito:** RF-08, RF-09; RN-36 a RN-38, RN-40 a RN-42
- **Tipo:** código y pruebas
- **Herramienta:** Claude Code

## Prompt

«si» (seguir con RF-08 y RF-09), con `specs/technical/stack-y-endpoints.md` (sección 4, transiciones) y el dominio `OrdenTrabajo.CambiarEstado` como contexto.

## Respuesta relevante

El asistente agregó `GET /api/v1/ordenes-trabajo` (filtros por número, tipo, estado y rango de fecha objetivo; orden numérico descendente; paginación en el servidor) y `PUT /api/v1/ordenes-trabajo/{id}/estado`, más 19 pruebas de integración.

## Cambio

`Application/OrdenesTrabajo/OtServicio.cs`, `Application/Abstracciones/Puertos.cs`, `Infrastructure/Persistencia/OtRepositorio.cs`, `Api/Controllers/OrdenesTrabajoController.cs`, `tests/.../OrdenesTrabajoTests.cs`, `docs/trazabilidad.md`.

## Decisión

**Aceptado.** 85 pruebas en verde en 3 corridas.

**Corregido por el propio asistente antes de probar:**

1. La primera versión validaba el estado del filtro dentro del controlador, contra ADR-004 (validación en un solo punto). Se movió al validador `FiltroOtValidator`.
2. El primer validador del cambio de estado combinaba `Enum.IsDefined` con un rango, de forma redundante y difícil de leer; se dejó solo `InclusiveBetween(37, 42)`.

**Decisiones del asistente que el responsable debe confirmar:**

1. El rango de fechas de la consulta de OT filtra por la **fecha objetivo**.
2. La lista no trae el historial (solo el detalle), para que la página pese menos.
3. Las transiciones siguen siendo la propuesta de RN-37, **a validar con un usuario**.

**Pendiente:** la medida de RNF-01 (respuesta en menos de 2 s y menos de 200 KB con 26 000 OT) no se probó: la semilla tiene pocas OT.

## Revisión humana

Pendiente: revisión del diff y de las tres decisiones.
