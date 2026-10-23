# ADR-005: Errores como ProblemDetails con códigos estables

- **Estado:** Aceptada (2026-10-23)
- **Decide:** responsable del proyecto, con propuesta del asistente de IA

## Contexto

El sistema original mostraba el texto de la excepción al usuario y, en varios botones, **informaba éxito aunque el procedimiento hubiera fallado** (hallazgo 12). Se necesitan errores uniformes, sin detalles internos y que un cliente pueda distinguir por código.

## Decisión

1. Todo error se devuelve como `application/problem+json` (RFC 9457) con `type`, `title`, `status`, `detail`, `traceId`, `correlationId` y un campo `codigo` estable.
2. Un **middleware global** (`IExceptionHandler`) traduce las excepciones; los controladores no atrapan excepciones para convertirlas en éxito.
3. Traducción:

| Situación | Estado HTTP | Código |
|---|---|---|
| Entrada inválida (FluentValidation) | 400 | `validacion.error` (con `errors` por campo) |
| Sin token o token vencido | 401 | `auth.no-autenticado` |
| Credenciales inválidas | 401 | `auth.credenciales-invalidas` |
| Perfil sin permiso | 403 | `auth.sin-permiso` |
| Recurso inexistente | 404 | `mp.no-encontrado`, `fecha.no-encontrada`, `ot.no-encontrada` |
| MP no aprobado / ya aprobado | 409 | `mp.no-aprobado`, `mp.ya-aprobado` |
| Fecha repetida / fecha con OT | 409 | `fecha.duplicada`, `fecha.con-ot` |
| Transición de estado no permitida | 409 | `ot.transicion-invalida` |
| Edición concurrente (`rowversion`) | 409 | `concurrencia.conflicto` |
| Cualquier otra falla | 500 | `error.interno` (sin texto de la excepción; el detalle va al registro) |

4. Cada solicitud recibe un `X-Correlation-Id` (se acepta el del cliente o se genera) que aparece en la respuesta, en `ProblemDetails` y en todas las líneas del registro (RN-54).
5. Si una operación falla, nunca se devuelve 2xx (RN-51); hay una prueba de error por endpoint.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| Formato propio `{ ok, mensaje }` | No es estándar; los clientes y Swagger ya entienden `ProblemDetails` |
| Códigos de estado sin código estable | El texto cambia; el cliente no puede decidir por él |
| `try/catch` en cada controlador | Se repite y es donde nace el «éxito falso» |

## Consecuencias

- (+) Una sola forma de error documentada en OpenAPI; pruebas de error simples.
- (+) Sin fugas de detalles internos (RNF-03, RNF-04).
- (–) Hay que mantener la tabla de códigos al agregar casos de uso.
