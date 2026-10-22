# 007 — Convertir el análisis del sistema original en historias, casos de uso y reglas

- **Fecha:** 2026-10-22
- **Tarea / requisito:** C-10, C-11 y C-12 del plan (Fase 1); RF-01 a RF-12
- **Tipo:** documentación (especificación)
- **Herramienta:** Claude Code

## Prompt

El responsable confirmó con *«si»* que se continuara con las historias de usuario, los casos de uso y las reglas `RN-xx`, tras haber aprobado el plan del curso.

## Respuesta relevante

El asistente leyó el análisis del sistema original (reglas de MP, fechas y OT, con sus hallazgos y la validación con un usuario) y redactó: 12 historias con criterios Dado/Cuando/Entonces, 9 casos de uso (prioridad M) y 34 reglas `RN-xx`. A cada regla le puso una marca: **heredada**, **corrige** (un defecto del original), **nueva** o **a validar**.

## Cambio

`specs/functional/historias.md`, `casos-de-uso.md`, `reglas.md`; `docs/trazabilidad.md` actualizado con historias, casos de uso, reglas y endpoints previstos.

## Decisión

**Aceptado como borrador, con tres puntos marcados «a validar»** que el asistente no pudo confirmar con el código ni con la base: efecto de aprobar un MP (RN-15, RN-16), transiciones de estado de la OT (RN-37) y la regla nueva RN-23 (no repetir fecha). El asistente prefirió marcarlas que inventarlas como hechos.

**Corregido por el propio asistente** al revisar: el análisis original usaba dos códigos de frecuencia distintos entre el alta y la lectura; se decidió un único catálogo (RN-12) y se anotó que corrige un defecto, no que lo reproduce.

## Revisión humana

Pendiente: el responsable debe revisar las reglas marcadas «a validar» con un usuario (tarea C-13) antes de congelar el alcance.
