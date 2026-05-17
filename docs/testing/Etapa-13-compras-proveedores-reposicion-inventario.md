# Etapa 13 - Compras, proveedores y reposicion de inventario

Fecha de cierre: 2026-05-17

## Estado

Etapa 13 cerrada.

La etapa implementa compras a proveedores, reposicion automatica de inventario,
actualizacion de costo promedio ponderado, historial de compras, outbox,
consumer Worker y eventos realtime para refresco de UI.

## Alcance implementado

- Modulo backend `Purchasing`.
- Endpoints de proveedores.
- Endpoints de compras.
- Migracion de base de datos para `purchasing` y `PurchaseId` en movimientos.
- Outbox para eventos criticos.
- Consumer Worker para `PurchaseReceivedEventV1`.
- Eventos realtime:
  - `purchase.received`
  - `inventory.updated`
  - `product.costUpdated`
- Movimientos de inventario vinculados con `PurchaseId`.
- Idempotencia al procesar compras recibidas.
- Costo promedio ponderado al recibir mercancia.
- Pantallas frontend:
  - `/suppliers`
  - `/purchases`
  - `/purchases/new`
  - `/purchases/:purchaseId`

## Validacion ejecutada

```txt
dotnet restore SaasCommerce.slnx --source %USERPROFILE%\.nuget\packages
dotnet build SaasCommerce.slnx --no-restore -v minimal
dotnet test SaasCommerce.slnx --no-build --no-restore -v minimal
npm run lint
npm run test
npm run build
```

## Resultados

```txt
Backend build: OK
Backend tests: OK

SharedKernel tests: 2 passed
BuildingBlocks tests: 23 passed
Modules tests: 114 passed
Architecture tests: 28 passed
Worker tests: 23 passed
API tests: 69 passed

Frontend lint: OK
Frontend tests: 53 passed
Frontend build: OK
```

## Nota de entorno

El restore inicial fallo con `NU1301` por bloqueo de red hacia NuGet desde el
entorno de ejecucion. Como las dependencias ya existian en el cache local, se
restauro la solucion usando explicitamente:

```txt
dotnet restore SaasCommerce.slnx --source %USERPROFILE%\.nuget\packages
```

Con los assets restaurados desde cache local, Worker, Architecture, build
completo y test completo quedaron verdes.

## Conclusion

Etapa 13 cerrada. Se puede avanzar a la siguiente etapa.
