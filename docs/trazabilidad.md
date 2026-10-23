# Matriz de trazabilidad

RF → Historia → Caso de uso → Reglas → Endpoint → Clase → Prueba. Se completa a medida que avanzan las fases. «—» significa pendiente (o que el RF no tiene caso de uso porque no es de prioridad M).

| RF | Historia | Caso de uso | Reglas | Endpoint | Clase | Prueba | Estado |
|---|---|---|---|---|---|---|---|
| RF-01 Registrar MP | HU-01 | CU-01 | RN-10 a RN-13, RN-53 | POST /mantenimientos-preventivos | — | — | Especificado; **fuera de la implementación** (doc. 31) |
| RF-02 Consultar MP | HU-02 | CU-02 | RN-40, RN-42 | GET /mantenimientos-preventivos | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-03 Editar MP | HU-03 | CU-03 | RN-14, RN-53 | PUT /mantenimientos-preventivos/{id} | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-04 Aprobar MP | HU-04 | CU-04 | RN-15, RN-16 | POST /mantenimientos-preventivos/{id}/aprobacion | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-05 Programar fecha | HU-05 | CU-05 | RN-20 a RN-24 | POST /mantenimientos-preventivos/{id}/fechas | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-06 Eliminar fecha | HU-06 | — | RN-25 | DELETE /mantenimientos-preventivos/{id}/fechas/{fechaId} | — | — | Especificado; **fuera de la implementación** (doc. 31) |
| RF-07 Generar OT | HU-07 | CU-07 | RN-30 a RN-35 | POST /fechas/{id}/ordenes-trabajo | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-08 Consultar OT | HU-08 | CU-08 | RN-40 a RN-42, RN-52 | GET /ordenes-trabajo | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-09 Cambiar estado de OT | HU-09 | CU-09 | RN-36 a RN-38 | PUT /ordenes-trabajo/{id}/estado | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-10 Planning | HU-10 | — | RN-43 | GET /planning | — | — | Especificado; **fuera de la implementación** (doc. 31) |
| RF-11 Iniciar sesión | HU-11 | CU-11 | RN-01 a RN-03 | POST /auth/login | — | — | Especificado; endpoint en `specs/technical/` y OpenAPI borrador |
| RF-12 Exportar OT a Excel | HU-12 | — | RN-42 | GET /ordenes-trabajo/exportacion | — | — | Especificado; **fuera de la implementación** (doc. 31) |

Los endpoints están definidos en `specs/technical/stack-y-endpoints.md` y `docs/api/openapi-borrador.yaml` (borrador v0.1, 2026-10-23). Arquitectura: `docs/arquitectura/` y ADR-003 a ADR-006. Todas las rutas llevan el prefijo `/api/v1`.
