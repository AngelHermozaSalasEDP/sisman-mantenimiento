# Consulta al instructor (enviada; respuesta recibida el 2026-10-24)

Estimado profesor Moyano:

Estoy preparando el proyecto integrador. Quisiera confirmar algunos puntos antes de empezar a construir, para no rehacer trabajo.

**Sobre el tema y el alcance**

1. Mi proyecto es la **modernización de un sistema real de mantenimiento** (SISMAN), no un producto nuevo. Ya hice el análisis del sistema existente (reglas de negocio, datos, seguridad) y lo validé con un usuario. 
2. Pienso acotar el entregable a **un módulo**: mantenimiento preventivo → fecha programada → orden de trabajo, construido como una API nueva con Clean Architecture, EF Core y OpenAPI. ¿Le parece un alcance adecuado?
3. El sistema original es de una empresa y contiene código y datos internos. ¿Basta con entregar **solo el módulo nuevo** en un repositorio propio, con una base de ejemplo (datos ficticios) en Docker, y mencionar el sistema original como contexto?
4. Mi análisis previsto incluye una arquitectura de microservicios para el sistema completo. ¿Se espera microservicios, o basta un módulo bien hecho?

**Sobre la entrega**

5. Fecha de entrega y de presentación.
6. Nota mínima aprobatoria y escala.
7. ¿El trabajo es individual o grupal?
8. Medio de entrega (enlace al repositorio, ZIP, plataforma).
9. ¿Es obligatorio Claude Code o se admite otro asistente?
10. ¿Hay motor de base de datos preferido y es obligatorio Docker?

**Sobre el uso de EF Core**

11. La base real ya existe y tiene procedimientos almacenados con lógica delicada (por ejemplo, el correlativo de órdenes de trabajo). Pienso usar EF Core con migraciones para un esquema propio del módulo en Docker. ¿Es aceptable?

Gracias.

---

## Respuesta del instructor (recibida el 2026-10-24)

| # | Pregunta | Respuesta | Efecto en el proyecto |
|---|---|---|---|
| 1 | ¿Tema válido? | Sí, el tema es libre si permite aplicar especificación, arquitectura, API y pruebas. | Sin cambios. |
| 2 | ¿Alcance de un módulo (MP → fecha → OT)? | Adecuado: mejor un módulo bien especificado, diseñado y probado. | Confirma ADR-002. |
| 3 | ¿Solo el módulo nuevo, con datos ficticios en Docker? | Sí. El sistema original solo como contexto; nunca subir secretos, claves ni cadenas de conexión reales. | Confirma la separación de repositorios. |
| 4 | ¿Microservicios? | No se requieren. Basta API REST .NET 10 con Clean Architecture (o capas justificadas). | El entregable sigue siendo un monolito modular. |
| 5-6 | Fecha y escala | Exposición y entrega el sábado 31 de octubre; ponderaciones según el documento del proyecto. | Sin cambios. |
| 7 | ¿Individual o grupal? | Individual. | Sin cambios. |
| 8 | Medio de entrega | Repositorio Git con `src/`, `tests/`, `specs/`, `docs/`, `docker/` y README (compilar, ejecutar, probar), más documento técnico, evidencias de IA y PPT. | Revisar el README contra esta lista. |
| 9 | ¿Claude Code obligatorio? | Sí. | Sin cambios. |
| 10 | Motor y Docker | SQL Server, MySQL o PostgreSQL con EF Core. Docker muy recomendado y parte de la estructura esperada. | **Pendiente: ejecutar el Docker Compose al menos una vez** (hoy está escrito pero no probado). |
| 11 | ¿EF Core con migraciones y esquema propio? | Aceptable; respeta Clean Architecture y separación de responsabilidades. | Confirma ADR-001. |

Recomendación final del instructor: documentar bien las especificaciones y las evidencias de IA conforme se avance.
