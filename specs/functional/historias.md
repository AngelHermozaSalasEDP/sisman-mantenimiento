# Historias de usuario

Estado: **borrador v0.1**. Formato: *Como … quiero … para …*, con criterios **Dado / Cuando / Entonces** (Given / When / Then). Las reglas `RN-xx` están en `reglas.md`. Prioridad MoSCoW: M imprescindible, S deseable, C si hay tiempo.

---

## HU-01 · Registrar un MP (RF-01 · M)
**Como** planificador **quiero** registrar un mantenimiento preventivo con su frecuencia **para** que el sistema pueda programarlo.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Un planificador con sesión y datos válidos | Registra el MP | Se crea con código `MP-nnnnnnn` correlativo y estado Nuevo; responde 201 con su ubicación (RN-10) |
| 2 | Una descripción vacía | Registra el MP | 400 con el campo indicado (RN-11) |
| 3 | Frecuencia mensual sin mes | Registra el MP | 400 (RN-13) |
| 4 | Una frecuencia fuera del catálogo | Registra el MP | 400 (RN-12) |

## HU-02 · Consultar MP (RF-02 · M)
**Como** planificador o especialista **quiero** buscar MP por código, tipo y rango de fechas **para** encontrar el que necesito sin cargar toda la lista.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | 600 MP en la base | Consulto sin filtros | Recibo la primera página de 50 con el total (RN-40) |
| 2 | Un código existente | Filtro por ese código | Recibo ese MP |
| 3 | Un especialista | Consulta MP | 200 (RN-03) |

## HU-03 · Editar un MP (RF-03 · M)
**Como** planificador **quiero** corregir un MP **para** mantenerlo al día.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Un MP existente | Cambio la descripción | El MP queda en estado Modificado con mi usuario y la fecha (RN-14) |
| 2 | Un especialista | Intenta editar | 403 (RN-03) |
| 3 | Un MP inexistente | Lo edito | 404 |

## HU-04 · Aprobar un MP (RF-04 · M)
**Como** planificador **quiero** aprobar un MP **para** poder programar sus fechas.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Un MP Nuevo o Modificado | Lo apruebo | Pasa a Aprobado con mi usuario y la fecha (RN-15, RN-16) |
| 2 | Un MP ya Aprobado | Lo apruebo | 409 (RN-15) |

## HU-05 · Programar una fecha (RF-05 · M)
**Como** planificador **quiero** programar una fecha para un MP **para** que de ella nazca la orden de trabajo.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Un MP Aprobado | Programo inicio mañana y fin mañana | Se crea la fecha sin OT y el MP actualiza sus fechas (RN-24) |
| 2 | Un MP no aprobado | Programo una fecha | 409 (RN-22) |
| 3 | Un inicio igual o anterior a hoy | Programo | 400 (RN-20) |
| 4 | Un inicio posterior al fin | Programo | 400 (RN-21) |
| 5 | Ya existe una fecha con ese inicio | Programo otra igual | 409 (RN-23) |

## HU-06 · Eliminar una fecha (RF-06 · S)
**Como** planificador **quiero** quitar una fecha mal programada **para** corregirla.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Una fecha sin OT | La elimino | 204 y deja de existir |
| 2 | Una fecha con OT | La elimino | 409 (RN-25) |

## HU-07 · Generar una OT (RF-07 · M)
**Como** planificador **quiero** generar la orden de trabajo de una fecha **para** que el trabajo se ejecute.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Una fecha sin OT de un MP Aprobado | Genero la OT | Nace Pendiente, Alta, Preventivo, con número único; la fecha queda enlazada (RN-31, RN-35) |
| 2 | Una fecha que ya tiene OT | Genero la OT | 409 (RN-30) |
| 3 | 20 solicitudes simultáneas sobre 20 fechas | Se generan las OT | 20 números distintos, sin huecos ni duplicados (RN-34) |
| 4 | Un fallo al enlazar la fecha | Genero la OT | No queda OT, ni número consumido, ni cambios a medias (RN-33) |

## HU-08 · Consultar OT (RF-08 · M)
**Como** planificador o especialista **quiero** buscar OT con filtros **para** encontrarlas rápido.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | 26 000 OT | Consulto sin filtros | Primera página de 50, en menos de 2 s y menos de 200 KB (RNF-01) |
| 2 | OT con números 999 y 9 000 | Consulto | 9 000 aparece antes que 999 (RN-41) |
| 3 | Filtro por estado y rango de fechas | Consulto | Solo las que cumplen ambos (RN-42) |

## HU-09 · Cambiar el estado de una OT (RF-09 · M)
**Como** planificador **quiero** cambiar el estado de una OT **para** reflejar su avance.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Una OT Pendiente | La paso a Concluido | Queda Concluido con mi usuario y la fecha (RN-37, RN-38) |
| 2 | Una OT Concluida | La reabro | Vuelve a Pendiente (RN-37) |
| 3 | Una OT En programación | La paso a Concluido | 409 (RN-37) |

## HU-10 · Ver el planning (RF-10 · S)
**Como** planificador **quiero** ver cuántos MP hay programados cada día del mes **para** repartir la carga.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Fechas programadas en un mes | Consulto el planning de ese mes y planta | Recibo la cuenta por día de las fechas sin OT (RN-43) |
| 2 | Una fecha con OT | Consulto | No se cuenta (RN-43) |

## HU-11 · Iniciar sesión (RF-11 · M)
**Como** usuario **quiero** iniciar sesión **para** usar la API.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Credenciales válidas | Inicio sesión | Recibo un token que vence en 30 minutos (RN-02) |
| 2 | Credenciales inválidas | Inicio sesión | 401 sin indicar cuál dato falló |
| 3 | Sin token | Llamo a cualquier otro endpoint | 401 (RN-01) |
| 4 | Un token vencido | Llamo | 401 |

## HU-12 · Exportar OT a Excel (RF-12 · C)
**Como** planificador **quiero** exportar la lista de OT filtrada **para** compartirla.

| # | Dado | Cuando | Entonces |
|---|---|---|---|
| 1 | Un filtro con 200 OT | Exporto | Recibo un archivo `.xlsx` con esas 200 filas |
| 2 | Un filtro sin resultados | Exporto | Archivo con solo los encabezados |
