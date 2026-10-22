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

## Dentro del alcance

1. Mantenimiento preventivo: alta, consulta, edición y aprobación.
2. Fechas programadas de un MP: alta y baja.
3. Generación de órdenes de trabajo desde una fecha programada, con número único garantizado.
4. Consulta de OT con filtros y paginación; cambio de estado.
5. Calendario de MP programados (planning).
6. Autenticación con JWT y manejo uniforme de errores.
7. API documentada con OpenAPI; entorno local reproducible con Docker.

## Fuera del alcance (mejoras pendientes)

Reportes y exportaciones a PDF, rotativa, asignación y consumo de personal en la OT, informe de falla, plan de trabajo, seguridad compartida con otros sistemas, interfaz de usuario web, migración de datos del sistema original.

## Fuente de las reglas

Las reglas provienen del análisis del sistema original (código, base de datos y validación con un usuario real). Cada regla citada en `specs/` indica su origen. El sistema original **no forma parte de este repositorio**.

## Criterio de éxito

Todos los requisitos de `specs/functional/requisitos.md` implementados, con al menos una prueba por criterio de aceptación, la matriz de trazabilidad completa y la API ejecutable desde un clon limpio con `docker compose up`.
