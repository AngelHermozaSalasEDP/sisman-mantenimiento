# 005 — Elegir cómo acceder a datos: EF Core frente a ADO.NET

- **Fecha:** 2026-10-22
- **Tarea / requisito:** Decisión de arquitectura (ADR-001)
- **Tipo:** arquitectura
- **Herramienta:** Claude Code

## Prompt

El responsable preguntó: *«explica esto: ¿uso EF Core en el módulo? Eso cambia la decisión A4, sin ORM»* y después *«cuál es tu recomendación final»*.

## Respuesta relevante

El asistente explicó qué es EF Core, por qué el curso lo exige y qué complica usarlo con la base existente (4 tablas sin clave primaria, 33 procedimientos ausentes, listas de 26 000 OT, migraciones sobre una base ya existente). Presentó tres opciones (EF Core completo, híbrido, seguir sin ORM) y dos formas de resolver las migraciones, y recomendó el híbrido con un esquema propio en Docker.

## Cambio

`docs/adr/ADR-001-ef-core.md` (este repositorio).

## Decisión

**Aceptado con un ajuste.** El responsable aprobó la recomendación (*«ok»*). Al pasar a un repositorio separado, el híbrido se simplificó: los procedimientos del sistema original **no existen** en la base de ejemplo, así que su lógica se reimplementa en C# y se prueba. Lo registra el ADR-001.

## Revisión humana

El responsable aprobó la recomendación completa (alcance acotado, repositorio separado, EF Core, registro de IA y consulta al instructor). **Pendiente:** confirmar con el instructor.
