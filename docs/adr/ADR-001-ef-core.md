# ADR-001: Usar Entity Framework Core en el módulo

- **Estado:** Aceptada (2026-10-22)
- **Decide:** responsable del proyecto, con propuesta del asistente de IA
- **Reemplaza:** la decisión A4 del proyecto de modernización («ADO.NET, sin ORM»), solo para este módulo

## Contexto

El curso exige SQL Server, MySQL o PostgreSQL con Entity Framework Core, y migraciones y datos de ejemplo en la Fase 3. El sistema original usa ADO.NET con SQL escrito a mano y procedimientos almacenados. Su base de datos real no se puede clonar (es de la empresa), tiene 4 tablas sin clave primaria y le faltan 33 de los 47 procedimientos que el código usa.

## Decisión

1. El módulo usa **EF Core**, con un esquema propio creado por **migraciones** en una base SQL Server en **Docker**, con **datos de ejemplo** (seed).
2. Las lecturas y las operaciones simples se hacen con EF Core (`AsNoTracking` en consultas, paginación en el servidor).
3. La generación del número de OT usa una **transacción** de EF Core con un correlativo protegido por restricción de unicidad.
4. Cualquier operación que en el sistema original dependa de un procedimiento almacenado se reimplementa en C# y se prueba; no se llama a procedimientos del sistema original.

> **Nota (2026-10-28):** el proyecto no usa Docker; ver [ADR-007](ADR-007-sin-docker.md). La base es SQL Server local o LocalDB y se ejecuta con `dotnet run`.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| Mantener ADO.NET sin ORM | No cumple el mínimo del curso |
| *Database-first* sobre la base real | No genera migraciones; el proyecto no se podría clonar y ejecutar fuera de la empresa |
| Llamar a los procedimientos existentes | Faltan 33; no se pueden probar ni compartir |

## Consecuencias

- (+) Cumple el mínimo del curso, el proyecto se clona y se ejecuta con `docker compose up` (sustituido por `dotnet run` con SQL Server local o LocalDB, ADR-007), y las pruebas de integración usan una base propia.
- (+) El correlativo de OT queda dentro de una transacción con restricción de unicidad.
- (–) El esquema del módulo queda separado del de la base real; alinearlos o migrar datos después es trabajo aparte.
- (–) Hay que reimplementar en C# la lógica de los procedimientos que el módulo necesite.
