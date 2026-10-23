# Especificación técnica: stack, endpoints y errores

Estado: **borrador v0.1** (2026-10-23). Contrato completo: `docs/api/openapi-borrador.yaml`. Decisiones: ADR-001 a ADR-006. Modelo de datos: `docs/arquitectura/modelo.md`.

## 1. Stack

| Aspecto | Elección | Decisión |
|---|---|---|
| Plataforma | .NET 10 (LTS), ASP.NET Core con controladores | — |
| Arquitectura | Domain, Application, Infrastructure, Api | ADR-003 |
| Datos | EF Core 10, SQL Server 2022, migraciones y seed | ADR-001 |
| Validación | FluentValidation | ADR-004 |
| Errores | `ProblemDetails` con códigos estables, `X-Correlation-Id` | ADR-005 |
| Seguridad | JWT, PBKDF2, perfiles Planificador y Especialista | ADR-006 |
| Documentación | Swashbuckle (OpenAPI 3) con ejemplos | RNF |
| Pruebas | xUnit, `WebApplicationFactory`, SQL Server de prueba (contenedor o instancia local) | Fase 5 |
| Registro | Serilog a consola con la correlación | RN-54 |
| Entorno | `docker compose up` levanta SQL Server y la API | RNF-05 |

Plan B mientras Docker no esté instalado: SQL Server local; la cadena de conexión viene de variable de entorno y el `docker/docker-compose.yml` se entrega igual.

## 2. Convenciones de la API

- Prefijo `/api/v1`; JSON en camelCase; fechas ISO 8601 (`yyyy-MM-dd` para fechas, UTC para instantes, RN-52).
- Recursos en español, como los nombran los usuarios.
- Paginación: `pagina` (desde 1) y `tamanoPagina` (50 por defecto, máximo 200). La respuesta lleva `items`, `pagina`, `tamanoPagina`, `total`. Un `tamanoPagina` mayor que 200 devuelve 400.
- Creación: 201 con cabecera `Location`.
- Todas las rutas exigen token salvo `POST /auth/login`.

## 3. Endpoints

| RF | Método y ruta | Perfil | Éxito | Errores | Reglas | En la implementación |
|---|---|---|---|---|---|---|
| RF-11 | `POST /auth/login` | público | 200 | 400, 401 | RN-01 a RN-03 | Sí |
| RF-01 | `POST /mantenimientos-preventivos` | Planificador | 201 | 400, 401, 403 | RN-10 a RN-13 | **No** (MP de ejemplo en el seed) |
| RF-02 | `GET /mantenimientos-preventivos` | ambos | 200 | 400, 401 | RN-40, RN-42 | Sí |
| RF-02 | `GET /mantenimientos-preventivos/{id}` | ambos | 200 | 401, 404 | — | Sí |
| RF-03 | `PUT /mantenimientos-preventivos/{id}` | Planificador | 200 | 400, 401, 403, 404, 409 | RN-14 | Sí |
| RF-04 | `POST /mantenimientos-preventivos/{id}/aprobacion` | Planificador | 200 | 401, 403, 404, 409 | RN-15, RN-16 | Sí |
| RF-05 | `POST /mantenimientos-preventivos/{id}/fechas` | Planificador | 201 | 400, 401, 403, 404, 409 | RN-20 a RN-24 | Sí |
| RF-06 | `DELETE /mantenimientos-preventivos/{id}/fechas/{fechaId}` | Planificador | 204 | 401, 403, 404, 409 | RN-25 | **No** |
| RF-07 | `POST /fechas/{fechaId}/ordenes-trabajo` | Planificador | 201 | 401, 403, 404, 409 | RN-30 a RN-35 | Sí |
| RF-08 | `GET /ordenes-trabajo` | ambos | 200 | 400, 401 | RN-40 a RN-42 | Sí |
| RF-08 | `GET /ordenes-trabajo/{id}` | ambos | 200 | 401, 404 | RN-38 | Sí |
| RF-09 | `PUT /ordenes-trabajo/{id}/estado` | Planificador | 200 | 400, 401, 403, 404, 409 | RN-36 a RN-38 | Sí (transiciones principales) |
| RF-10 | `GET /planning` | ambos | 200 | 400, 401 | RN-43 | **No** |
| RF-12 | `GET /ordenes-trabajo/exportacion` | Planificador | 200 (xlsx) | 401, 403 | RN-42 | **No** |

La columna «En la implementación» sigue el recorte del documento 31 del proyecto de modernización: lo que no se implementa conserva su especificación completa. `GET /planning` y la exportación se describen en el OpenAPI definitivo solo si se implementan.

## 4. Transiciones de estado de la OT (RN-37, a validar)

| Desde | Hacia permitido |
|---|---|
| 37 Solicitud | 39 En programación |
| 39 En programación | 40 Pendiente |
| 40 Pendiente | 41 Concluido, 38 Anulación |
| 38 Anulación | 42 Concluido por anulación |
| 41 Concluido | 40 Pendiente (reapertura) |
| 42 Concluido por anulación | ninguna |

Toda otra combinación responde 409 `ot.transicion-invalida`. La OT que genera RF-07 nace en 40 Pendiente. Si el 27/10 RF-07 no está cerrado, RF-09 se limita a 40 → 41 y 40 → 38.

## 5. Errores

Catálogo de códigos en ADR-005. Cada endpoint tiene al menos una prueba de error (RNF-04).

## 6. Orden de implementación

1. Solución, `DbContext`, migración, seed y Docker Compose (domingo 25).
2. Middleware de errores, correlación y RF-11.
3. RF-02/03, RF-04 y RF-05 (lunes 26).
4. RF-07 con el correlativo transaccional y la prueba concurrente (martes 27).
5. RF-08, RF-09 y los ejemplos de Swagger (miércoles 28).
