# 011 — RF-07: generar OT con correlativo transaccional y prueba concurrente

- **Fecha:** 2026-10-23
- **Tarea / requisito:** RF-07; RN-30 a RN-35; RNF-02 (hallazgo 1 del sistema original: número de OT sin transacción)
- **Tipo:** código, pruebas, depuración
- **Herramienta:** Claude Code

## Prompt

«si» (seguir con RF-07), con el modelo `docs/arquitectura/modelo.md` (sección 4, correlativo con `UPDLOCK`) y ADR-001 como contexto.

## Respuesta relevante

El asistente implementó `OtServicio.GenerarAsync` dentro de una transacción ejecutada por la estrategia de reintentos de EF Core; la fecha se lee con `UPDLOCK` (dos solicitudes sobre la misma fecha se ponen en cola) y el número se toma con un `UPDATE … OUTPUT` parametrizado sobre la fila del correlativo. Agregó el endpoint `POST /api/v1/fechas/{id}/ordenes-trabajo`, el detalle `GET /api/v1/ordenes-trabajo/{id}` y 8 pruebas de integración.

## Cambio

`Application/OrdenesTrabajo/OtServicio.cs`, `Infrastructure/Persistencia/OtRepositorio.cs`, `Api/Controllers/OrdenesTrabajoController.cs`, `Application/Abstracciones/Puertos.cs`, `tests/.../GenerarOtTests.cs`, `docs/trazabilidad.md`.

## Decisión

**Aceptado.** 66 pruebas en verde en 5 corridas seguidas.

**Depuración:** la primera corrida completa falló en la prueba de 20 solicitudes. El asistente leyó el detalle antes de cambiar el código: el error era de la prueba, no del sistema. El contador de días que la prueba usaba para crear fechas chocaba con una fecha que la semilla ya tenía (hoy + 31 en el MP 29) y la API respondía, correctamente, 409 `fecha.duplicada`. Se movió el contador a 100 días.

**Comprobación de que la prueba detecta el defecto (prueba de mutación manual):** se reemplazó temporalmente el correlativo por la forma del sistema original (leer el número y luego actualizarlo, en dos pasos). La prueba concurrente falló: 19 de 20 solicitudes terminaron en 500, porque recibieron números repetidos y la restricción `UNIQUE` sobre el número de OT las rechazó. Se restauró el código (sin diferencias) y todo volvió a verde. Esto también demuestra la segunda defensa: aun con un correlativo defectuoso, la base no admite números repetidos.

**Observaciones para el responsable:**

1. Si alguna vez la restricción de unicidad rechaza un número, la API responde 500 (no un éxito falso). Con el correlativo actual no ocurre.
2. La regla «el MP debe estar aprobado» se comprueba después de tomar el número; si falla, la transacción se deshace y el número no se consume (no se escribió una prueba específica para ese caso).

## Revisión humana

Pendiente: revisión del diff por el responsable.
