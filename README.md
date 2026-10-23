# SISMAN Mantenimiento — API de mantenimiento preventivo y órdenes de trabajo

Proyecto integrador del curso *Agentic Software Development (.NET 10)*. Es el módulo central de la modernización del sistema SISMAN (gestión de mantenimiento de una editora): **mantenimiento preventivo (MP) → fecha programada → orden de trabajo (OT)**.

> **Estado (2026-10-23):** los 12 requisitos funcionales y los 6 no funcionales están implementados y probados: API REST (16 operaciones) y web de demostración. 119 pruebas en verde; 0 advertencias. Ver la [matriz de trazabilidad](docs/trazabilidad.md) y el [informe de calidad](docs/calidad.md).

## Alcance

Un módulo acotado. Ver [`docs/alcance.md`](docs/alcance.md). Lo que queda fuera (reportes, rotativa, seguridad compartida, resto de pantallas) está listado como mejoras pendientes.

## Funciones

| RF | Función | API | Web |
|---|---|---|---|
| RF-11 | Iniciar sesión (JWT, dos perfiles) | `POST /api/v1/auth/login` | `/login` |
| RF-01 | Registrar un MP con código `MP-nnnnnnn` | `POST /api/v1/mantenimientos-preventivos` | `/mp/nuevo` |
| RF-02 | Consultar MP con filtros y paginación | `GET /api/v1/mantenimientos-preventivos` | `/mp` |
| RF-03 | Editar un MP | `PUT /api/v1/mantenimientos-preventivos/{id}` | `/mp/editar/{id}` |
| RF-04 | Aprobar un MP | `POST …/{id}/aprobacion` | `/mp/{id}` |
| RF-05 | Programar una fecha | `POST …/{id}/fechas` | `/mp/{id}` |
| RF-06 | Eliminar una fecha sin OT | `DELETE …/{id}/fechas/{fechaId}` | `/mp/{id}` |
| RF-07 | Generar una OT con número transaccional | `POST /api/v1/fechas/{id}/ordenes-trabajo` | `/mp/{id}` |
| RF-08 | Consultar OT con filtros y paginación | `GET /api/v1/ordenes-trabajo` | `/ot` |
| RF-09 | Cambiar el estado de una OT | `PUT /api/v1/ordenes-trabajo/{id}/estado` | `/ot/{id}` |
| RF-10 | Planning por día | `GET /api/v1/planning` | `/planning` |
| RF-12 | Exportar OT a Excel | `GET /api/v1/ordenes-trabajo/exportacion` | `/ot` (Exportar a Excel) |

Contrato completo: [`docs/api/openapi.json`](docs/api/openapi.json). Peticiones de ejemplo: [`docs/api/sisman.http`](docs/api/sisman.http).

## Stack

| Capa | Tecnología |
|---|---|
| Plataforma | .NET 10, ASP.NET Core (API REST) |
| Arquitectura | Clean Architecture: Domain, Application, Infrastructure, Api |
| Datos | SQL Server + Entity Framework Core (ver [ADR-001](docs/adr/ADR-001-ef-core.md)) |
| Validación y errores | FluentValidation, ProblemDetails |
| Autenticación | JWT |
| Documentación de la API | OpenAPI / Swagger |
| Pruebas | xUnit, `WebApplicationFactory` (119 pruebas) |
| Entorno local | Docker Compose (SQL Server) |

## Estructura

```
src/            Domain, Application, Infrastructure, Api y Web (web mínima de demostración)
tests/          UnitTests (dominio), IntegrationTests (API + SQL Server) y WebTests (web con la API simulada)
specs/          functional/ y technical/
docs/           alcance, convenciones, trazabilidad, adr/, ia/, arquitectura/, api/
docker/         Docker Compose (SQL Server 2022 + API + web) y Dockerfile
```

## Cómo ejecutar

Requisitos: .NET SDK 10 y una instancia de SQL Server (Docker, SQL Server local o LocalDB).

**Opción A: Docker Compose**

```
cd docker
copy .env.example .env      # y edite las cuatro claves
docker compose up --build
```

La API queda en http://localhost:8080/swagger y la web en http://localhost:8081. Al arrancar, la API aplica las migraciones y carga los datos de ejemplo. (El Compose está escrito pero no se ha ejecutado: en el equipo de desarrollo no hay Docker.)

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

Con la API en marcha (en http://localhost:5300), la web mínima se levanta en otra terminal:

```
dotnet run --project src/Sisman.Mantenimiento.Web --urls http://localhost:5400
```

Abre http://localhost:5400 e inicia sesión con un usuario de prueba. La dirección de la API se cambia con `Api:UrlBase` (variable `Api__UrlBase`).

Las migraciones también se pueden aplicar a mano: `dotnet tool restore` y `dotnet ef database update -p src/Sisman.Mantenimiento.Infrastructure -s src/Sisman.Mantenimiento.Api` (la cadena sale de `SISMAN_CONNECTION`, o LocalDB si no existe).

**Pruebas**

```
dotnet test
```

Las pruebas de la web no necesitan la API ni la base: usan una API simulada. Las pruebas de integración crean una base propia (`SismanPruebas_<guid>`) y la borran al terminar. Usan LocalDB, o la instancia indicada en `SISMAN_TEST_CONNECTION` (sin nombre de base).

## Credenciales de prueba

Los usuarios `planificador` (todas las operaciones) y `especialista` (solo consultas) los crea la semilla con las claves que usted defina (`Semilla:ClavePlanificador` y `Semilla:ClaveEspecialista`, o `CLAVE_PLANIFICADOR` y `CLAVE_ESPECIALISTA` en Docker). Son claves de ejemplo para datos ficticios. **No hay claves, tokens ni cadenas de conexión reales en este repositorio**; el archivo `appsettings.example.json` solo muestra los nombres de los valores.

## Evidencia de uso de IA

Registro en [`docs/ia/`](docs/ia/). Cada entrada indica el prompt, la respuesta relevante, el cambio y si se aceptó, corrigió o rechazó, con el motivo.

## Documentos

- [Alcance](docs/alcance.md)
- [Requisitos funcionales y no funcionales](specs/functional/requisitos.md)
- [Historias de usuario](specs/functional/historias.md) · [Casos de uso](specs/functional/casos-de-uso.md) · [Reglas de negocio](specs/functional/reglas.md)
- [Plan de trabajo](docs/plan-de-trabajo.md)
- [Documento técnico](docs/documento-tecnico.md) · [Informe de calidad](docs/calidad.md)
- [Arquitectura: modelo y C4](docs/arquitectura/)
- [Matriz de trazabilidad](docs/trazabilidad.md)
- [Convenciones de commits y trabajo](docs/convenciones.md)
- [Decisiones de arquitectura](docs/adr/)
- [Consulta al instructor](docs/consulta-instructor.md)
