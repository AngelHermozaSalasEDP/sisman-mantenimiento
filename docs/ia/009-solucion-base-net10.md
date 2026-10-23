# 009 — Solución base .NET 10: dominio, EF Core, JWT, errores y pruebas

- **Fecha:** 2026-10-23
- **Tarea / requisito:** Fase 3 del plan (solución, EF Core, migración, seed, Docker, ProblemDetails) y RF-11; ADR-001, 003 a 006
- **Tipo:** código, pruebas
- **Herramienta:** Claude Code

## Prompt

«Podemos generar el nuevo sisman-mantenimiento», con los ADR 003 a 006 ya aceptados y las specs técnicas como contexto.

## Respuesta relevante

El asistente creó la solución (4 proyectos y 2 de pruebas), las reglas en el dominio (`Aprobar`, `ProgramarFecha`, `Generar`, `CambiarEstado`), el `DbContext` con las restricciones del modelo, la migración `Inicial`, la semilla (2 usuarios, catálogos, 8 máquinas, 30 MP y 3 OT), el manejo de errores `ProblemDetails`, la correlación, el login con JWT y la política de autorización por defecto. También `docker/` con Compose y Dockerfile.

## Cambio

`src/` (4 proyectos), `tests/` (23 pruebas de dominio y 8 de integración), `docker/`, `README.md`, `.gitignore`, `docs/trazabilidad.md`.

## Decisión

**Aceptado con estas correcciones del propio asistente al revisar:**

1. Un primer intento de escribir los archivos con *heredocs* del shell falló y no creó nada; se reescribieron con la herramienta de edición (mismo problema que la evidencia 003).
2. La versión 10 de Swashbuckle usa Microsoft.OpenApi 2.x: el código de seguridad de Swagger se corrigió tras el primer error de compilación.
3. El paquete transitivo `System.Security.Cryptography.Pkcs` 9.0.11 traía una vulnerabilidad conocida (NU1902/NU1903); se fijó la versión 10.0.13.
4. El login devolvía el perfil como número; se configuró la serialización de enumeraciones como texto para coincidir con el OpenAPI.

**Verificado en ejecución:** migración y semilla contra LocalDB; login correcto, clave incorrecta, usuario inexistente y entrada vacía; 31 pruebas en verde.

**No verificado:** `docker compose up` (Docker no está instalado); el Compose y el Dockerfile están escritos pero sin ejecutar.

## Revisión humana

Pendiente: el responsable debe revisar el modelo de datos y las reglas del dominio (en especial RN-37, las transiciones de estado, que siguen a validar con un usuario).
