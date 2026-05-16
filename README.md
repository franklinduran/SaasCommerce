# SaasCommerce RD

SaasCommerce RD es un SaaS para comercios dominicanos. Esta base deja listo el repositorio para construir el MVP con Clean Architecture, SOLID, API y Worker separados, multi-tenancy, mensajeria, tiempo real, Sonar y pruebas automatizadas.

## Arquitectura Base

```txt
backend/
  src/
    Api/       ASP.NET Core Web API
    Worker/    Worker Service para eventos y tareas de fondo
    SharedKernel/
    BuildingBlocks/
    Modules/
      Tenancy/
      Identity/
      Catalog/
      Inventory/
      Sales/
      Customers/
      Billing/
      Payments/
      Reporting/

  tests/
    SaasCommerce.Api.Tests/
    SaasCommerce.Architecture.Tests/
    SaasCommerce.BuildingBlocks.Tests/
    SaasCommerce.Modules.Tests/
    SaasCommerce.SharedKernel.Tests/
    SaasCommerce.Worker.Tests/

frontend/      React + Vite + TypeScript
```

La regla principal esta en `ARCHITECTURE_RULES.md`: ningun cambio debe romper Clean Architecture, separacion por contextos, multi-tenancy ni el flujo orientado a eventos.

La base backend esta pensada como monolito modular preparado para extraer microservicios mas adelante. `Api` y `Worker` son hosts; `Modules` contiene el negocio; `BuildingBlocks` contiene abstracciones e infraestructura compartida; `SharedKernel` queda pequeno y solo tiene conceptos universales.

## Stack Oficial

- Backend: .NET 10, ASP.NET Core Web API, Worker Service, EF Core, PostgreSQL, MassTransit, RabbitMQ, SignalR, FluentValidation, Serilog y JWT.
- Frontend: React, Vite, TypeScript, TailwindCSS, ShadCN-style UI, TanStack Query, Zustand, React Hook Form, Zod y SignalR client.
- Testing: xUnit, FluentAssertions, NSubstitute, NetArchTest, Testcontainers y MassTransit Test Harness.
- Local: Docker Compose con PostgreSQL, RabbitMQ, API, Worker y Web.

## Comandos Principales

Backend:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet run --project backend/src/Api/SaasCommerce.Api.csproj
dotnet run --project backend/src/Worker/SaasCommerce.Worker.csproj
```

Frontend:

```bash
cd frontend
npm install
npm run dev
npm run test
npm run build
```

En Windows con PowerShell restringido, usa `npm.cmd`:

```bash
npm.cmd run dev
npm.cmd run test
npm.cmd run build
```

Docker:

```bash
docker compose up --build
```

SonarQube / SonarCloud:

```powershell
# Levantar SonarQube local
# Usa SonarQube Community Build 25.1.0.102122 con PostgreSQL.
docker compose -f docker-compose.sonar.yml up -d

[Environment]::SetEnvironmentVariable("SONAR_HOST_URL", "http://localhost:9000", "User")
[Environment]::SetEnvironmentVariable("SONAR_PROJECT_KEY", "saascommerce", "User")
[Environment]::SetEnvironmentVariable("SONAR_TOKEN", "tu-token", "User")

# Solo para SonarCloud:
[Environment]::SetEnvironmentVariable("SONAR_ORGANIZATION", "tu-organizacion", "User")

.\scripts\sonar.ps1
```

URLs para crear token:

```txt
SonarQube local: http://localhost:9000/account/security
SonarCloud: https://sonarcloud.io/account/security
```

Credenciales iniciales de SonarQube local:

```txt
Usuario: admin
Password: admin
```

En el primer login SonarQube pedira cambiar la contrasena.

No guardar tokens en el repositorio. El script `scripts/sonar.ps1` lee secretos desde variables de entorno de usuario.
El script envia el token como `sonar.token` y `sonar.login` para mantener compatibilidad con instancias locales.

Servicios locales:

- Web: `http://localhost:5173`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- RabbitMQ Management: `http://localhost:15672`
- PostgreSQL: `localhost:5433` por defecto en host, `5432` dentro de Docker.

## Variables De Entorno

Usa `.env.example` como referencia para el ambiente local:

```txt
ConnectionStrings__DefaultConnection
RabbitMq__Host
RabbitMq__Username
RabbitMq__Password
Jwt__Secret
Jwt__Issuer
Jwt__Audience
VITE_API_BASE_URL
VITE_SIGNALR_HUB_URL
```

## Autenticacion Y Tenancy

Endpoints base:

```txt
POST /api/account/register-business
POST /api/auth/login
POST /api/auth/refresh
GET  /api/me
PUT  /api/me/profile
PUT  /api/me/password
GET  /api/business/current
PUT  /api/business/current
GET  /api/branches/current
PUT  /api/branches/current
```

Usuario seed para desarrollo:

```txt
Email: admin@test.com
Password: Admin123!
BusinessId: 11111111-1111-1111-1111-111111111111
BranchId: 22222222-2222-2222-2222-222222222222
```

Claims obligatorios del JWT:

```txt
sub
nameidentifier
business_id
branch_id
role
```

Regla de seguridad: `BusinessId`, `BranchId` y `UserId` salen del JWT o de `ICurrentUserService`. El frontend nunca es fuente confiable para esos valores, aunque los envie en un request.

## Account Onboarding

El flujo principal para empezar a usar el SaaS es registrar un comercio real:

```txt
POST /api/account/register-business
```

El registro crea:

```txt
Business
Branch principal
Usuario administrador
Rol Admin
Access token
Refresh token
```

El comercio puede tener identificacion flexible:

```txt
Cedula
RNC
Pasaporte
```

Y puede registrar varios telefonos desde el inicio. El seed de desarrollo se mantiene para pruebas locales, pero el flujo principal de la aplicacion no depende del seed.

### Hardening De Onboarding

- `identificationType` e `identificationNumber` son obligatorios.
- Tipos permitidos: `Cedula`, `Rnc`, `Passport`.
- Cedula se normaliza a digitos y debe tener 11 digitos.
- RNC se normaliza a digitos y debe tener 9 digitos.
- Pasaporte permite letras y numeros y requiere minimo 5 caracteres.
- Debe existir al menos un telefono.
- Debe existir exactamente un telefono principal.
- No se permiten telefonos vacios ni duplicados en el request.
- Backend valida estas reglas como fuente de verdad; frontend las replica con Zod para UX.

## Reglas Para Agentes IA

- Leer `ARCHITECTURE_RULES.md` antes de modificar codigo.
- Identificar el modulo correcto antes de crear clases, endpoints, consumers o componentes.
- No crear casos de uso en una Application global.
- Todo caso de uso debe vivir en `backend/src/Modules/{ModuleName}/Application`.
- No compartir entidades de dominio entre modulos.
- Compartir integraciones mediante `Contracts`, eventos o read models.
- No poner logica de negocio en API, Worker, controllers, consumers, hubs ni EF Core.
- Centralizar acceso HTTP en `frontend/src/shared/services/httpClient.ts`.
- Centralizar SignalR en `frontend/src/shared/services/signalrClient.ts`.
- Mantener pruebas de arquitectura actualizadas cuando cambien capas o referencias.

## Pruebas De Arquitectura

`backend/tests/SaasCommerce.Architecture.Tests` protege estas reglas:

- SharedKernel no depende de BuildingBlocks, Modules, EF Core, MassTransit ni ASP.NET Core.
- BuildingBlocks.Application no depende de Infrastructure ni frameworks tecnicos.
- Los dominios de modulos no dependen de dominios de otros modulos.
- Los contratos de modulos no dependen de Infrastructure.
- SharedKernel, BuildingBlocks y Modules no dependen de API ni Worker.
- No existe una `SaasCommerce.Application` global para casos de uso.

## Proteccion De Etapa 3

La base modular queda protegida con pruebas ejecutables:

- `Architecture.Tests` valida limites entre modulos, hosts sin dominio interno y ausencia de Application global.
- `Modules.Tests` valida eventos de integracion versionados con `EventId`, `CorrelationId`, `BusinessId`, `OccurredAt` y `Version`.
- `BuildingBlocks.Tests` valida Inbox, `EfInboxStore` e `IdempotentConsumer`.
- `Api.Tests` valida `CorrelationIdMiddleware` y el contrato JSON `isSuccess/data/error`.
- El frontend debe consumir `isSuccess`, `data` y `error`; no debe volver a `succeeded/errors`.

## Proteccion De Etapa 4

- `Tenancy` contiene `Business`, `BusinessPhone` y `Branch`.
- `Identity` contiene `User`, `Role`, `RefreshToken`, login, refresh token y consulta de usuario actual.
- `Api` expone `/api/account/register-business`, `/api/auth/login`, `/api/auth/refresh`, `/api/me`, `/api/me/profile`, `/api/me/password`, `/api/business/current` y `/api/branches/current` sin logica de negocio.
- `CurrentUserService` lee `UserId`, `BusinessId`, `BranchId` y roles desde claims.
- `GET /api/me` devuelve usuario, roles, negocio actual y sucursal actual sin exponer `PasswordHash` ni `RefreshTokens`.
- `PUT /api/me/profile` solo permite editar el perfil del usuario autenticado.
- `PUT /api/me/password` valida la contrasena actual y guarda la nueva contrasena hasheada.
- `GET/PUT /api/business/current` usan `BusinessId` del token; el update requiere rol `Admin`, identificacion valida y exactamente un telefono principal.
- `GET/PUT /api/branches/current` usan `BranchId` y `BusinessId` del token; no aceptan cambiar tenant desde frontend.
- El frontend tiene `RegisterBusinessPage`, `LoginPage`, `SettingsPage`, `ProfileSettingsCard`, `BusinessSettingsCard`, `BranchSettingsCard` y `SecuritySettingsCard`.
- Tests backend validan login, `/api/me`, endpoints de settings, cambio de contrasena invalido y validacion de telefono principal.
- Tests frontend validan registro, login y secciones/validaciones de `/settings`.

## Etapa 5: Catalog E Inventory Base

- `Catalog` contiene `Product`, `Category`, `ProductComponent`, contratos de requests/responses y eventos versionados `ProductCreatedIntegrationEventV1`, `ProductUpdatedIntegrationEventV1`, `ProductActivatedIntegrationEventV1` y `ProductDeactivatedIntegrationEventV1`.
- `Inventory` contiene `StockItem`, `InventoryMovement`, razones de movimiento, contratos de ajuste/stock/movimientos y eventos versionados `InventoryAdjustedIntegrationEventV1` y `StockReservedIntegrationEventV1`.
- `Product` soporta tipos `Simple`, `Service`, `Weighed`, `Composite`, `VariantParent` y `VariantChild`.
- `Product` administra datos comerciales reales: descripcion, SKU, barcode, unidad de medida, precio venta, costo, mayorista, precio minimo, impuestos, codigos internos/proveedor, variantes y atributos JSON.
- `Product` define su politica de inventario: `TrackInventory`, `MinimumStock`, `MaximumStock`, `ReorderPoint` y `AllowNegativeStock`.
- `Inventory` administra existencias y movimientos: cantidad anterior, cantidad nueva, cantidad del ajuste, razon, usuario y tenant.
- `Inventory` separa stock y movimientos por `BusinessId` y `BranchId`; un producto puede tener stock independiente por sucursal.
- `Inventory` consulta la politica publica de inventario del producto antes de ajustar stock; servicios y productos sin `TrackInventory` no aceptan ajustes.
- `Inventory` no referencia infraestructura de `Catalog`; usa contratos publicos/read models como frontera.
- La API expone endpoints autenticados:

```txt
POST /api/catalog/products
PUT  /api/catalog/products/{id}
PUT  /api/catalog/products/{id}/activate
PUT  /api/catalog/products/{id}/deactivate
GET  /api/catalog/products/{id}
GET  /api/catalog/products
POST /api/catalog/categories
PUT  /api/catalog/categories/{id}
GET  /api/catalog/categories
POST /api/inventory/adjustments
GET  /api/inventory/stock
GET  /api/inventory/movements
```

- El frontend tiene pantallas funcionales para `Productos` e `Inventario`, usando TanStack Query, servicios centralizados, formularios con React Hook Form + Zod, estados loading/error/empty y contrato `isSuccess/data/error`.
- La UI de producto esta organizada por secciones: datos generales, precio/costos, inventario, codigos, impuestos/opciones y opciones avanzadas.
- Tests backend validan creacion de productos, SKU duplicado por tenant, barcode duplicado, producto servicio sin inventario, producto pesado sin unidad `Unit`, ajuste de inventario, movimiento generado, filtros de stock/movimientos y proteccion contra stock negativo.

## Etapa 5.2: UX De Productos Y Paginacion

- `/products` prioriza busqueda, filtros, listado y paginacion; no muestra formularios grandes en el flujo principal.
- Crear y editar productos se hace en un drawer lateral reutilizando el mismo formulario por secciones.
- El listado soporta filtros por `query`, `productType`, `isActive`, `categoryId`, `page` y `pageSize`.
- `GET /api/catalog/products` soporta `sortBy` y `sortDirection` con ordenamiento seguro.
- `GET /api/catalog/products` devuelve `items`, `page`, `pageSize`, `totalItems`, `totalPages`, `hasPreviousPage` y `hasNextPage`.
- `pageSize` permitido: 10, 25 o 50.
- El filtro de categoria consume `GET /api/catalog/categories`; no se escribe el `categoryId` manualmente en la UI.
- La tabla incluye producto, tipo, SKU/barcode, unidad, precio, politica de stock, estado y acciones.
- La UI incluye skeleton de tabla, empty state, error state con reintento, paginacion anterior/siguiente y selector de 10, 25 o 50 registros.
- Los productos pueden activarse y desactivarse sin borrado fisico; la UI pide confirmacion antes de desactivar.
- `/inventory` prioriza stock y movimientos, con ajuste de inventario en drawer lateral.
- `GET /api/inventory/stock` soporta `search`, `lowStockOnly`, `productType`, `categoryId`, `page`, `pageSize`, `sortBy` y `sortDirection`.
- `GET /api/inventory/movements` soporta `productId`, `movementType`, `dateFrom`, `dateTo`, `page`, `pageSize`, `sortBy` y `sortDirection`.
- Las respuestas paginadas de inventario devuelven `items`, `page`, `pageSize`, `totalItems`, `totalPages`, `hasPreviousPage` y `hasNextPage`.
- Tests frontend cubren render de stock, paginacion de inventario y validacion del drawer de ajuste.

## Etapa 5.3: POS Readiness

- `Simple`, `Service` y `Weighed` quedan soportados para el POS inicial.
- `Composite`, `VariantParent` y `VariantChild` quedan preparados en el modelo, pero no funcionales completos en el MVP inicial.
- Los combos con descuento automatico de componentes, matrices de variantes y atributos dinamicos avanzados quedan para una etapa posterior.
- Producto inactivo no es vendible, conserva historial y puede reactivarse.
- Servicio activo puede venderse aunque no controle inventario.
- Producto no inventariable activo puede venderse sin stock.
- Producto inventariable activo requiere validacion de stock en `Inventory`.
- Producto pesado permite cantidades decimales.
- Producto simple usa cantidades enteras por defecto; configuracion decimal avanzada queda para una etapa posterior.
- `Sales` no debe consultar entidades internas ni `DbContext` de `Catalog`.
- `Sales` debe consultar datos de venta mediante `IProductSalesPolicyReader`.
- `Sales` no debe descontar inventario directamente.
- `Sales` debe validar stock mediante `IInventoryAvailabilityService` y dejar la deduccion final preparada para flujo controlado/eventos.
- `Inventory` sigue siendo dueno del stock y valida `BusinessId` y `BranchId`.
- `InventoryMovementsPage` avanzada queda para etapa posterior; el minimo actual aceptado es `GET /api/inventory/movements` con filtros y paginacion.

## Definition Of Done

- `dotnet build --no-restore` pasa con 0 warnings.
- `dotnet test --no-build` pasa.
- `npm run build` pasa en `frontend`.
- `npm run test` pasa en `frontend` cuando hay pruebas de UI.
- No se introducen issues nuevos de Sonar en codigo nuevo.
- Las dependencias respetan Clean Architecture.
- API y Worker siguen separados.
- Cada funcionalidad nueva declara su modulo dueño.
- Cualquier acceso a datos futuro considera `BusinessId`.
- Eventos criticos futuros deben pasar por Outbox.
- Consumers criticos deben usar Inbox/idempotencia.
- Cobertura objetivo para codigo nuevo: 80% o mas.

## Etapa 6: Guia Operativa De Estabilizacion

Esta guia permite levantar, probar y validar el sistema antes de avanzar a `Sales/POS`.

### Levantar Todo Con Docker

Desde la raiz del repositorio:

```bash
docker compose down
docker compose up --build
```

Para dejarlo corriendo en segundo plano:

```bash
docker compose up --build -d
docker compose ps
```

Servicios locales esperados:

```txt
Web: http://localhost:5173
Login: http://localhost:5173/login
Registro de comercio: http://localhost:5173/register-business
API: http://localhost:8080
Swagger: http://localhost:8080/swagger
RabbitMQ Management: http://localhost:15672
PostgreSQL host: localhost:5433
```

`docker-compose.yml` no requiere secretos hardcodeados para desarrollo. Si `JWT_SECRET` no se define en `.env`, la API genera un secreto efimero solo en `Development`. Para sesiones persistentes entre reinicios, define `JWT_SECRET` en `.env` usando `.env.example` como base.

### Registro, Login Y Settings

Flujo manual minimo:

```txt
1. Abrir http://localhost:5173/register-business.
2. Registrar un comercio con RNC, cedula o pasaporte valido y exactamente un telefono principal.
3. Confirmar entrada al shell autenticado.
4. Salir.
5. Abrir http://localhost:5173/login.
6. Iniciar sesion con el usuario registrado.
7. Abrir /settings y validar Mi perfil, Negocio, Sucursal y Seguridad.
```

Validaciones esperadas:

```txt
GET /api/me devuelve el usuario actual.
BusinessId, BranchId y UserId salen del JWT.
El frontend no envia BusinessId ni BranchId como fuente de verdad.
Logout limpia la sesion local.
Una sesion expirada redirige a /login antes de consultar endpoints protegidos.
```

### Swagger JWT

Para probar endpoints protegidos desde Swagger:

```txt
1. Abrir http://localhost:8080/swagger.
2. Ejecutar POST /api/auth/login.
3. Copiar data.accessToken.
4. Pulsar Authorize.
5. Pegar solo el JWT. Swagger agrega Bearer automaticamente.
6. Ejecutar endpoints protegidos.
```

Endpoints minimos para validar:

```txt
GET /api/me
GET /api/business/current
GET /api/branches/current
GET /api/catalog/products
GET /api/inventory/stock
GET /api/inventory/movements
```

Sin token deben responder `401` con `isSuccess=false`, `data=null` y `error.code=UNAUTHORIZED`.

### Products

Endpoints principales:

```txt
POST /api/catalog/categories
GET  /api/catalog/categories
POST /api/catalog/products
PUT  /api/catalog/products/{id}
PUT  /api/catalog/products/{id}/activate
PUT  /api/catalog/products/{id}/deactivate
GET  /api/catalog/products/{id}
GET  /api/catalog/products
```

Validacion funcional:

```txt
/products muestra filtros, listado, empty state y paginacion.
Crear y editar producto se abre en drawer.
Categorias se cargan desde backend.
SKU duplicado devuelve PRODUCT_SKU_ALREADY_EXISTS.
Barcode duplicado devuelve PRODUCT_BARCODE_ALREADY_EXISTS.
Producto inexistente devuelve PRODUCT_NOT_FOUND.
```

### Inventory

Endpoints principales:

```txt
POST /api/inventory/adjustments
GET  /api/inventory/stock
GET  /api/inventory/movements
```

Validacion funcional:

```txt
Un ajuste valido crea o actualiza StockItem.
Un ajuste valido genera InventoryMovement.
PreviousStock y NewStock quedan correctos.
Stock y movimientos filtran por BusinessId y BranchId.
Ajuste que deja stock negativo devuelve INVENTORY_STOCK_INSUFFICIENT.
Ajuste sobre servicio o producto no inventariable devuelve INVENTORY_PRODUCT_NOT_TRACKED.
```

### Health Checks

Endpoints:

```txt
GET /health/live
GET /health/ready
```

Uso:

```bash
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready
```

`/health/live` valida que el proceso API esta vivo. `/health/ready` valida PostgreSQL y RabbitMQ cuando se usa broker real. Si una dependencia no esta lista, responde `503` con `error.code=SERVICE_UNAVAILABLE`.

### Regresion Backend

Desde la raiz del repositorio:

```bash
dotnet restore SaasCommerce.slnx
dotnet build SaasCommerce.slnx --no-restore -m:1 /nr:false -v minimal
dotnet test SaasCommerce.slnx --no-build -m:1 /nr:false -v minimal
```

Criterios:

```txt
Build OK.
0 warnings.
Api.Tests OK.
Architecture.Tests OK.
Modules.Tests OK.
BuildingBlocks.Tests OK.
SharedKernel.Tests OK.
Worker.Tests OK.
```

### Regresion Frontend

Desde `frontend`:

```bash
npm.cmd run lint
npm.cmd run test
npm.cmd run build
```

O desde la raiz:

```bash
npm.cmd --prefix frontend run lint
npm.cmd --prefix frontend run test
npm.cmd --prefix frontend run build
```

Criterios:

```txt
Lint OK.
Tests OK.
TypeScript OK.
Build OK.
Sin warning de chunks.
Sin errores de consola en /settings, /products e /inventory.
```

### SonarQube Y Quality Gate

Levantar SonarQube local:

```powershell
docker compose -f docker-compose.sonar.yml up -d
```

Configurar variables de usuario:

```powershell
[Environment]::SetEnvironmentVariable("SONAR_HOST_URL", "http://localhost:9000", "User")
[Environment]::SetEnvironmentVariable("SONAR_PROJECT_KEY", "saascommerce", "User")
[Environment]::SetEnvironmentVariable("SONAR_TOKEN", "tu-token", "User")
```

Ejecutar analisis:

```powershell
.\scripts\sonar.ps1
```

Criterios:

```txt
CE task SUCCESS.
Quality Gate OK.
Open issues = 0.
New coverage >= 80%.
New duplication <= 3%.
Security Hotspots revisados al 100%.
```

### Troubleshooting

Problemas comunes:

```txt
401 en endpoints protegidos:
  Inicia sesion de nuevo o limpia localStorage si cambiaste JWT_SECRET.

Swagger no autoriza:
  Pega solo el JWT en Authorize. No escribas Bearer manualmente.

/health/ready devuelve 503:
  Revisa docker compose ps y logs de postgres/rabbitmq/api.

Worker no conecta al broker:
  Confirma que rabbitmq aparece healthy. El healthcheck valida ping y puerto AMQP.

Registro de comercio devuelve VALIDATION_ERROR:
  Revisa identificacion, telefono principal unico y formato de email/password.

Docker muestra contenedores huerfanos de SonarQube:
  Es solo un aviso si Sonar se levanto con docker-compose.sonar.yml.
```
