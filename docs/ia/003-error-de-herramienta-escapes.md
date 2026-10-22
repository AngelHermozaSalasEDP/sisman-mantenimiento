# 003 — Un error del asistente al escribir código con saltos de línea

- **Fecha:** 2026-10-22
- **Tarea / requisito:** Añadir bitácora a las pruebas de caracterización (soporte de RNF-05)
- **Tipo:** debugging
- **Herramienta:** Claude Code

## Prompt

Continuación del trabajo de pruebas; el responsable había conectado la red y pedido reiniciar la captura de la referencia.

## Respuesta relevante

El asistente editó el archivo de pruebas con un script y, por cómo se pasó la secuencia de escape `\n`, quedó un **salto de línea real dentro de una cadena** de C#. Dos compilaciones fallaron con «literal de cadena no terminado». El primer arreglo (otro script con la misma secuencia) repitió el fallo.

## Cambio

Cambio final en `tests/.../Pruebas.cs` y `Soporte.cs` del repositorio de modernización (commit `8e6e44a`): se usó `Environment.NewLine` y `string.Join('\n', ...)` con la herramienta de edición de archivos, que no interpreta escapes.

## Decisión

**Rechazado el primer arreglo, aceptado el segundo.** Se dejó de usar scripts para editar texto con barras invertidas.

## Revisión humana

El responsable no intervino en el detalle; el compilador y las pruebas en verde fueron la verificación. Aprendizaje: para texto con escapes, usar la herramienta de edición y no un script.
