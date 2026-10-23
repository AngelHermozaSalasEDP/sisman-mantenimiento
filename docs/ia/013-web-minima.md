# 013 — Web mínima de demostración (Sisman.Mantenimiento.Web)

- **Fecha:** 2026-10-23
- **Tarea / requisito:** documento 33 (W-01 a W-12); usa RF-02, RF-04, RF-05, RF-07, RF-08, RF-09 y RF-11
- **Tipo:** código, pruebas
- **Herramienta:** Claude Code

## Prompt

«si» (aprobar las decisiones D1 a D6 del documento 33: Razor Pages, token en la cookie del servidor, sin edición de MP, estilo propio, puerto 5400, agregarla al Compose).

## Respuesta relevante

El asistente creó el proyecto Razor Pages con: cliente tipado de la API que lee `ProblemDetails`, inicio de sesión con cookie que guarda el token de la API (vence con él), cierre de sesión automático si la API responde 401, siete páginas (login, salir, lista y detalle de MP, lista y detalle de OT, error) y una hoja de estilos propia sin dependencias. Quitó Bootstrap y jQuery de la plantilla.

**Cambio en la API para no duplicar reglas:** el detalle de la OT ahora incluye `transicionesPermitidas` (calculado por el dominio, RN-37). La web solo ofrece esos botones; no copia la tabla de transiciones.

## Cambio

`src/Sisman.Mantenimiento.Web/` (nuevo), `tests/Sisman.Mantenimiento.WebTests/` (7 pruebas con una API simulada), `Domain/Entidades/OrdenTrabajo.cs`, `Application/OrdenesTrabajo/OtServicio.cs`, `Infrastructure/Persistencia/SismanDbContext.cs`, `docker/Dockerfile` (una imagen para la API o la web), `docker/docker-compose.yml`, `README.md`, `docs/trazabilidad.md`.

## Verificación

1. **Recorrido de la demo contra la API y la base reales** (script HTTP, 21 comprobaciones): sin sesión va al login; clave incorrecta; login; lista y filtro de MP; aprobar; reaprobar (error legible); programar con fecha de hoy (error) y con fecha válida; generar OT; solo se ofrecen Concluido y Anulación desde Pendiente; concluir; transición prohibida enviada a mano (error legible); lista de OT filtrada; el especialista no ve acciones; salir.
2. **Primera corrida: 3 de 21 fallaron, pero por el script**, no por la web: Razor escribía las letras acentuadas como entidades HTML (`&#xE1;`) y las comprobaciones de texto no las reconocían. Se configuró la web para emitir el texto en español tal cual y el script para decodificar entidades. Segunda corrida: 21 de 21.
3. Detalle de redacción encontrado en el recorrido: la API decía «El MP MP-0000001 ya está aprobado»; se cambió a «El MP-0000001 ya está aprobado».
4. **Pruebas automáticas:** 92 en verde (23 de dominio, 62 de integración, 7 de la web).

## Decisión

**Aceptado.** Pendiente de revisión humana: probar la web en un navegador (el recorrido fue por HTTP, no se vio la pantalla) y revisar el estilo.

**No verificado:** el Compose con la web (Docker no está instalado). Los mensajes de transición prohibida usan el nombre interno del estado (por ejemplo «EnProgramacion»), sin tildes ni espacios.
