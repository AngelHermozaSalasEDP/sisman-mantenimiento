# ADR-004: Validación con FluentValidation en la capa de aplicación

- **Estado:** Propuesta (2026-10-23), pendiente de aceptación del responsable
- **Decide:** responsable del proyecto, con propuesta del asistente de IA

## Contexto

El sistema original validaba en el navegador (la fecha de inicio posterior a hoy, hallazgo FE-8), de modo que una llamada directa se saltaba la regla. Hay dos tipos de validación distintos: **forma de la entrada** (campo obligatorio, longitud, rango) y **reglas de negocio que dependen del estado** (el MP debe estar aprobado, la fecha no puede repetirse).

## Decisión

1. **Forma de la entrada:** validadores de **FluentValidation** en `Application`, uno por solicitud (`RegistrarMpValidator`, `ProgramarFechaValidator`, …). Fallan con **400** y la lista de campos con error (ver ADR-005).
2. **Reglas de estado:** viven en el dominio y fallan con excepciones de dominio propias que la API traduce a **409** (conflicto) o **404**.
3. La validación se ejecuta en un único punto, antes del caso de uso (filtro de la API o comportamiento del caso de uso), no repartida en cada controlador.
4. El reloj se inyecta (`IReloj`) para que «posterior a hoy» (RN-20) sea probable sin esperar al día siguiente.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| *DataAnnotations* en los DTO | Difícil expresar reglas entre campos (mes obligatorio según frecuencia, RN-13) y condiciones con el reloj |
| Validar solo en el dominio | Mezcla errores de formato con errores de negocio y da mensajes poco útiles por campo |
| Validar en los controladores | Se repite y se olvida en algún endpoint |

## Consecuencias

- (+) Cada regla de entrada (RN-11, RN-12, RN-13, RN-20, RN-21) tiene una prueba unitaria directa.
- (+) Mensajes por campo uniformes.
- (–) Una dependencia más (FluentValidation) y una clase por solicitud.
