# Informe de calidad

Fecha: **2026-10-28** (actualiza la medición del 2026-10-23) · Rama `main` · Medido en el equipo de desarrollo (Windows, .NET SDK 10.0.402, SQL Server LocalDB).

## 1. Resumen

| Aspecto | Resultado |
|---|---|
| Compilación | **0 errores, 0 advertencias** (`dotnet build`) |
| Pruebas | **121 en verde**: 25 de dominio, 85 de integración (API + SQL Server), 11 de la web |
| Estabilidad | 5 corridas completas seguidas sin fallos; el 2026-10-28, 10 corridas seguidas de integración (850 ejecuciones) sin fallos; 1 fallo aislado del 2026-10-23 no reproducido (sección 5) |
| Cobertura de líneas | Dominio 97,6 %, Aplicación 99,3 %, API 98,0 %, Infraestructura 95,9 % (sin migraciones), Web 60,1 % (medida el 2026-10-28) |
| Rendimiento (RNF-01) | 13 a 47 ms y 22,9 KB por página con 26 003 OT (límite 2 s y 200 KB); medido el 2026-10-23, la prueba sigue en verde el 2026-10-28 |
| Concurrencia (RNF-02) | 20 solicitudes simultáneas sin números repetidos; la prueba detecta el defecto del sistema original (prueba de mutación) |
| Paquetes con vulnerabilidades conocidas | **Ninguno**, incluidas dependencias transitivas |
| Secretos en el repositorio y su historial | **Ninguno** (sección 4) |
| Clon limpio | Compila, pruebas en verde y la web arranca sin pasos ocultos (2026-10-23; se repite antes de la entrega) |

## 2. Arquitectura

Referencias entre proyectos, comprobadas en los `.csproj`:

| Proyecto | Depende de | Cumple ADR-003 |
|---|---|---|
| Domain | — | Sí |
| Application | Domain | Sí |
| Infrastructure | Application, Domain | Sí |
| Api | Application, Infrastructure (solo para registrar servicios) | Sí |
| Web | — (cliente HTTP de la API; no referencia sus proyectos) | Sí |

- Las reglas de negocio viven en el dominio (`Aprobar`, `ProgramarFecha`, `QuitarFecha`, `Generar`, `CambiarEstado`); los controladores solo traducen HTTP.
- La validación de entrada está en un solo punto por caso de uso (FluentValidation, ADR-004); las reglas de estado, en el dominio.
- La transacción y el correlativo están en un único puerto (`ITransacciones`) que usan el alta de MP y la generación de OT.
- La web no duplica reglas: las transiciones permitidas de una OT las calcula el dominio y llegan en la respuesta de la API.

## 3. Seguridad

| Control | Cómo |
|---|---|
| Todo exige autenticación | Política por defecto (`FallbackPolicy`); solo el login es anónimo. Probado en cada recurso (401 sin token) |
| Perfiles | Planificador y Especialista con políticas; 403 probado en cada operación de escritura |
| Claves | PBKDF2 (`PasswordHasher`); el JWT vence a los 30 min |
| Clave de firma | Mínimo 256 bits, desde configuración; la API no arranca sin ella |
| SQL | Sin SQL armado con texto: EF Core y dos instrucciones interpoladas que EF convierte en parámetros (`FromSqlInterpolated`, `SqlQuery`) |
| Errores | `ProblemDetails` sin detalles internos; el texto de una excepción no controlada solo va al registro |
| Web | Cookie `HttpOnly` y `SameSite=Strict`; token antifalsificación en cada formulario (probado); el token de la API no llega al navegador |

## 4. Búsqueda de secretos

- Se revisó **todo el historial** (`git log -p --all`; última revisión el 2026-10-28) buscando asignaciones de `password`, `pwd`, `secret`, `apikey` y `token` con un valor literal. Único resultado: la constante `ClaimToken = "api_token"`, que es el nombre de un dato, no un secreto.
- Las claves de ejemplo usadas en el equipo de desarrollo no aparecen en el historial.
- En el repositorio solo hay marcadores: `appsettings.example.json`. Las claves de las pruebas automáticas son de prueba y están a la vista en `tests/`.
- `.gitignore` excluye `.env`, `appsettings.Development.json` y cualquier otro `appsettings*.json`, salvo los dos de configuración base (sin secretos) y el de ejemplo.

## 5. Pendientes y limitaciones conocidas

| # | Tema | Estado |
|---|---|---|
| 1 | Un fallo aislado en 1 de 84 pruebas de integración, con la API y la web de desarrollo corriendo a la vez; no se capturó cuál fue | No se repitió en 5 corridas ni en 10 corridas seguidas el 2026-10-28. Registrado en la evidencia de IA 014 |
| 2 | Docker no se usa en el proyecto (decisión del 2026-10-26, ADR-007) | La ejecución y las pruebas son con SQL Server local o LocalDB |
| 3 | Cobertura de la web de 60,1 %: las páginas se probaron también con dos recorridos HTTP (39 comprobaciones) que no cuentan en la cobertura | Aceptado |
| 4 | Paquetes de prueba con versiones más nuevas (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector`) | Sin vulnerabilidades; se dejan como están para no mover herramientas antes de la entrega |
| 5 | La edición de MP no envía un token de versión del cliente: la concurrencia se detecta solo dentro de la misma solicitud | Documentado en la evidencia 010 |
| 6 | Reglas a validar con usuarios de planta: RN-15, RN-16, RN-22, RN-23, RN-37 | Decididas por el responsable el 2026-10-26 y confirmadas el 2026-10-28 (ver `specs/functional/reglas.md`) |

## 6. Cómo repetir las mediciones

```
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage"
dotnet list package --vulnerable --include-transitive
git log -p --all | grep -i -E "(password|pwd|secret|apikey|token)\s*[=:]\s*['\"][^'\"<{$]{6,}"
```
