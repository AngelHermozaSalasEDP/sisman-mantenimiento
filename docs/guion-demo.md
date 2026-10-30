# Guion de la demo (5 minutos)

Sigue la diapositiva 11 de la presentación (v1.3, 2026-10-30).

## Preparación (antes de presentar)

1. Base de ejemplo: la semilla crea 30 MP, 3 OT, 2 usuarios y catálogos. Para empezar limpio, borre la base `SismanMantenimiento` y deje que la API la recree al arrancar.
2. Levante la API (puerto 5300) con las claves de `user-secrets` y la web (puerto 5400), como indica el README. La secuencia completa, probada paso a paso el 2026-10-27, está en el documento 40 «Secuencia de ejecución del sistema» v1.3 (carpeta `05 Documentos` del proyecto de modernización; no forma parte de este repositorio).
3. Abra en pestañas: http://localhost:5300/swagger (paso 0 y plan B) y http://localhost:5400/login.
4. Inicie sesión **justo antes** de empezar: el token dura 30 minutos.
5. Tenga a mano las claves de prueba de `planificador` y `especialista`.

## Pasos

| # | Acción | Qué se espera ver | Regla que demuestra |
|---|---|---|---|
| 0 | **Swagger**: mostrar las 16 operaciones; probar una sin token | 401; tras **Authorize** con el token del login, la operación responde | RF-11, RN-01 |
| 1 | Iniciar sesión como `planificador` | Lista de mantenimientos preventivos, 30 en total | RF-11, RN-01 |
| 2 | **Nuevo MP**: frecuencia mensual **sin mes** → Registrar | Aviso: «La frecuencia mensual o anual exige un mes entre 1 y 12.» | RN-13 |
| 3 | Elegir el mes → Registrar | Detalle del MP con código `MP-0000031` en estado Nuevo | RF-01, RN-10 |
| 3b | **Editar** el MP (por ejemplo, la descripción) → Guardar | Detalle del MP en estado **Modificado** | RF-03, RN-14 |
| 4 | **Aprobar** | «El MP-0000031 quedó aprobado.» | RF-04, RN-16 |
| 5 | Programar una fecha **con la fecha de hoy** | Aviso: «La fecha de inicio debe ser posterior a hoy.» | RN-20 |
| 6 | Programar una fecha de la próxima semana | La fecha aparece «Sin OT», con **Generar OT** y **Eliminar** | RF-05 |
| 7 | **Generar OT** | Detalle de la OT: Pendiente, prioridad Alta; solo ofrece Concluido y Anulación | RF-07, RN-31, RN-37 |
| 8 | **Pasar a Concluido** | Historial con el cambio, el usuario y la hora; ahora ofrece reabrir | RF-09, RN-38 |
| 9 | Menú **Planning** | Calendario del mes con la cantidad de MP sin OT por día | RF-10 |
| 10 | **Órdenes de trabajo** → **Exportar a Excel** | Se descarga el `.xlsx` | RF-12 |
| 11 | Salir e iniciar sesión como `especialista` | Ve listas y detalles sin botones de escritura; el menú no muestra Planning y al abrir `/planning` vuelve al login (captura 07) | RN-03 |

Mensaje de cierre de la demo: «Lo que acaban de ver está cubierto por 121 pruebas automáticas; la más importante genera 20 OT a la vez y comprueba que no se repite ningún número.»

## Plan B

- Si la web falla: el mismo flujo en Swagger (Authorize con el token del login) o con `docs/api/sisman.http`.
- Si la base falla: mostrar la corrida de pruebas (`dotnet test`) y la evidencia de IA 011 (prueba concurrente y prueba de mutación).
- Último recurso: capturas 03 a 07 de `docs/ia/capturas`.

## Preguntas esperadas

| Pregunta | Dónde está la respuesta |
|---|---|
| ¿Por qué Clean Architecture y no un solo proyecto? | ADR-003 |
| ¿Cómo garantiza que el número de OT no se repite? | `Transacciones.SiguienteNumeroAsync`, `OtServicio.GenerarAsync`, evidencia 011 |
| ¿Qué hizo la IA y qué corrigió usted? | `docs/ia/resumen.md` y evidencias 002, 011, 013 y 014 |
| ¿Por qué no microservicios? | ADR-002 y `docs/arquitectura/c4-contenedores.md` |
| ¿Qué falta? | Sección 12 del documento técnico |
| ¿Por qué no hay Docker? | ADR-007 y evidencia 015: el profesor confirmó en clase que no era obligatorio, y no se entrega un entorno que no se pudo ejecutar |
| ¿Qué fue el fallo intermitente? | Evidencia 014: falló una vez el 23/10 sin registrar cuál; el 28/10, 10 corridas seguidas (850 ejecuciones) en verde |
| ¿Qué es RN-37? | `specs/functional/reglas.md`: transiciones permitidas de la OT, con reapertura de una OT concluida |
