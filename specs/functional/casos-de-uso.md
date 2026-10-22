# Casos de uso

Estado: **borrador v0.1**. Se describen los requisitos de prioridad **M** (9 de 12). Actor principal: planificador, salvo indicación. Las reglas `RN-xx` están en `reglas.md`.

## CU-01 · Registrar un MP (RF-01)
| Campo | Detalle |
|---|---|
| Precondición | El usuario tiene sesión como planificador |
| Flujo principal | 1. Envía los datos del MP. 2. El sistema valida los campos (RN-11, RN-12, RN-13). 3. Obtiene el siguiente código (RN-10). 4. Guarda el MP en estado Nuevo con usuario y fecha (RN-53). 5. Responde 201 |
| Alternativos | 2a. Datos inválidos → 400 con la lista de errores |
| Postcondición | Existe un MP Nuevo con código único |

## CU-02 · Consultar MP (RF-02)
| Campo | Detalle |
|---|---|
| Precondición | Sesión como planificador o especialista |
| Flujo principal | 1. Envía filtros y página. 2. El sistema aplica filtros (RN-42) y pagina (RN-40). 3. Responde 200 con la página y el total |
| Alternativos | 1a. Tamaño de página mayor al máximo → se limita a 200 |

## CU-03 · Editar un MP (RF-03)
| Campo | Detalle |
|---|---|
| Precondición | Sesión como planificador; el MP existe |
| Flujo principal | 1. Envía los cambios. 2. Valida (RN-11 a RN-13). 3. Guarda, deja el estado en Modificado y registra usuario y fecha (RN-14). 4. Responde 200 |
| Alternativos | 3a. El MP no existe → 404 |

## CU-04 · Aprobar un MP (RF-04)
| Campo | Detalle |
|---|---|
| Precondición | Sesión como planificador; el MP existe |
| Flujo principal | 1. Solicita la aprobación. 2. El sistema comprueba que no esté ya Aprobado (RN-15). 3. Lo pasa a Aprobado con usuario y fecha. 4. Responde 200 |
| Alternativos | 2a. Ya está Aprobado → 409 |

## CU-05 · Programar una fecha (RF-05)
| Campo | Detalle |
|---|---|
| Precondición | Sesión como planificador; el MP está Aprobado (RN-22) |
| Flujo principal | 1. Envía inicio y fin. 2. El sistema valida que el inicio sea posterior a hoy (RN-20), que no supere al fin (RN-21) y que no exista otra igual (RN-23). 3. Crea la fecha sin OT. 4. Actualiza las fechas del MP (RN-24). 5. Responde 201 |
| Alternativos | 2a. Cualquier validación falla → 400 o 409 con el motivo |
| Postcondición | La fecha existe sin OT; el MP refleja inicio, próxima y última |

## CU-07 · Generar una OT (RF-07)
| Campo | Detalle |
|---|---|
| Precondición | Sesión como planificador; la fecha existe, no tiene OT y su MP está Aprobado (RN-30) |
| Flujo principal | 1. Solicita generar la OT de una fecha. 2. **Dentro de una transacción**: toma y avanza el correlativo, crea la OT Pendiente, Alta y Preventivo con los datos del MP (RN-31, RN-32), y enlaza la fecha (RN-35). 3. Confirma la transacción. 4. Responde 201 con el número de OT |
| Alternativos | 1a. La fecha ya tiene OT → 409. 2a. Cualquier paso falla → se deshace todo, responde 500 y no consume número (RN-33) |
| Postcondición | Una OT con número único; la fecha enlazada |

## CU-08 · Consultar OT (RF-08)
| Campo | Detalle |
|---|---|
| Precondición | Sesión como planificador o especialista |
| Flujo principal | 1. Envía filtros y página. 2. El sistema filtra (RN-42), ordena por número numérico descendente (RN-41) y pagina (RN-40). 3. Responde 200 |
| Alternativos | 1a. Fechas con formato inválido → 400 (RN-52) |

## CU-09 · Cambiar el estado de una OT (RF-09)
| Campo | Detalle |
|---|---|
| Precondición | Sesión como planificador; la OT existe |
| Flujo principal | 1. Envía el nuevo estado. 2. El sistema comprueba que la transición esté permitida (RN-37). 3. Cambia el estado y registra usuario y fecha (RN-38). 4. Responde 200 |
| Alternativos | 2a. Transición no permitida → 409. 1a. La OT no existe → 404 |

## CU-11 · Iniciar sesión (RF-11)
| Campo | Detalle |
|---|---|
| Actor | Cualquier usuario |
| Flujo principal | 1. Envía usuario y clave. 2. El sistema verifica las credenciales contra el hash guardado. 3. Emite un token con el perfil y vigencia de 30 minutos (RN-02, RN-03). 4. Responde 200 |
| Alternativos | 2a. Credenciales inválidas → 401 sin indicar cuál falló |
