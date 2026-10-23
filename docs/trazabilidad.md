# Matriz de trazabilidad

RF → Historia → Caso de uso → Reglas → Endpoint → Clase → Prueba. Se completa a medida que avanzan las fases. «—» significa pendiente (o que el RF no tiene caso de uso porque no es de prioridad M).

| RF | Historia | Caso de uso | Reglas | Endpoint | Clase | Prueba | Estado |
|---|---|---|---|---|---|---|---|
| RF-01 Registrar MP | HU-01 | CU-01 | RN-10 a RN-13, RN-53 | POST /mantenimientos-preventivos | `MantenimientoPreventivo.Crear` (dominio) | `DominioTests` (RN-10 a RN-13) | Dominio listo; sin endpoint (fuera de la implementación) |
| RF-02 Consultar MP | HU-02 | CU-02 | RN-40, RN-42 | GET /mantenimientos-preventivos | `MantenimientosPreventivosController`, `MpServicio`, `MpRepositorio` | `MantenimientosPreventivosTests` (consulta, paginación, filtros, detalle, 404) | Implementado (2026-10-23) |
| RF-03 Editar MP | HU-03 | CU-03 | RN-14, RN-53 | PUT /mantenimientos-preventivos/{id} | `MantenimientosPreventivosController.Editar`, `MantenimientoPreventivo.Modificar` | `MantenimientosPreventivosTests` (edición, 400, 403, referencias) y `DominioTests` | Implementado (2026-10-23) |
| RF-04 Aprobar MP | HU-04 | CU-04 | RN-15, RN-16 | POST /mantenimientos-preventivos/{id}/aprobacion | `MantenimientosPreventivosController.Aprobar`, `MantenimientoPreventivo.Aprobar` | `MantenimientosPreventivosTests` (aprobar, reaprobar, 403, 404) y `DominioTests` | Implementado (2026-10-23) |
| RF-05 Programar fecha | HU-05 | CU-05 | RN-20 a RN-24 | POST /mantenimientos-preventivos/{id}/fechas | `MantenimientosPreventivosController.ProgramarFecha`, `MantenimientoPreventivo.ProgramarFecha` | `MantenimientosPreventivosTests` (RN-20 a RN-24, 403) y `DominioTests` | Implementado (2026-10-23) |
| RF-06 Eliminar fecha | HU-06 | — | RN-25 | DELETE /mantenimientos-preventivos/{id}/fechas/{fechaId} | — | — | Especificado; **fuera de la implementación** (doc. 31) |
| RF-07 Generar OT | HU-07 | CU-07 | RN-30 a RN-35 | POST /fechas/{id}/ordenes-trabajo | `OrdenesTrabajoController.Generar`, `OtServicio`, `OtRepositorio` (correlativo con `UPDLOCK`), `OrdenTrabajo.Generar` | `GenerarOtTests` (8, incluida la prueba concurrente de 20 solicitudes y la de falla con vuelta atrás) y `DominioTests` | Implementado (2026-10-23) |
| RF-08 Consultar OT | HU-08 | CU-08 | RN-40 a RN-42, RN-52 | GET /ordenes-trabajo | `OrdenesTrabajoController.Consultar` y `ObtenerOt`, `OtServicio`, `OtRepositorio` | `OrdenesTrabajoTests` (orden numérico, paginación, filtros, 400, especialista). Falta la medida de RNF-01 (2 s y 200 KB con 26 000 OT) | Implementado (2026-10-23) |
| RF-09 Cambiar estado de OT | HU-09 | CU-09 | RN-36 a RN-38 | PUT /ordenes-trabajo/{id}/estado | `OrdenesTrabajoController.CambiarEstado`, `OrdenTrabajo.CambiarEstado` | `OrdenesTrabajoTests` (reapertura, anulación, prohibidas, 400, 404, 403) y `DominioTests` | Implementado (2026-10-23); RN-37 a validar con un usuario |
| RF-10 Planning | HU-10 | — | RN-43 | GET /planning | — | — | Especificado; **fuera de la implementación** (doc. 31) |
| RF-11 Iniciar sesión | HU-11 | CU-11 | RN-01 a RN-03 | POST /auth/login | `AuthController`, `AutenticacionServicio`, `GeneradorTokenJwt` | `AutenticacionTests` (6) | Implementado (2026-10-23) |
| RF-12 Exportar OT a Excel | HU-12 | — | RN-42 | GET /ordenes-trabajo/exportacion | — | — | Especificado; **fuera de la implementación** (doc. 31) |

Los endpoints están definidos en `specs/technical/stack-y-endpoints.md` y `docs/api/openapi-borrador.yaml` (borrador v0.1, 2026-10-23). Arquitectura: `docs/arquitectura/` y ADR-003 a ADR-006. Todas las rutas llevan el prefijo `/api/v1`.

## Web mínima de demostración (`Sisman.Mantenimiento.Web`)

Plan: documento 33 del proyecto de modernización (decisiones D1 a D6 aprobadas el 2026-10-23). La web no tiene reglas propias: llama a la API y muestra sus respuestas.

| Pantalla | Ruta | RF que usa | Página | Prueba |
|---|---|---|---|---|
| Login y salir | `/login`, `/logout` | RF-11 | `LoginModel`, `LogoutModel` | `WebTests` (sin sesión, credenciales inválidas, login, token vencido) |
| Lista de MP | `/mp` | RF-02 | `Mp/IndexModel` | `WebTests` (lista con token) |
| Detalle de MP, aprobar, programar fecha, generar OT | `/mp/{id}` | RF-02, RF-04, RF-05, RF-07 | `Mp/DetalleModel` | `WebTests` (botones por perfil, error de la API, antifalsificación) |
| Lista de OT | `/ot` | RF-08 | `Ot/IndexModel` | Recorrido manual de la demo |
| Detalle de OT y cambio de estado | `/ot/{id}` | RF-08, RF-09 | `Ot/DetalleModel` | Recorrido manual de la demo |

El recorrido completo de la demo (21 comprobaciones, contra la API y la base reales) se ejecutó el 2026-10-23 con un script HTTP; está descrito en la evidencia de IA 013.
