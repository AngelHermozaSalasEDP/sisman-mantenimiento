# Resumen del uso de IA

Estado: **parcial** (11 evidencias registradas; se amplía con cada tarea).

| # | Entrada | Tipo | Resultado |
|---|---|---|---|
| 001 | Comparar la aplicación nueva con el sistema original | pruebas | Aceptado con correcciones |
| 002 | Botones que dicen «éxito» sin hacer nada | debugging | Corregido; corrigió una conclusión previa de la IA |
| 003 | Error del asistente con escapes en código | debugging | Primer arreglo rechazado, segundo aceptado |
| 004 | Secreto en el historial de git | calidad y seguridad | Hallazgo aceptado; corrección pendiente |
| 005 | Elegir EF Core frente a ADO.NET | arquitectura | Aceptado con ajuste |
| 006 | Commit con archivos ajenos | control de cambios | Error reconocido |
| 007 | Historias, casos de uso y reglas desde el análisis | documentación | Aceptado como borrador; 3 reglas a validar |
| 008 | Modelo, C4, ADR-003 a 006, OpenAPI y specs técnicas | arquitectura | Aceptado como borrador; 3 puntos a revisar |
| 009 | Solución base .NET 10: dominio, EF Core, JWT, errores y pruebas | código y pruebas | Aceptado con 4 correcciones; Docker sin probar |
| 010 | Endpoints de MP (RF-02 a RF-05) con pruebas de integración | código y pruebas | Aceptado; 4 decisiones por confirmar |
| 011 | RF-07: correlativo transaccional, prueba de 20 solicitudes y prueba de mutación | código, pruebas y depuración | Aceptado; el primer fallo era de la prueba |

## Dónde ayudó la IA

Análisis de un sistema grande en poco tiempo, comparación automática de respuestas, descubrimiento de errores silenciosos, borradores de documentación y de decisiones.

## Errores de la IA que se detectaron

- Una conclusión de un informe resultó equivocada y se corrigió (002).
- Edición de código con escapes mal pasados (003).
- Un `git add` sobre una carpeta completa mezcló archivos ajenos (006).

## Aprendizajes

1. Una prueba que solo compara «igual que antes» no prueba que funcione: hay que mirar qué se compara (001).
2. Un resultado de «éxito» en pantalla no es una prueba (002).
3. Las decisiones destructivas (reescribir el historial) se piden, no se toman (004).
4. Añadir archivos por nombre, no por carpeta (006).

*Pendiente: capturas de pantalla en `capturas/` y entradas por cada tarea de construcción.*
