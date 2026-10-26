# ADR-007: Sin Docker en el proyecto

- **Estado:** Aceptada (2026-10-26)
- **Decide:** responsable del proyecto

## Contexto

Las ADR-001 y ADR-006 y los documentos técnicos previos suponían un entorno con Docker Compose (SQL Server 2022, API y web). Docker nunca se instaló en el equipo de desarrollo, así que el Compose nunca se ejecutó. El instructor lo describió como recomendado y parte de la estructura esperada. El responsable decidió no usarlo en este proyecto.

## Decisión

1. El proyecto no usa Docker. La base de datos es SQL Server local o LocalDB, y la API y la web se ejecutan con `dotnet run`.
2. Las pruebas de integración usan la instancia indicada en `SISMAN_TEST_CONNECTION` o LocalDB.
3. No se mantiene la carpeta `docker/` ni los archivos que dependían de ella (`docker-compose.yml`, `Dockerfile`, `.env.example`).
4. Este ADR reemplaza las menciones a Docker en el contexto y las consecuencias de ADR-001 y ADR-006. Las decisiones de EF Core y de JWT no cambian.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| Mantener `docker/` sin ejecutar | Afirma un entorno que no se probó |
| Instalar Docker Desktop | Decisión del responsable: no se usa en este proyecto |

## Consecuencias

- (+) El README describe un solo camino de ejecución, que sí se probó.
- (–) No se puede demostrar un arranque desde cero con un solo comando; el evaluador debe tener SQL Server local o LocalDB.
- (–) Si el instructor exige `docker/` en la entrega, habrá que revisar esta decisión.
