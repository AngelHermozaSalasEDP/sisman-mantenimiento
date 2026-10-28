# 004 — Secreto encontrado en el historial de git

- **Fecha:** 2026-10-22
- **Tarea / requisito:** Revisión de requisitos del curso («sin secretos en el repo ni en el historial»; RNF-03)
- **Tipo:** revisión de calidad y seguridad
- **Herramienta:** Claude Code

## Prompt

El responsable pidió analizar los archivos `Plan-de-trabajo.md` y `Requisitos-para-aprobar.md`, compararlos con el proyecto y entregar un informe.

## Respuesta relevante

Entre las comprobaciones, el asistente buscó patrones de claves en los archivos versionados y en el historial (mostrando solo nombres de archivo). Encontró que el archivo `NLog.config` del sistema original trae una cadena de conexión con la cuenta `sa` y su clave en texto, en 2 commits. **No copió ni mostró el valor.**

## Cambio

Informe: documento 24 del repositorio de modernización (`70d69b3`). No se modificó el historial.

## Decisión

**Aceptado el hallazgo; pendiente la corrección.** Reescribir el historial es destructivo y requiere la autorización explícita del responsable, que todavía no la dio. Primero debe rotarse la clave. Por eso este repositorio nuevo se creó sin ningún archivo del sistema original.

## Revisión humana

El responsable decidió trabajar en un repositorio separado y limpio (ADR-002).

**Revisión del responsable (2026-10-28, checklist 41):** la rotación de la clave y la limpieza del historial del repositorio de modernización **siguen pendientes**; quedan para entregarlas al DBA. Este repositorio no tiene archivos del sistema original. Búsqueda de secretos en este repositorio y su historial (2026-10-28): limpia; solo aparece la constante `ClaimToken`, que es el nombre de un dato.
