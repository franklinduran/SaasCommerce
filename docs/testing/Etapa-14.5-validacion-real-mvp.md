# Etapa 14.5 - Validacion Real del MVP

## Ambiente usado

- Fecha: 2026-05-17 America/Santo_Domingo. Eventos del contenedor registrados en UTC 2026-05-18.
- Rama: develop
- Commit: 06ca9cc062c4e84652414801dd9c79a1f1b1de00
- Base de datos: PostgreSQL real en Docker, `saascommerce-postgres`, DB `saas_rd_db`, puerto local `5433`.
- Broker: RabbitMQ real en Docker, `saascommerce-rabbitmq`, puertos `5672` y `15672`.
- API: `http://localhost:8080`, contenedor `saascommerce-api`.
- Worker: contenedor `saascommerce-worker`.
- Frontend: `http://localhost:5173`, contenedor `saascommerce-web`.

Estado final del ambiente:

```txt
docker compose up --build -d: OK
/health/live: Live
/health/ready: Healthy, PostgreSQL Healthy, RabbitMQ Healthy
Frontend HTTP: 200
RabbitMQ queues: 0 ready / 0 unacknowledged, consumers activos
Outbox: Published 189, Pending 0, Failed 0
Tabla de migraciones: public.__EFMigrationsHistory
Tablas Etapa 14: customers, customer_credit_accounts, customer_credit_movements, customer_payments
Logs API/Worker finales: sin [ERR], sin 500, sin Unhandled API error
```

## Comandos ejecutados

- `docker compose up --build -d`: OK.
- `dotnet build SaasCommerce.slnx -v minimal`: OK, 0 warnings, 0 errors.
- `dotnet test SaasCommerce.slnx -v minimal`: OK, 273 tests passed.
- `npm.cmd run lint`: OK.
- `npm.cmd run test`: OK, 13 files passed, 53 tests passed.
- `npm.cmd run build`: OK.
- SonarQube Quality Gate: no ejecutado en esta sesion. El intento de `.\scripts\sonar.ps1` fue bloqueado por politica de aprobacion porque usaria `SONAR_TOKEN` para subir analisis del repo a un host configurado. Requiere autorizacion explicita del usuario.

## Datos de prueba creados

- Negocios:
  - Negocio A: Colmado La Bendicion, `BusinessId=9fb46f32-5288-4946-95bf-a859dd6205db`.
  - Negocio B: Mini Market El Progreso, `BusinessId=bef2bf65-b33f-4470-bb56-864e7592736a`.
- Usuarios:
  - Admin Negocio A: `admin.a.mpagokdy-kk14s@example.com`, rol `Admin`.
  - Admin Negocio B: `admin.b.mpagokdy-kk14s@example.com`, rol `Admin`.
  - Cajeros: bloqueado, no existe endpoint publico/UI para crearlos.
- Sucursales:
  - Negocio A: Sucursal Principal, `BranchId=87dbe293-c3ab-44f5-b568-b801c257609a`.
  - Negocio B: Sucursal Principal, `BranchId=bb5b3207-1da8-421b-a7f4-f7e8e027369a`.
  - Sucursal secundaria: bloqueado, no existe endpoint publico/UI para crearla.
- Productos:
  - Arroz Selecto 10 lb, Aceite Crisol 128 oz, Habichuelas Goya, Leche Rica, Salami Induveca, Pan de agua, Cafe Santo Domingo, Azucar crema, Pasta dental Colgate, Detergente Ariel.
- Proveedores:
  - Distribuidora La Vega, Importadora Duarte, Suplidora El Caribe.
- Clientes:
  - Juan Perez, Maria Rodriguez, Carlos Gomez, Ana Martinez.

## Flujos validados

### Login y seguridad

Resultado: OK parcial.

Evidencia: login de ambos admins OK; `/api/me` devolvio negocio y sucursal correctos; ruta protegida sin token devolvio `401`. Usuario A no pudo leer cliente ni venta de B: `404`. Logout server-side no existe; el logout actual es limpieza de sesion/token en frontend.

### Productos e inventario

Resultado: OK.

Evidencia: se crearon 10 productos para A y 1 producto aislado para B. Se edito `Arroz Selecto 10 lb` a `Arroz Selecto 10 lb Editado`. Inventario inicial aplicado a 10 productos. Ajuste manual dejo Arroz en `4`, filtro de bajo stock devolvio `1`, historial del producto mostro `ManualAdjustment` e `InitialStock`.

### Compras

Resultado: OK.

Evidencia: compra `0b9aba40-8518-498b-a346-c4e8c16334bd` creada en `Draft`, recibida en `Received`. Stock de Aceite Crisol 128 oz subio `50 -> 62`. Historial de inventario contiene 1 movimiento con `PurchaseId`. Reintento de recibir la misma compra devolvio `400` y no duplico el movimiento.

### Venta contado

Resultado: OK.

Evidencia: venta contado `f0528da4-9d93-4f73-8d80-731349bcc5fc` paso `Received -> Completed`. Stock de Leche Rica bajo `50 -> 48`. Total de detalle: `166`. SignalR emitio cambios de estado durante la corrida.

### Venta fiada

Resultado: OK.

Evidencia: intento `Credit` sin cliente devolvio `400` con `The sale request is invalid.` Venta fiada `beb17c13-1dc0-435e-9121-67e3959fa791` completo y genero debito de `285`; balance antes del abono SignalR: `285`, balance despues del abono de `50`: `235`. Se encontro exactamente 1 movimiento `Debit` para esa venta.

### Abonos

Resultado: OK.

Evidencia: cliente Maria Rodriguez quedo con deuda `107` por venta fiada desbloqueada. Abono parcial de `10` dejo balance `97`. Sobre-abono devolvio `400` con `The payment amount exceeds the pending balance.` Abono final de `97` dejo balance `0`. Historial: `Payment`, `Payment`, `Debit`.

### SignalR

Resultado: OK.

Evidencia: conexion SignalR autenticada para A recibio:

```txt
customer.creditBlocked
customer.creditUnblocked
sale.statusChanged Received
sale.statusChanged Completed
customer.creditDebited
customer.paymentRegistered
```

La conexion autenticada para B no recibio eventos de A (`bReceivedAEvents=0`). Los payloads incluyeron `businessId`, `correlationId` cuando aplica, `customerId`, `saleId`, `amount` y `newBalance`.

### Multi-tenancy

Resultado: OK.

Evidencia: listas de productos/clientes no cruzaron datos:

```txt
productsAContainB: false
productsBContainA: false
customersAContainB: false
customersBContainA: false
foreignCustomerByAStatus: 404
foreignSaleByBStatus: 404
```

### Idempotencia

Resultado: OK parcial.

Evidencia: reintento de recibir compra ya recibida devolvio `400`; stock quedo `62` y solo hubo 1 movimiento con `PurchaseId`. Venta fiada tuvo exactamente 1 movimiento `Debit` para el `SaleId`. RabbitMQ quedo sin mensajes ready/unack y Outbox quedo solo con `Published`. No se inyectaron manualmente mensajes duplicados al broker en esta corrida.

### Limite de credito

Resultado: Bloqueado.

Evidencia: no existe endpoint publico/UI para configurar `CreditLimit`. Las cuentas nuevas quedan con limite `0`, interpretado por el dominio actual como credito ilimitado. No se valido rechazo por limite sin tocar DB manualmente.

### Cliente bloqueado

Resultado: OK.

Evidencia: cliente bloqueado quedo `Blocked`; intento de venta fiada devolvio `400` con `The customer's credit account is blocked.` Luego `unblock` dejo `Active`, la venta fiada posterior completo y genero balance `107`.

## Bugs encontrados

| Codigo | Descripcion | Severidad | Estado |
|---|---|---|---|
| E145-001 | `GET /api/inventory?productId&branchId` devolvia 500 por filtro LINQ sobre `BranchId.Value` despues de proyectar. | Alta | Corregido con filtro antes de proyeccion y prueba API de regresion. |
| E145-002 | No existe endpoint publico/UI para crear usuarios cajeros. | Media | Abierto |
| E145-003 | No existe endpoint publico/UI para crear sucursal secundaria. | Media | Abierto |
| E145-004 | No existe endpoint publico/UI para configurar limite de credito. | Alta | Abierto |
| E145-005 | `pageSize=100` en catalogo devuelve 400; el maximo operativo usado fue `50`. | Baja | Documentado |
| E145-006 | Saga de venta depende del polling de Outbox; la validacion real necesita esperas de hasta aproximadamente 30s para `Completed` y debito de credito. | Baja | Documentado |
| E145-007 | SonarQube Quality Gate no pudo ejecutarse sin aprobacion explicita para subir analisis con token configurado. | Media | Pendiente |

## Resultado final

- Aprobado: Parcial. El flujo core del MVP funciona en ambiente real: login, multi-tenancy, productos, inventario, compras, ventas contado, ventas fiadas, abonos, eventos, Outbox, Worker y SignalR.
- Bloqueantes: cajeros, sucursal secundaria, configuracion de limite de credito y SonarQube Quality Gate sin autorizacion explicita.
- Observaciones: no hubo errores criticos finales en API/Worker; RabbitMQ quedo drenado; Outbox quedo publicado. Se corrigio un bug real encontrado durante la etapa y se agrego prueba automatizada para cubrirlo.

## Decision final

La Etapa 14.5 queda aprobada parcialmente.

El core operativo del MVP fue validado correctamente con ambiente real, datos dominicanos, Docker, API, Worker, Web, PostgreSQL, RabbitMQ, Outbox y SignalR.

Se corrigio un bug real encontrado durante la validacion y se agrego prueba de regresion.

La aprobacion total queda condicionada a la Etapa 14.6, donde se cerraran los bloqueantes operativos: gestion de cajeros, sucursal secundaria, configuracion de limite de credito y ejecucion de SonarQube con autorizacion.
