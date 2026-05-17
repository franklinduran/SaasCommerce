# Etapa 9 - Customers API y Sales API

## Objetivo

Exponer endpoints HTTP para Customers y Sales sin mover logica de negocio a API. API recibe requests, valida autenticacion, delega a Application y devuelve `ApiResponse` estandar.

## Endpoints Customers

```txt
POST   /api/customers
GET    /api/customers
GET    /api/customers/{id}
PUT    /api/customers/{id}
DELETE /api/customers/{id}
```

Reglas:

- Requieren JWT.
- `BusinessId` sale de `ICurrentUserService`.
- No aceptan `BusinessId` desde frontend.
- `DELETE` desactiva el cliente.
- Clientes de otro negocio responden `404 NOT_FOUND`.

## Endpoints Sales

```txt
POST /api/sales
GET  /api/sales
GET  /api/sales/{id}
POST /api/sales/{id}/cancel
```

Reglas:

- Requieren JWT.
- `BusinessId`, `BranchId` y `UserId` salen del token.
- `POST /api/sales` calcula precios y total desde Catalog.
- No se confia en precios ni totales enviados por frontend.
- La venta inicia en `Received`.
- `SaleCreatedEventV1` se guarda en Outbox.
- La saga continua el flujo asincrono desde Worker.
- `GET /api/sales` y `GET /api/sales/{id}` filtran por `BusinessId`.
- No se puede cancelar una venta `Completed`.

## Requests Locales

Archivos principales:

```txt
requests/10-customers.http
requests/11-sales.http
requests/12-sales-saga-http-flow.http
```

Tambien se actualizaron:

```txt
requests/05-customers.http
requests/06-sales.http
```

## Tests Agregados

- Application tests para Customers.
- Application tests para Sales HTTP use cases.
- API tests para Customers.
- API tests para Sales.
- Validacion de Outbox al crear venta.
- Validacion de aislamiento por `BusinessId`.
- Ajuste de tests de stock para el flujo real `Sale Received -> Processing`.

## Validacion Esperada

```powershell
$env:NUGET_PACKAGES='C:\Users\frank\.nuget\packages'
dotnet restore .\SaasCommerce.slnx
dotnet build .\SaasCommerce.slnx --no-restore -m:1 /nr:false -v minimal
dotnet test .\SaasCommerce.slnx --no-build -m:1 /nr:false -v minimal
```

Frontend:

```powershell
cd frontend
npm.cmd run lint
npm.cmd run test
npm.cmd run build
```

Docker local:

```powershell
docker compose up --build -d
```

Despues de Docker, ejecutar `requests/10-customers.http`, `requests/11-sales.http` y `requests/12-sales-saga-http-flow.http`.
