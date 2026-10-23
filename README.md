# SISMAN Mantenimiento — API de mantenimiento preventivo y órdenes de trabajo

Proyecto integrador del curso *Agentic Software Development (.NET 10)*. Es el módulo central de la modernización del sistema SISMAN (gestión de mantenimiento de una editora): **mantenimiento preventivo (MP) → fecha programada → orden de trabajo (OT)**.

> **Estado:** solución base compilable (2026-10-23): dominio, EF Core con migración y datos de ejemplo, inicio de sesión con JWT (RF-11), errores `ProblemDetails` y correlación. Los casos de uso RF-02 a RF-09 se implementan del 26 al 28/10. Ver el [plan de trabajo](docs/plan-de-trabajo.md).

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
src/            Domain, Application, Infrastructure, Api
tests/          UnitTests (dominio) e IntegrationTests (API + SQL Server)
specs/          functional/ y technical/
docs/           alcance, convenciones, trazabilidad, adr/, ia/, arquitectura/, api/
docker/         Docker Compose (SQL Server 2022 + API) y Dockerfile
```

## Cómo ejecutar

Requisitos: .NET SDK 10 y una instancia de SQL Server (Docker, SQL Server local o LocalDB).

**Opción A: Docker Compose**

```
cd docker
copy .env.example .env      # y edite las cuatro claves
docker compose up --build
```

La API queda en http://localhost:8080/swagger. Al arrancar aplica las migraciones y carga los datos de ejemplo.

**Opción B: SQL Server local o LocalDB (sin Docker)**

```
cd src/Sisman.Mantenimiento.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Clave" "<32 caracteres o más>"
dotnet user-secrets set "ConnectionStrings:Sisman" "Server=(localdb)\MSSQLLocalDB;Database=SismanMantenimiento;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Migraciones:AplicarAlArrancar" "true"
dotnet user-secrets set "Semilla:Habilitada" "true"
dotnet user-secrets set "Semilla:ClavePlanificador" "<clave de prueba>"
dotnet user-secrets set "Semilla:ClaveEspecialista" "<clave de prueba>"
dotnet run
```

Las migraciones también se pueden aplicar a mano: `dotnet tool restore` y `dotnet ef database update -p src/Sisman.Mantenimiento.Infrastructure -s src/Sisman.Mantenimiento.Api` (la cadena sale de `SISMAN_CONNECTION`, o LocalDB si no existe).

**Pruebas**

```
dotnet test
```

Las pruebas de integración crean una base propia (`SismanPruebas_<guid>`) y la borran al terminar. Usan LocalDB, o la instancia indicada en `SISMAN_TEST_CONNECTION` (sin nombre de base).

## Credenciales de prueba

Los usuarios `planificador` (todas las operaciones) y `especialista` (solo consultas) los crea la semilla con las claves que usted defina (`Semilla:ClavePlanificador` y `Semilla:ClaveEspecialista`, o `CLAVE_PLANIFICADOR` y `CLAVE_ESPECIALISTA` en Docker). Son claves de ejemplo para datos ficticios. **No hay claves, tokens ni cadenas de conexión reales en este repositorio**; el archivo `appsettings.example.json` solo muestra los nombres de los valores.

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
