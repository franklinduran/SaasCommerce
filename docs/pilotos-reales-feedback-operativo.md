# Etapa 37 - Pilotos reales, feedback y soporte operativo

## Objetivo

Operar ComercioFlow RD con 1 a 3 negocios piloto reales, capturar feedback dentro del sistema y monitorear senales basicas de salud sin romper Clean Architecture, multi-tenancy, API/Worker separados, Outbox ni SignalR por negocio.

## Flujo de feedback beta

Los usuarios piloto reportan casos desde `Feedback Beta`.

Categorias permitidas:

- Bug
- Mejora
- Duda
- Error de datos
- Problema de venta
- Problema de inventario
- Problema de compra
- Problema de caja

Estados permitidos:

- Nuevo
- En revision
- Aceptado
- Rechazado
- Resuelto

Reglas operativas:

- Todo feedback se crea con el `BusinessId` del usuario autenticado.
- La API no acepta `BusinessId` desde frontend.
- La consulta de feedback siempre filtra por `BusinessId`.
- Solo usuarios con permiso de gestion pueden cambiar estado.
- El equipo interno revisa diariamente casos `Nuevo` y `En revision`.

## Auditoria operativa de beta

Durante pilotos, revisar diariamente:

- Login de usuarios piloto.
- Creacion de ventas.
- Ventas en `Processing` por mas de 5 minutos.
- Ventas fallidas.
- Compras recibidas y compras fallidas.
- Movimientos de inventario manuales.
- Cierres de caja con faltante o sobrante.
- Devoluciones y notas de credito.
- Mensajes Outbox pendientes o fallidos.
- Errores de consumers del Worker.

Cada incidencia debe conservar cuando aplique:

- `CorrelationId`
- `BusinessId`
- `UserId`
- `SaleId`
- `PurchaseId`
- `CashRegisterId`
- `MessageId`
- `EventName`
- `ConsumerName`

No registrar tokens, passwords, datos completos de tarjetas ni secretos.

## Metricas minimas

Indicadores diarios por negocio piloto:

- Ventas creadas.
- Ventas completadas.
- Ventas fallidas.
- Compras registradas.
- Compras fallidas.
- Movimientos de inventario.
- Cierres de caja.
- Diferencias de caja.
- Feedback nuevo por categoria.
- Feedback resuelto.
- Usuarios activos.
- Eventos Outbox pendientes.
- Eventos Outbox fallidos.

## Soporte interno rapido

### Reiniciar seed demo

1. Detener API y Worker.
2. Restaurar base local o limpiar datos de prueba segun la guia de despliegue beta.
3. Ejecutar migraciones EF.
4. Iniciar API.
5. Confirmar login demo.
6. Iniciar Worker.

### Validar Worker

1. Abrir logs del Worker.
2. Confirmar que no hay errores repetidos de consumidores.
3. Registrar una venta o compra de prueba.
4. Confirmar que el evento se procesa y cambia el estado.

### Revisar Outbox

1. Buscar mensajes pendientes antiguos.
2. Revisar mensajes fallidos y su `CorrelationId`.
3. Confirmar que no hay acumulacion luego de reiniciar Worker.

### Confirmar SignalR

1. Abrir dos sesiones del mismo negocio.
2. Ejecutar una accion con notificacion, como venta, compra, caja o feedback.
3. Confirmar que la vista se actualiza solo en el negocio correspondiente.

### Venta queda en Processing

1. Buscar la venta por `SaleId`.
2. Revisar Outbox e Inbox asociados al `CorrelationId`.
3. Revisar logs de Worker para el consumidor de inventario, pago o factura.
4. No modificar inventario manualmente hasta confirmar si el mensaje ya fue procesado.
5. Si se requiere correccion, registrar el caso en feedback como `Problema de venta`.

## Criterios para cerrar un ciclo de piloto

- No hay errores criticos abiertos.
- Feedback critico esta resuelto o aceptado con plan.
- No hay eventos Outbox pendientes antiguos.
- Las ventas, compras, inventario, caja y devoluciones completan sin intervencion manual.
- No se detecta fuga de datos entre negocios.
- SonarQube mantiene Quality Gate OK.
