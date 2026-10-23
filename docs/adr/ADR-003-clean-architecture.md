# ADR-003: Clean Architecture con cuatro proyectos y pruebas separadas

- **Estado:** Propuesta (2026-10-23), pendiente de aceptación del responsable
- **Decide:** responsable del proyecto, con propuesta del asistente de IA

## Contexto

El curso pide Clean Architecture. Las reglas de negocio del módulo (transiciones de estado, correlativo de OT, validación de fechas) son lo que se evalúa y lo que el sistema original tenía mezclado con las pantallas y los procedimientos almacenados. Hay ocho días para entregar.

## Decisión

Cuatro proyectos de producción y dos de pruebas (ver `docs/arquitectura/c4-contenedores.md`):

| Proyecto | Contiene | Depende de |
|---|---|---|
| `Domain` | Entidades, objetos de valor, reglas | nada |
| `Application` | Casos de uso, DTOs, validadores, puertos (interfaces) | `Domain` |
| `Infrastructure` | EF Core, migraciones, repositorios, JWT, hash de claves | `Application`, `Domain` |
| `Api` | Controladores, middleware, configuración, OpenAPI | `Application`, `Infrastructure` (solo para registrar servicios) |

Las reglas viven en el dominio (`Aprobar`, `ProgramarFecha`, `CambiarEstado`), de modo que se prueban sin base de datos ni HTTP. Los controladores solo traducen HTTP a casos de uso.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| Un solo proyecto por carpetas | No hace cumplir la regla de dependencias; las capas se mezclan |
| Una capa más (`Contracts`, `Shared`, CQRS con MediatR) | Más piezas de las que el módulo necesita en ocho días |
| Repositorio genérico sobre `DbContext` | Oculta las consultas; se usan repositorios por agregado con métodos concretos |

## Consecuencias

- (+) Las reglas RN-xx de dominio se cubren con pruebas unitarias rápidas.
- (+) La capa de infraestructura se cambia (por ejemplo a PostgreSQL) sin tocar reglas.
- (–) Más archivos y asignaciones entre entidades y DTOs que en un proyecto simple.
