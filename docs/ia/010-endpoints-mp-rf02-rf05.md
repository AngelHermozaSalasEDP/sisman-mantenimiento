# 010 — Endpoints de MP: consulta, edición, aprobación y programar fecha

- **Fecha:** 2026-10-23
- **Tarea / requisito:** RF-02, RF-03, RF-04, RF-05; RN-03, RN-11 a RN-16, RN-20 a RN-24, RN-40, RN-42
- **Tipo:** código y pruebas
- **Herramienta:** Claude Code

## Prompt

El responsable confirmó («si») que se añadiera la web mínima al plan y que se siguiera con los endpoints. Contexto: `specs/technical/stack-y-endpoints.md`, el OpenAPI borrador, las reglas RN-xx y el dominio ya escrito.

## Respuesta relevante

El asistente añadió el servicio de aplicación `MpServicio`, los validadores (FluentValidation), el repositorio `MpRepositorio` (filtros, paginación en el servidor, validación de referencias, traducción de concurrencia y duplicados a 409), el controlador con cuatro rutas y 27 pruebas de integración contra una base propia con la semilla.

## Cambio

`Application/MantenimientosPreventivos`, `Infrastructure/Persistencia/MpRepositorio.cs`, `Api/Controllers/MantenimientosPreventivosController.cs`, `Api/Errores/UsuarioActual.cs`, `tests/.../MantenimientosPreventivosTests.cs`, `docs/trazabilidad.md`, `docs/alcance.md` (web mínima).

## Decisión

**Aceptado.** 58 pruebas en verde (23 de dominio y 35 de integración).

**Decisiones tomadas por el asistente que el responsable debe confirmar:**

1. El filtro de rango de fechas de la consulta de MP usa la **fecha de inicio** del MP; la especificación decía solo «rango de fechas».
2. La frecuencia viaja como código entero 1 a 4 (como en el OpenAPI) y el estado como texto.
3. Se validó que máquina, sistema, equipo y catálogos existan (400 `mp.referencia-invalida`), una regla que las specs no numeraban. Conviene darle un número (RN) en `reglas.md`.
4. El filtro de código usa «contiene», no «igual».

**Corregido por el asistente al revisar:** una prueba de RN-13 quedó escrita con una expresión confusa (construía un objeto por reflexión) y se reescribió con un parámetro `sinMes`.

**Pendiente de verificar:** la edición no usa un token de versión del cliente; la concurrencia solo se detecta dentro de la misma solicitud (la `rowversion` se comprueba al guardar, pero el cliente no la envía). Es una limitación conocida, no una prueba de dos ediciones simultáneas.

## Revisión humana

Pendiente: el responsable debe revisar las cuatro decisiones de arriba.
