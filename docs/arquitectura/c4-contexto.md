# C4 – Nivel 1: contexto del sistema

Estado: **borrador v0.1** (2026-10-23).

```mermaid
flowchart TB
    plan["<b>Planificador de mantenimiento</b><br/>Persona<br/>Crea y aprueba MP, programa fechas,<br/>genera OT y cambia su estado"]
    esp["<b>Especialista de mantenimiento</b><br/>Persona<br/>Consulta MP y OT"]
    sis["<b>API SISMAN Mantenimiento</b><br/>Sistema de software<br/>Flujo MP → fecha → OT"]
    cli["<b>Cliente de la API</b><br/>Swagger UI / colección de peticiones<br/>(la interfaz web queda fuera del alcance)"]
    orig["<b>SISMAN original</b><br/>Sistema existente<br/>Solo referencia; no hay integración"]

    plan -->|"usa, HTTPS + JWT"| cli
    esp -->|"usa, HTTPS + JWT"| cli
    cli -->|"llama a la API REST, JSON"| sis
    sis -. "sus reglas se heredaron del análisis;<br/>sin conexión en ejecución" .-> orig
```

| Elemento | Descripción |
|---|---|
| Planificador, Especialista | Actores de `docs/alcance.md`. El perfil determina qué operaciones puede ejecutar (RN-03). |
| API SISMAN Mantenimiento | El sistema que se construye. Expone una API REST `/api/v1` documentada con OpenAPI. |
| Cliente de la API | En este proyecto: Swagger UI y una colección de peticiones. La interfaz web del producto real es otro trabajo. |
| SISMAN original | Fuente de reglas. No se conecta, no se migran sus datos ni se incluye su código. |

Fuera del contexto por decisión de alcance: seguridad compartida con otros sistemas, rotativa, reportes y la integración con BaaN.
