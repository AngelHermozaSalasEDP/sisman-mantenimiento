# Alcance del proyecto

## Problema

SISMAN es el sistema con el que una editora gestiona el mantenimiento de su planta de impresión. Se construyó hace años como parte de una aplicación mayor; se está modernizando. Hoy el flujo central se apoya en reglas que viven dentro del código de las pantallas, sin restricciones en la base de datos:

- El número de una orden de trabajo (OT) se toma de un correlativo en cuatro pasos separados y **sin transacción**: dos usuarios a la vez podrían obtener el mismo número.
- Un mantenimiento preventivo (MP) no calcula sus fechas: **una persona las programa a mano, cada día**.
- Cuando un procedimiento de la base falla, la pantalla igualmente informa éxito.
- Las listas de OT pesan más de 1 MB y la queja principal de los usuarios es la lentitud.

## Módulo elegido

**Mantenimiento preventivo → fecha programada → orden de trabajo.** Es el flujo que más usan, ya existe, y se comprobó en ejecución.

## Actores

| Actor | Qué hace |
|---|---|
| Planificador de mantenimiento | Crea y aprueba MP, programa fechas, genera y asigna OT, cambia su estado |
| Especialista de mantenimiento | Consulta las OT que le asignan y registra su avance |
| Administrador técnico | Mantiene catálogos y accesos (fuera del alcance de esta API) |

## Dentro del alcance (se implementa)

Recorte confirmado el 2026-10-23 por el plazo de entrega (31/10/2026). Las especificaciones de los 12 RF se mantienen completas; se recorta el código, no las specs ni las pruebas.

1. Inicio de sesión con JWT y manejo uniforme de errores (RF-11).
2. Consulta y edición de mantenimientos preventivos (RF-02, RF-03) y su aprobación (RF-04).
3. Programar fechas de un MP (RF-05).
4. Generar una OT desde una fecha programada, con número único garantizado por una transacción y probado con 20 solicitudes simultáneas (RF-07).
5. Consulta paginada de OT y cambio de estado con las transiciones principales (RF-08, RF-09).
6. API documentada con OpenAPI y entorno local reproducible (SQL Server local o LocalDB).
7. **Web mínima de demostración** (proyecto `Sisman.Mantenimiento.Web`, añadida el 2026-10-23): login, lista de MP, programar fecha, generar OT, lista de OT y cambio de estado. Se construye después de RF-08 y RF-09 y se reduce u omite si el plazo lo exige.

## Ampliación del 2026-10-23

El responsable pidió completar todas las opciones del módulo: RF-01 (alta de MP, con catálogos), RF-06 (eliminar fecha), RF-10 (planning) y RF-12 (exportación a Excel) vuelven al alcance, junto con el alta y la edición de MP en la web. Orden de recorte si el plazo aprieta: RF-10, RF-12 y el alta en la web.

Fuera del módulo: reportes y exportaciones a PDF, rotativa, asignación y consumo de personal en la OT, informe de falla, plan de trabajo, seguridad compartida con otros sistemas, interfaz de usuario completa (solo hay la web mínima de demostración), migración de datos del sistema original.

## Fuente de las reglas

Las reglas provienen del análisis del sistema original (código, base de datos y validación con un usuario real). Cada regla citada en `specs/` indica su origen. El sistema original **no forma parte de este repositorio**.

## Criterio de éxito

Todos los requisitos de `specs/functional/requisitos.md` implementados, con al menos una prueba por criterio de aceptación, la matriz de trazabilidad completa y la API ejecutable desde un clon limpio con `dotnet run`.
