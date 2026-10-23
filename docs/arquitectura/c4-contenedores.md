# C4 – Nivel 2: contenedores y componentes

Estado: **borrador v0.1** (2026-10-23).

## Contenedores

```mermaid
flowchart LR
    cli["Cliente<br/>Swagger UI / peticiones HTTP"]
    subgraph docker["docker compose"]
        api["<b>Sisman.Mantenimiento.Api</b><br/>ASP.NET Core, .NET 10<br/>API REST + JWT + OpenAPI"]
        db[("<b>SQL Server 2022</b><br/>Esquema propio<br/>creado por migraciones EF Core")]
    end
    cli -->|"HTTPS, JSON, Bearer JWT"| api
    api -->|"EF Core, TDS"| db
```

| Contenedor | Tecnología | Responsabilidad |
|---|---|---|
| API | ASP.NET Core 10, controladores, FluentValidation, Swashbuckle | Autenticación, validación de entradas, orquestación de casos de uso, `ProblemDetails` |
| Base de datos | SQL Server 2022 en Docker (plan B local mientras Docker no esté instalado) | Datos, restricciones, correlativo transaccional |

La API es un solo contenedor (no microservicios): el módulo es pequeño y el instructor aún no confirmó si se espera más (consulta pendiente). La arquitectura por capas permite separar servicios después.

## Componentes de la API (Clean Architecture, ADR-003)

```mermaid
flowchart TB
    subgraph Api["Api (presentación)"]
        ctrl["Controladores<br/>Auth, MP, Fechas, OT"]
        mw["Middleware<br/>ProblemDetails, correlación"]
        jwt["Autenticación JWT<br/>y políticas por perfil"]
    end
    subgraph App["Application (casos de uso)"]
        uc["Casos de uso<br/>RegistrarMp, AprobarMp, ProgramarFecha,<br/>GenerarOt, ConsultarOt, CambiarEstadoOt"]
        val["Validadores<br/>FluentValidation"]
        ports["Puertos<br/>IMpRepositorio, IOtRepositorio,<br/>ICorrelativo, IReloj, IUnidadDeTrabajo"]
    end
    subgraph Dom["Domain"]
        ent["Entidades y reglas<br/>MantenimientoPreventivo, FechaProgramada,<br/>OrdenTrabajo, Usuario"]
    end
    subgraph Inf["Infrastructure"]
        ef["DbContext, configuraciones,<br/>migraciones, seed"]
        rep["Repositorios y correlativo<br/>(UPDLOCK)"]
        tok["Emisión de JWT, hash de claves"]
    end
    ctrl --> uc
    mw --> ctrl
    jwt --> ctrl
    uc --> val
    uc --> ent
    uc --> ports
    rep -. implementa .-> ports
    ef --> rep
    rep --> ent
    tok -. implementa .-> ports
```

Regla de dependencias: `Api → Application → Domain`; `Infrastructure → Application, Domain`. `Domain` no depende de nadie. `Api` solo conoce `Infrastructure` para registrar los servicios al arrancar.

## Estructura de la solución

```
src/
  Sisman.Mantenimiento.Domain/
  Sisman.Mantenimiento.Application/
  Sisman.Mantenimiento.Infrastructure/
  Sisman.Mantenimiento.Api/
tests/
  Sisman.Mantenimiento.UnitTests/
  Sisman.Mantenimiento.IntegrationTests/   (WebApplicationFactory + SQL Server de prueba)
docker/
  docker-compose.yml
```
