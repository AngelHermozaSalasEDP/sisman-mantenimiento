# SISMAN Mantenimiento — API de mantenimiento preventivo y órdenes de trabajo

Proyecto integrador del curso *Agentic Software Development (.NET 10)*. Es el módulo central de la modernización del sistema SISMAN (gestión de mantenimiento de una editora): **mantenimiento preventivo (MP) → fecha programada → orden de trabajo (OT)**.

> **Estado:** en construcción. Fase actual: especificaciones (Fase 1 del [plan de trabajo](docs/plan-de-trabajo.md)): requisitos, historias, casos de uso y reglas en borrador. No hay código todavía.

## Alcance

Un módulo acotado. Ver [`docs/alcance.md`](docs/alcance.md). Lo que queda fuera (reportes, rotativa, seguridad compartida, resto de pantallas) está listado como mejoras pendientes.

## Stack previsto

| Capa | Tecnología |
|---|---|
| Plataforma | .NET 10, ASP.NET Core (API REST) |
| Arquitectura | Clean Architecture: Domain, Application, Infrastructure, Api |
| Datos | SQL Server + Entity Framework Core (ver [ADR-001](docs/adr/ADR-001-ef-core.md)) |
| Validación y errores | FluentValidation, ProblemDetails |
| Autenticación | JWT |
| Documentación de la API | OpenAPI / Swagger |
| Pruebas | xUnit, `WebApplicationFactory` |
| Entorno local | Docker Compose (SQL Server) |

## Estructura

```
src/            Domain, Application, Infrastructure, Api   (pendiente)
tests/          pruebas unitarias y de integración         (pendiente)
specs/          functional/ y technical/
docs/           alcance, convenciones, trazabilidad, adr/, ia/, arquitectura/, api/
docker/         Docker Compose                              (pendiente)
```

## Cómo ejecutar

Pendiente (Fase 3). Se completará con: requisitos, `docker compose up`, migraciones, `dotnet run`, `dotnet test`.

## Credenciales de prueba

Pendiente (Fase 4). Serán solo de ejemplo y se documentarán aquí. **No hay claves, tokens ni cadenas de conexión reales en este repositorio.**

## Evidencia de uso de IA

Registro en [`docs/ia/`](docs/ia/). Cada entrada indica el prompt, la respuesta relevante, el cambio y si se aceptó, corrigió o rechazó, con el motivo.

## Documentos

- [Alcance](docs/alcance.md)
- [Requisitos funcionales y no funcionales](specs/functional/requisitos.md)
- [Historias de usuario](specs/functional/historias.md) · [Casos de uso](specs/functional/casos-de-uso.md) · [Reglas de negocio](specs/functional/reglas.md)
- [Plan de trabajo](docs/plan-de-trabajo.md)
- [Matriz de trazabilidad](docs/trazabilidad.md)
- [Convenciones de commits y trabajo](docs/convenciones.md)
- [Decisiones de arquitectura](docs/adr/)
- [Consulta al instructor](docs/consulta-instructor.md)
