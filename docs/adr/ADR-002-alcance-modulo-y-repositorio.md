# ADR-002: Entregable acotado a un módulo, en un repositorio propio

- **Estado:** Aceptada (2026-10-22), pendiente de confirmar con el instructor
- **Decide:** responsable del proyecto, con propuesta del asistente de IA

## Contexto

El plan de trabajo del curso prevé unas 6 semanas y «un módulo bien cerrado»; los requisitos dicen que un módulo bien especificado vale más que muchos incompletos. El proyecto de modernización de SISMAN estima entre 68 y 92 días-persona con microservicios. Además, el repositorio de modernización contiene el código y la configuración del sistema original, que son de la empresa (y un archivo versionado con una clave que debe rotarse).

## Decisión

1. El entregable del curso es el flujo **MP → fecha programada → OT**.
2. Vive en un **repositorio separado y limpio** (`sisman-mantenimiento`), sin código ni datos del sistema original.
3. El resto de SISMAN (reportes, rotativa, seguridad compartida, otras pantallas) queda como **mejoras pendientes**.
4. El análisis del sistema original se reutiliza como fuente de las especificaciones, citando el origen de cada regla, sin copiar su código.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| Entregar todo SISMAN | No cabe en el tiempo del curso |
| Reorganizar el repositorio de modernización | Arrastraría el sistema original y su historial (con un secreto) |
| Empezar un tema nuevo (por ejemplo, Help Desk) | Se pierde el análisis ya hecho y el valor real del producto |

## Consecuencias

- (+) Alcance realista, repositorio ejecutable fuera de la empresa y sin riesgo de secretos.
- (–) Dos repositorios que mantener alineados; el módulo del curso no sustituye al plan de producto.
