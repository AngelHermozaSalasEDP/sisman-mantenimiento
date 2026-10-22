# 001 — Comparar la aplicación nueva con el sistema original

- **Fecha:** 2026-10-20
- **Tarea / requisito:** Verificar que la extracción a .NET 10 (etapa B1 de la modernización) devuelve los mismos datos
- **Tipo:** pruebas
- **Herramienta:** Claude Code

## Prompt

El responsable pidió: *«Comparar los datos con el sistema actual, pantalla por pantalla»* y, ante la propuesta de que el asistente lo hiciera, respondió *«hazlo tú ahora»*.

## Respuesta relevante

El asistente levantó las dos aplicaciones a la vez (puertos distintos, misma base de datos de pruebas) y escribió scripts que compararon 14 pantallas, 11 búsquedas y 29 exportaciones a Excel. Resultado: pantallas y búsquedas idénticas, salvo la etiqueta de instancia y dos atributos de validación que genera el framework; 17 de 29 exportaciones idénticas.

## Cambio

Repositorio de modernización: commits `f917264` (extracción), `eda2293` (informe), `8e6e44a` (pruebas automáticas de lectura).

## Decisión

**Corregido.** La primera comparación de 12 exportaciones devolvió la página de error en ambos sistemas porque los parámetros eran inválidos; el asistente lo informó como «no verificadas» en lugar de contarlas como correctas, y después amplió el rango de fechas para obtener datos reales. Varias exportaciones «idénticas» eran hojas vacías; el informe lo declara.

## Revisión humana

El responsable revisó el informe (documento 10) y decidió seguir con pruebas de escritura. Queda anotado que la comparación solo cubre el contenido de las celdas, no los estilos.
