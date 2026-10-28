# 014 — Completar todas las opciones: RF-01, RF-06, RF-10, RF-12, catálogos, web y RNF-01

- **Fecha:** 2026-10-23
- **Tarea / requisito:** RF-01, RF-03 (web), RF-06, RF-10, RF-12, RNF-01; documento 31 v1.2
- **Tipo:** código, pruebas, depuración, refactorización
- **Herramienta:** Claude Code

## Prompt

«si completa todo» y luego «completar todas las opciones», tras la lista de pendientes (funciones fuera del recorte, pendientes de lo construido y entregables).

## Respuesta relevante

1. **Refactorización:** la transacción y el correlativo salieron de `IOtRepositorio` a un puerto común `ITransacciones`, para que el alta de MP (código `MP-nnnnnnn`) y la generación de OT usen el mismo mecanismo sin duplicar código.
2. **API:** `POST /mantenimientos-preventivos` (RF-01), `DELETE …/fechas/{id}` (RF-06), `GET /planning` (RF-10), `GET /ordenes-trabajo/exportacion` (RF-12, ClosedXML), `GET /catalogos` y `GET /maquinas`. Mensajes de transición con nombres legibles («En programación» en vez de «EnProgramacion»).
3. **Web:** formulario de alta y edición, botones Editar y Eliminar fecha, Nuevo MP, Exportar a Excel y una pantalla de planning con calendario mensual y filtros.
4. **Pruebas:** 16 de integración nuevas, 6 de rendimiento, 2 de dominio y 3 de la web. Total: 25 + 84 + 10 = **119**.

## Depuración y correcciones

| # | Qué pasó | Causa | Corrección |
|---|---|---|---|
| 1 | La compilación falló con archivos bloqueados | La API y la web de desarrollo seguían corriendo | Se detuvieron antes de compilar |
| 2 | 4 de 5 casos de rendimiento fallaron | El `INSERT` de 26 000 OT superó los 30 s de tiempo máximo de LocalDB (preparación, no medición) | Tiempo máximo de 5 min solo para la carga y carga única por clase |
| 3 | Error de compilación en el formulario | Una página Razor admite una sola directiva `@page` | Ruta `/mp/editar/{id?}` y `/mp/nuevo` como ruta adicional |
| 4 | El recorrido mostró «'Mes' no debe estar vacío» | `WithMessage` de FluentValidation solo se aplica a la regla anterior de la cadena | Un mensaje por regla |

## Verificación

- **RNF-01:** con 26 003 OT, la página responde en 13 a 47 ms y pesa 22,9 KB (91 KB con 200 filas). Límite: 2 s y 200 KB.
- **Recorridos de la demo contra la API y la base reales:** 18 de 18 (funciones nuevas) y 21 de 21 (flujo anterior).
- **Pruebas:** 119 en verde en 5 corridas completas.

## Decisión

**Aceptado.** Decisiones del asistente que el responsable debe confirmar:

1. Eliminar una fecha **no recalcula** las fechas de inicio, próxima y última del MP (la especificación no lo pide).
2. El planning admite **hasta 92 días** por consulta y cuenta por **día de inicio** de la fecha programada.
3. La exportación tiene un **tope de 10 000 filas**; con más, pide afinar los filtros.
4. En el formulario web, sistema y equipo se eligen de listas agrupadas por máquina (sin JavaScript); la API rechaza combinaciones que no pertenecen.

**Pendiente sin explicar:** en una corrida completa, con la API y la web de desarrollo corriendo al mismo tiempo, falló 1 de 84 pruebas de integración y no se capturó cuál. En 5 corridas posteriores no se repitió. Se deja registrado; no se atribuye una causa que no se comprobó.

## Revisión humana

Pendiente: probar las pantallas nuevas en el navegador y confirmar las cuatro decisiones.

**Revisión del responsable (2026-10-28, checklist 41):** probó en el navegador el alta, la edición, la exportación y el planning, y acepta las cuatro decisiones (eliminar una fecha no recalcula las fechas del MP; planning de hasta 92 días; exportación de hasta 10 000 filas; listas agrupadas por máquina). El fallo intermitente **no se ha repetido**.

**Nota (2026-10-28):** el total de este registro (119 pruebas) es el de su fecha; hoy son 121.

**Fallo intermitente, comprobación (2026-10-28):** 10 corridas seguidas de las pruebas de integración, con el resultado de cada prueba guardado: 85 de 85 en las 10 (850 ejecuciones, sin fallos). Diferencia con el 23/10: esta vez la API y la web de desarrollo no estaban corriendo, así que no se reprodujeron las condiciones exactas del fallo. Si vuelve a ocurrir, el registro por prueba dirá cuál fue.

## Capturas

- [01 — 121 pruebas en verde](capturas/01-pruebas-121-verdes.png)
- [05 — validación al registrar un MP](capturas/05-alta-mp-validacion.png)
- [06 — planning del mes](capturas/06-planning-mes.png)
- [07 — el especialista que abre /planning vuelve al login](capturas/07-especialista-sin-planning.png)
