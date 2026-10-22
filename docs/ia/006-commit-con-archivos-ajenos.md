# 006 — Un commit del asistente incluyó documentos que no eran suyos

- **Fecha:** 2026-10-22
- **Tarea / requisito:** Guardar el documento 08 v1.2 (plan de microservicios)
- **Tipo:** control de cambios
- **Herramienta:** Claude Code

## Prompt

El responsable aprobó la arquitectura de microservicios y pidió documentarla.

## Respuesta relevante

Al guardar el documento, el asistente usó `git add` sobre **toda la carpeta de documentos**. El commit `6a3015a` quedó con ese documento y además con los documentos 12 a 19 y sus capturas, que había escrito el responsable en otras sesiones y no estaban guardados.

## Cambio

Commit `6a3015a` del repositorio de modernización.

## Decisión

**Error reconocido; sin reescribir el historial.** El asistente lo detectó al revisar el resultado del commit, lo informó y ofreció separarlo (con una operación destructiva). El responsable no pidió separarlo. Desde entonces se añaden archivos por nombre.

## Revisión humana

Se comprobó que no se perdió nada ni se publicó nada. Lección: nunca añadir una carpeta completa sin revisar antes qué contiene.
