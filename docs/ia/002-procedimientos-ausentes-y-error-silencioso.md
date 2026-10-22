# 002 — Botones que dicen «éxito» sin hacer nada

- **Fecha:** 2026-10-20
- **Tarea / requisito:** Probar las acciones de escritura del módulo (insumo de RF-04, RF-07, RF-09)
- **Tipo:** debugging
- **Herramienta:** Claude Code

## Prompt

El responsable autorizó: *«te autorizo»* a probar los botones de escritura en la base de pruebas.

## Respuesta relevante

Al aprobar un MP la pantalla mostró «MP Aprobado», pero la base no cambió. El asistente buscó la causa: el procedimiento `SP_Aprobar_MP` no existe en la base, y el repositorio captura la excepción y la descarta. Después comprobó todos los procedimientos que usa el código: **33 de 47 no existen**.

## Cambio

Documento 10 (`eda2293`) y 22 del repositorio de modernización; lista completa de procedimientos ausentes en el anexo del documento 10.

## Decisión

**Corregido, y se corrigió también una conclusión anterior del propio asistente.** El informe de una etapa previa decía que los procedimientos ausentes eran código sin uso; esta prueba lo desmintió (incluye aprobar MP y asignar personal). Se corrigió en el informe siguiente y se generó el requisito **RNF-04** (nunca informar éxito si la operación falló).

## Revisión humana

El responsable aceptó el hallazgo y pidió que se registrara como pendiente de la base de datos.
