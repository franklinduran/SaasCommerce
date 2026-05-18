# Etapa 15 - Facturacion basica, recibos y comprobantes internos

Fecha de cierre: 2026-05-18

## Estado

Etapa 15 implementada.

La etapa agrega recibos internos no fiscales persistidos en el modulo `Billing`,
con generacion idempotente al completar una venta, endpoints protegidos, eventos
por Outbox, notificaciones SignalR por negocio y UI operativa para listar,
consultar, cancelar e imprimir recibos.

## Alcance implementado

- Modulo backend `Billing` completado para recibos internos.
- Entidad `Invoice` con estado `Draft`, `Issued` y `Cancelled`.
- Numero secuencial interno por negocio con formato `RI-00000001`.
- Relacion logica con `SaleId`.
- Migracion `202605180001_BillingInvoices`.
- Repositorios:
  - `IInvoiceRepository`
  - `IInvoiceSaleReader`
  - `EfInvoiceRepository`
  - `EfInvoiceSaleReader`
- Casos de uso:
  - `GenerateInvoiceCommand`
  - `GenerateInvoiceHandler`
  - `GetInvoiceBySaleQuery`
  - `GetInvoicesQuery`
  - `CancelInvoiceCommand`
- Endpoints:
  - `GET /api/invoices`
  - `GET /api/invoices/{id}`
  - `GET /api/sales/{saleId}/invoice`
  - `POST /api/invoices/{id}/cancel`
- Eventos:
  - `InvoiceGeneratedEventV1`
  - `InvoiceGenerationFailedEventV1`
  - `InvoiceCancelledEventV1`
- SignalR:
  - `invoice.generated`
  - `invoice.cancelled`
- Frontend:
  - `/invoices`
  - `/invoices/:invoiceId`
  - enlace "Ver recibo" desde detalle de venta cuando existe recibo.

## Cambio de flujo de saga

Antes, la saga esperaba generar factura antes de emitir `SaleCompletedEventV1`.

Ahora:

```txt
SaleCreated
-> StockValidationRequested
-> InventoryDeductionRequested
-> PaymentRegistrationRequested
-> SaleCompletedEventV1
-> SaleCompletedConsumer completa la venta
-> GenerateInvoiceHandler genera recibo si la venta esta Completed
-> InvoiceGeneratedEventV1 por Outbox
-> API realtime consumer notifica invoice.generated por business-{BusinessId}
```

Este flujo cumple la regla de no generar recibos para ventas `Failed` o
`Cancelled`, y evita duplicados por `BusinessId + SaleId`.

## Reglas cubiertas

- El frontend no envia `BusinessId`.
- Los endpoints leen `BusinessId` desde el contexto autenticado.
- El consumer usa eventos internos confiables.
- `GET /api/invoices` filtra por tenant.
- `GET /api/sales/{saleId}/invoice` no cruza negocios.
- `GenerateInvoiceHandler` devuelve el recibo existente si ya hay uno para la venta.
- `CancelInvoiceHandler` solo cancela recibos emitidos.
- Los eventos salen por Outbox.
- SignalR notifica solo al grupo de negocio.

## Validacion automatizada ejecutada

Backend:

```txt
dotnet build .\comercioflow\SaasCommerce.slnx -v minimal
dotnet test .\comercioflow\backend\tests\SaasCommerce.Modules.Tests\SaasCommerce.Modules.Tests.csproj -v minimal
dotnet test .\comercioflow\backend\tests\SaasCommerce.Worker.Tests\SaasCommerce.Worker.Tests.csproj -v minimal
dotnet test .\comercioflow\backend\tests\SaasCommerce.Api.Tests\SaasCommerce.Api.Tests.csproj -v minimal
dotnet test .\comercioflow\backend\tests\SaasCommerce.Architecture.Tests\SaasCommerce.Architecture.Tests.csproj -v minimal
dotnet test .\comercioflow\SaasCommerce.slnx -v minimal
```

Resultado:

```txt
Build backend: OK, 0 warnings, 0 errors
Modules tests: 129 passed
Worker tests: 24 passed
API tests: 76 passed
Architecture tests: 28 passed
Suite backend completa: 282 passed
```

Frontend:

```txt
npm.cmd --prefix .\comercioflow\frontend run lint
npm.cmd --prefix .\comercioflow\frontend run test
npm.cmd --prefix .\comercioflow\frontend run build
```

Resultado:

```txt
Lint: OK
Tests: 14 files passed, 56 tests passed
Build: OK
```

## Pruebas agregadas

Backend:

- `GenerateInvoice_ShouldCreateInvoice_WhenSaleIsCompleted`
- `GenerateInvoice_ShouldNotDuplicateInvoice_WhenAlreadyExists`
- `GenerateInvoice_ShouldFail_WhenSaleIsCancelled`
- `GetInvoices_ShouldFilterByBusinessId`
- `CancelInvoice_ShouldChangeStatus_WhenInvoiceIsIssued`
- Contract test de `InvoiceGeneratedEventV1`
- API tests de lista, consulta por venta y cancelacion.
- Worker tests ajustados al nuevo flujo idempotente.

Frontend:

- Lista de recibos renderiza datos.
- Detalle de recibo muestra totales.
- Boton imprimir existe.
- Estado cancelado se muestra correctamente.

## Pendiente fuera de alcance

- NCF fiscal dominicano.
- Secuencias fiscales por tipo de comprobante.
- Anulacion fiscal con reglas DGII.
- Lineas de recibo persistidas como snapshot independiente.
- SonarQube Quality Gate no ejecutado en esta sesion.

## Resultado

La Etapa 15 queda implementada y validada en pruebas automatizadas.

El MVP ya puede generar recibos internos no fiscales al completar ventas,
consultarlos desde API/UI, imprimirlos y cancelarlos de forma controlada.
