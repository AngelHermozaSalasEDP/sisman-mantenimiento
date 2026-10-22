# Consulta al instructor (borrador para enviar)

Estimado profesor Moyano:

Estoy preparando el proyecto integrador. Quisiera confirmar algunos puntos antes de empezar a construir, para no rehacer trabajo.

**Sobre el tema y el alcance**

1. Mi proyecto es la **modernización de un sistema real de mantenimiento** (SISMAN), no un producto nuevo. Ya hice el análisis del sistema existente (reglas de negocio, datos, seguridad) y lo validé con un usuario. ¿Es un tema válido para el proyecto integrador?
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
