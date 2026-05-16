# Reglas de Arquitectura — SaasCommerce RD

> Todo desarrollo debe seguir estas reglas antes de crear, modificar o revisar código en SaasCommerce RD.

---

## 1. Contexto del proyecto

SaasCommerce RD es un SaaS para negocios dominicanos como colmados, minimarkets, supermercados pequeños, importadoras y distribuidoras.

El sistema debe permitir vender, controlar inventario, manejar clientes y fiados, registrar compras, generar recibos/facturas básicas, mostrar reportes y actualizar la interfaz en tiempo real.

La arquitectura oficial es:

```txt
Clean Architecture
+ Principios SOLID
+ Event-Driven Architecture
+ Saga State Machine
+ Outbox Pattern
+ SignalR
+ API y Worker separados
+ React + Vite como frontend
+ .NET como backend
+ PostgreSQL como base de datos
+ RabbitMQ o Azure Service Bus como broker
```

---

## 2. Regla principal para ti

Debes respetar esta regla en todo cambio:

> No debes romper Clean Architecture, SOLID, separación por capas, multi-tenancy ni el flujo orientado a eventos.

Antes de crear o modificar código, debes identificar en qué capa pertenece la funcionalidad.

---

## 3. Regla obligatoria de calidad SonarQube / SonarLint

Antes de crear o modificar código, debes asumir que el cambio será analizado por SonarQube, SonarCloud o SonarLint.

No debes introducir:

```txt
Issues nuevos de Sonar
Vulnerabilidades
Bugs
Code smells críticos
Duplicación innecesaria
Complejidad injustificada
Reducción injustificada de cobertura
Secretos hardcodeados
Código muerto
Warnings relevantes
```

Debes seguir este enfoque:

```txt
Clean as You Code
+ Sonar Way Quality Gate
+ Cero issues nuevos en código nuevo
+ Cobertura mínima del 80% en código nuevo
+ Duplicación máxima del 3% en código nuevo
+ Security Hotspots revisados al 100%
```

### Condiciones mínimas del Quality Gate

```txt
New Issues = 0
Security Hotspots Reviewed = 100%
Coverage on New Code >= 80%
Duplicated Lines on New Code <= 3%
Reliability Rating on New Code = A
Security Rating on New Code = A
Maintainability Rating on New Code = A
```

### Reglas generales de calidad

Debes:

```txt
Escribir código simple, legible y mantenible.
Evitar duplicación de lógica.
Evitar métodos largos.
Evitar clases con múltiples responsabilidades.
Evitar complejidad cognitiva alta.
Evitar anidamientos profundos.
Evitar código muerto.
Evitar variables, parámetros o imports sin uso.
Evitar magic strings y magic numbers.
Evitar capturas genéricas de excepciones sin manejo adecuado.
Evitar silenciamiento de errores.
Evitar comentarios que sustituyen código mal nombrado.
Evitar TODO/FIXME sin justificación.
Evitar credenciales, secretos o tokens hardcodeados.
Evitar exposición de datos sensibles en logs.
Evitar null handling inseguro.
Evitar async void en C#, salvo eventos UI específicos.
Evitar bloquear async con .Result o .Wait().
Usar CancellationToken en métodos async del backend.
Usar validaciones explícitas con FluentValidation o Zod según la capa.
Agregar pruebas para mantener cobertura del código nuevo.
```

### Reglas para C# / .NET

Debes:

```txt
Activar nullable reference types.
Tratar warnings importantes como errores en CI.
Preferir inyección de dependencias por constructor.
Usar excepciones de dominio o aplicación cuando aplique.
Usar IClock o DateTime.UtcNow según convención.
Usar SQL parametrizado si necesitas consultas manuales.
Usar Result, respuesta explícita o excepción controlada cuando evitar null haga el código más seguro.
```

No debes:

```txt
Crear servicios estáticos con estado mutable.
Usar Service Locator.
Capturar Exception sin agregar contexto, log o transformación útil.
Lanzar excepciones genéricas cuando exista una excepción específica.
Mezclar lógica de negocio con EF Core, controllers, consumers o hubs.
Construir SQL manual sin parámetros.
Loggear passwords, tokens, refresh tokens, RNC sensible o datos de pago.
```

### Reglas para React / TypeScript

Debes:

```txt
Usar TypeScript estricto.
Usar TanStack Query para server state.
Usar Zustand solo para estado cliente/local.
Usar React Hook Form y Zod para formularios.
Extraer hooks reutilizables.
Manejar estados loading, error y empty.
Centralizar el cliente HTTP.
Centralizar el cliente SignalR.
```

No debes:

```txt
Usar any salvo justificación explícita.
Duplicar tipos de API innecesariamente.
Hacer fetch directo desde componentes complejos.
Mezclar lógica de negocio con componentes visuales.
Crear componentes gigantes.
Usar props ambiguas.
Exponer datos de un BusinessId en pantallas de otro tenant.
```

---

## 4. Stack oficial

La UI debe seguir tambien las reglas visuales del archivo `UI_RULES.md`. Toda modificacion de frontend debe respetar Tailwind Stone, estilo SaaS administrativo sobrio, componentes reutilizables, estados visibles y accesibilidad.

### Frontend

```txt
React
Vite
TypeScript
TailwindCSS
ShadCN UI
TanStack Query
Zustand
React Hook Form
Zod
@microsoft/signalr
```

### Backend

```txt
.NET 8 o superior
ASP.NET Core Web API
.NET Worker Service
Entity Framework Core
PostgreSQL
MassTransit
RabbitMQ para desarrollo local
Azure Service Bus para producción futura
SignalR
FluentValidation
Serilog
JWT Authentication
```

### Testing

```txt
xUnit
FluentAssertions
NSubstitute o Moq
Testcontainers
WebApplicationFactory
MassTransit Test Harness
Respawn
Bogus
NetArchTest
Vitest
React Testing Library
MSW
Playwright
```

---

## 5. Estructura oficial del proyecto

Debes mantener esta estructura:

```txt
<repo>/
|-- backend/
|   |-- src/
|   |   |-- Api/
|   |   |-- Worker/
|   |   |-- SharedKernel/
|   |   |-- BuildingBlocks/
|   |   `-- Modules/
|   |       |-- Tenancy/
|   |       |-- Identity/
|   |       |-- Catalog/
|   |       |-- Inventory/
|   |       |-- Sales/
|   |       |-- Customers/
|   |       |-- Billing/
|   |       |-- Payments/
|   |       `-- Reporting/
|   |
|   `-- tests/
|       |-- SaasCommerce.Api.Tests/
|       |-- SaasCommerce.Architecture.Tests/
|       |-- SaasCommerce.BuildingBlocks.Tests/
|       |-- SaasCommerce.Modules.Tests/
|       |-- SaasCommerce.SharedKernel.Tests/
|       `-- SaasCommerce.Worker.Tests/
|
|-- frontend/
|-- docker-compose.yml
|-- README.md
`-- ARCHITECTURE_RULES.md
```

Regla modular obligatoria:

```txt
No debes crear comandos, queries, handlers, entidades ni servicios de negocio en una Application global.
Todo caso de uso debe vivir dentro de backend/src/Modules/{ModuleName}/Application.
Cada modulo debe tener Domain, Application, Contracts e Infrastructure.
SharedKernel debe mantenerse pequeno.
BuildingBlocks solo contiene herramientas y abstracciones compartidas.
```

Modulos iniciales:

```txt
Tenancy     -> negocios, sucursales, BusinessId y configuracion del tenant.
Identity    -> usuarios, roles, permisos, autenticacion y refresh tokens.
Catalog     -> productos, categorias, marcas y precios base.
Inventory   -> stock, movimientos, ajustes, entradas y salidas.
Sales       -> POS, ventas, items, estados, cancelaciones y devoluciones.
Customers   -> clientes, fiados, cuentas por cobrar y abonos.
Billing     -> recibos, facturas, comprobantes y secuencias fiscales futuras.
Payments    -> metodos de pago, pagos de venta, caja y validacion de pagos.
Reporting   -> dashboard, metricas, reportes y modelos de lectura.
```

Reglas para microservicios futuros:

```txt
El MVP es un monolito modular, no microservicios reales.
Cada modulo debe tratar sus datos como propios.
No compartas entidades de dominio entre modulos.
Si un modulo necesita reaccionar a otro, usa eventos.
Si un modulo necesita leer datos de otro, usa query publica, contrato o read model.
Sales no debe consultar entidades internas ni DbContext de Catalog.
Sales no debe descontar inventario directamente ni consultar DbContext de Inventory.
Sales debe consultar productos vendibles mediante contratos publicos de Catalog, como IProductSalesPolicyReader.
Sales debe validar disponibilidad mediante contratos publicos de Inventory, como IInventoryAvailabilityService.
Inventory conserva la propiedad del stock y toda deduccion debe pasar por flujo controlado o eventos.
Los eventos de integracion deben vivir en Contracts del modulo dueño.
Los eventos de integracion deben incluir EventId, CorrelationId, BusinessId, OccurredAt y Version.
Los consumers criticos deben ser idempotentes usando Inbox.
```

Proteccion obligatoria de arquitectura modular:

```txt
Architecture.Tests debe validar limites entre modulos.
Ningun Domain de un modulo debe referenciar Domain de otro modulo.
Ningun modulo debe acceder a Infrastructure de otro modulo.
API y Worker son hosts y no deben referenciar Domain interno de modulos.
Worker no debe contener reglas de negocio; debe delegar a modulos o BuildingBlocks.
Todo consumer que procese eventos del broker debe usar una base idempotente cuando sea critico.
Contract tests deben validar eventos versionados.
ApiResponse<T> debe serializar como isSuccess, data y error.
CorrelationIdMiddleware debe devolver X-Correlation-Id en cada response.
El frontend debe consumir isSuccess/data/error.
```

---

## 6. Responsabilidades por capa

## 6.1 Domain

Usa Domain para el negocio puro.

Aquí debes colocar:

```txt
Entidades
Value Objects
Enums del dominio
Reglas de negocio
Domain Events
Excepciones de dominio
Interfaces puras si son estrictamente del dominio
```

No debes hacer que Domain dependa de:

```txt
Entity Framework Core
MassTransit
RabbitMQ
Azure Service Bus
SignalR
ASP.NET Core
PostgreSQL
JWT
Servicios externos
```

Ejemplos válidos en Domain:

```txt
Sale
SaleItem
Product
InventoryMovement
Customer
CustomerCredit
Invoice
Purchase
Supplier
DomainException
SaleCreatedDomainEvent
```

Regla obligatoria:

> No llames base de datos, no publiques mensajes, no envíes SignalR y no uses infraestructura desde Domain.

---

## 6.2 Application

Usa Application para casos de uso y orquestación.

Aquí debes colocar:

```txt
Commands
Queries
Handlers
Use Cases
DTOs internos
Validaciones de aplicación
Interfaces de abstracción
Orquestación de procesos
```

Application puede depender de:

```txt
Domain
Contracts
Abstractions
```

No debes hacer que Application dependa directamente de:

```txt
EF Core
MassTransit
SignalR
RabbitMQ
Azure Service Bus
Controllers
DbContext concreto
JWT concreto
```

Ejemplos válidos en Application:

```txt
CreateSaleCommand
CreateSaleHandler
RegisterPaymentCommand
AdjustInventoryCommand
GenerateInvoiceCommand
ISaleRepository
IUnitOfWork
IEventBus
IRealtimeNotifier
ICurrentUserService
```

Regla obligatoria:

> Orquesta casos de uso en Application, pero implementa los detalles técnicos en Infrastructure.

---

## 6.3 Contracts

Usa Contracts para contratos compartidos entre API, Worker y mensajería.

Aquí debes colocar:

```txt
Integration Events
Integration Commands
Notification Contracts
Request/Response Contracts compartidos
Constantes públicas de contratos
```

Ejemplos válidos:

```txt
SaleCreatedEvent
InventoryDeductedEvent
PaymentRegisteredEvent
InvoiceGeneratedEvent
SaleCompletedEvent
SaleFailedEvent
SaleStatusChangedNotification
```

Regla obligatoria:

> Coloca en Contracts todos los eventos que viajan por RabbitMQ o Azure Service Bus.

---

## 6.4 Infrastructure

Usa Infrastructure para detalles técnicos.

Aquí debes colocar:

```txt
EF Core
AppDbContext
Repositories
UnitOfWork
MassTransit
Consumers
Sagas
Outbox
SignalR
JWT
Servicios externos
Implementaciones de interfaces
Configuraciones técnicas
```

Ejemplos válidos:

```txt
AppDbContext
EfSaleRepository
EfUnitOfWork
MassTransitEventBus
SaleStateMachine
SaleSagaState
SignalRRealtimeNotifier
JwtTokenService
OutboxMessage
```

Regla obligatoria:

> Implementa infraestructura aquí, pero no concentres reglas de negocio en esta capa.

---

## 6.5 API

Usa API solo para exponer endpoints HTTP.

Aquí debes colocar:

```txt
Endpoints
Controllers si se usan
Middlewares
Swagger
Configuración HTTP
Autenticación HTTP
CORS
Program.cs
```

Regla obligatoria:

> No coloques lógica de negocio en API. Recibe requests, llama casos de uso y devuelve respuestas.

Ejemplo correcto:

```txt
POST /api/sales
  → validas request básico
  → obtienes usuario actual
  → creas command
  → llamas CreateSaleHandler
  → devuelves ApiResponse
```

---

## 6.6 Worker

Usa Worker para procesar eventos, consumers, sagas y tareas en segundo plano.

Aquí debes colocar:

```txt
Configuración de MassTransit
Registro de consumers
Registro de sagas
Hosted services
Procesamiento de Outbox si aplica
```

Regla obligatoria:

> No coloques lógica de negocio extensa en Worker. Consume eventos y delega a Application.

---

## 7. Reglas SOLID obligatorias

## 7.1 Single Responsibility Principle

Debes crear clases con una sola responsabilidad.

No debes crear clases que hagan demasiadas cosas.

Incorrecto:

```txt
CreateSaleHandler:
- Crear venta
- Validar inventario
- Descontar inventario
- Procesar pago
- Generar factura
- Enviar SignalR
- Enviar WhatsApp
```

Correcto:

```txt
CreateSaleHandler
ValidateSaleStockConsumer
DeductInventoryConsumer
RegisterPaymentConsumer
GenerateInvoiceConsumer
SaleNotificationConsumer
```

---

## 7.2 Open/Closed Principle

Debes permitir agregar comportamientos sin modificar código existente innecesariamente.

Para métodos de pago, usa estrategia:

```csharp
public interface IPaymentProcessor
{
    PaymentMethod Method { get; }
    Task<PaymentResult> ProcessAsync(ProcessPaymentCommand command, CancellationToken ct);
}
```

Implementaciones posibles:

```txt
CashPaymentProcessor
TransferPaymentProcessor
CreditPaymentProcessor
CardPaymentProcessor
```

---

## 7.3 Liskov Substitution Principle

Debes diseñar implementaciones sustituibles sin romper casos de uso.

Ejemplo:

```txt
IEventBus
```

Puede tener implementaciones como:

```txt
MassTransitEventBus
InMemoryEventBus
AzureServiceBusEventBus
```

El código de Application no debe cambiar.

---

## 7.4 Interface Segregation Principle

Debes crear interfaces pequeñas y específicas.

Incorrecto:

```csharp
public interface IBusinessService
{
    Task CreateSale();
    Task CreateProduct();
    Task SendEmail();
    Task GenerateInvoice();
    Task DeductInventory();
}
```

Correcto:

```txt
ISaleRepository
IProductRepository
IInventoryRepository
IInvoiceService
IRealtimeNotifier
IEventBus
IUnitOfWork
```

---

## 7.5 Dependency Inversion Principle

Debes depender de abstracciones, no de detalles.

Application debe depender de:

```txt
IEventBus
IUnitOfWork
ISaleRepository
IInventoryRepository
IRealtimeNotifier
ICurrentUserService
IClock
```

Infrastructure debe implementar esas abstracciones.

---

## 8. Reglas para arquitectura orientada a eventos

## 8.1 Domain Events

Usa Domain Events para representar hechos internos del dominio.

Ejemplos:

```txt
SaleCreatedDomainEvent
InventoryAdjustedDomainEvent
CustomerCreditCreatedDomainEvent
```

Reglas:

```txt
Debes colocarlos en Domain.
No deben depender de MassTransit.
No deben conocer RabbitMQ.
No debes publicarlos directamente desde las entidades.
```

---

## 8.2 Integration Events

Usa Integration Events para mensajes que viajan por el broker.

Ejemplos:

```txt
SaleCreatedEvent
InventoryDeductedEvent
PaymentRegisteredEvent
InvoiceGeneratedEvent
SaleCompletedEvent
SaleFailedEvent
```

Reglas:

```txt
Debes colocarlos en Contracts.
Debes hacerlos inmutables, preferiblemente record.
Debes incluir CorrelationId cuando aplique.
Debes incluir BusinessId en eventos de negocio.
Debes incluir CreatedAt.
```

Ejemplo:

```csharp
public record SaleCreatedEvent(
    Guid CorrelationId,
    Guid SaleId,
    Guid BusinessId,
    Guid BranchId,
    Guid UserId,
    decimal Total,
    string PaymentMethod,
    DateTime CreatedAt
);
```

---

## 8.3 Publicación de eventos

No debes usar MassTransit directamente desde Application.

Usa esta abstracción:

```csharp
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class;
}
```

Implementa esa interfaz en Infrastructure con MassTransit.

---

## 8.4 Consumers

No debes colocar lógica de negocio extensa en consumers.

Correcto:

```csharp
public sealed class SaleCreatedConsumer : IConsumer<SaleCreatedEvent>
{
    private readonly IStartSaleProcessingUseCase _useCase;

    public SaleCreatedConsumer(IStartSaleProcessingUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task Consume(ConsumeContext<SaleCreatedEvent> context)
    {
        await _useCase.HandleAsync(context.Message, context.CancellationToken);
    }
}
```

Regla:

> Usa el consumer como adaptador de MassTransit hacia Application.

---

## 8.5 Saga State Machine

Usa sagas para procesos largos con varios estados.

Usa sagas para:

```txt
Procesamiento de venta
Facturación electrónica futura
Transferencia entre sucursales futura
Compras complejas futuras
```

No uses sagas para:

```txt
Crear producto
Editar producto
Crear cliente
Login
Crear categoría
Cambiar configuración simple
```

Estados recomendados para venta:

```txt
Received
Processing
StockValidated
InventoryDeducted
PaymentRegistered
InvoiceGenerated
Completed
Failed
Cancelled
```

Regla:

> Usa la saga para coordinar el proceso, pero no pongas toda la lógica de negocio dentro de la saga.

---

## 9. Outbox Pattern

Debes usar Outbox Pattern en todo flujo crítico que publique eventos.

Problema que debes evitar:

```txt
Se guarda la venta en la base de datos, pero falla la publicación del evento.
```

Flujo correcto:

```txt
Guardas entidad
Guardas OutboxMessage en la misma transacción
Confirmas transacción
Publicas mensaje desde OutboxPublisher
Marcas mensaje como publicado
```

Reglas:

```txt
No publiques eventos críticos antes de confirmar base de datos.
No dependas solo de publish directo para procesos importantes.
Haz que los mensajes sean idempotentes.
Haz que los consumers toleren duplicados.
```

---

## 10. SignalR

Usa SignalR para notificaciones en tiempo real hacia React.

No debes usar SignalR directamente desde Application.

Usa esta abstracción:

```csharp
public interface IRealtimeNotifier
{
    Task NotifyBusinessAsync(Guid businessId, string eventName, object payload, CancellationToken ct);
    Task NotifyUserAsync(Guid userId, string eventName, object payload, CancellationToken ct);
}
```

Implementa la abstracción en Infrastructure con SignalR.

Grupos obligatorios:

```txt
business-{businessId}
branch-{branchId}
user-{userId}
```

Regla:

> Nunca envíes notificaciones globales con datos de un negocio. Siempre usa grupos por BusinessId, BranchId o UserId.

---

## 11. Multi-tenancy

Debes tratar el sistema como multi-tenant desde el inicio.

Toda entidad de negocio debe incluir:

```txt
BusinessId
```

Ejemplos:

```txt
Product
Sale
Customer
Purchase
Invoice
InventoryMovement
Payment
```

Reglas obligatorias:

```txt
Filtra toda consulta por BusinessId.
Nunca devuelvas datos de otro negocio.
Incluye BusinessId en todo evento de negocio.
Envía toda notificación en tiempo real al grupo correcto del negocio.
No crees endpoints que consulten datos sin contexto del tenant.
No confíes en BusinessId enviado desde frontend; obtén el BusinessId desde el usuario autenticado o un contexto seguro.
```

---

## 12. Flujo oficial de venta

Debes implementar la venta como flujo orientado a eventos:

```txt
React POS
   ↓
POST /api/sales
   ↓
CreateSaleUseCase
   ↓
Guardar Sale como Received
   ↓
Guardar/Publicar SaleCreatedEvent mediante Outbox
   ↓
SaleStateMachine inicia proceso
   ↓
ValidateStockConsumer
   ↓
InventoryDeductedEvent
   ↓
RegisterPaymentConsumer
   ↓
PaymentRegisteredEvent
   ↓
GenerateInvoiceConsumer
   ↓
InvoiceGeneratedEvent
   ↓
SaleCompletedEvent o SaleFailedEvent
   ↓
SignalR notifica a React
```

Estados mínimos de venta:

```txt
Received
Processing
Completed
Failed
Cancelled
```

---

## 13. Reglas para frontend React

Usa esta estructura:

```txt
frontend/src/
├── app/
│   ├── router.tsx
│   └── providers.tsx
│
├── modules/
│   ├── auth/
│   ├── dashboard/
│   ├── products/
│   ├── inventory/
│   ├── sales/
│   ├── customers/
│   ├── purchases/
│   ├── invoices/
│   └── reports/
│
├── shared/
│   ├── components/
│   ├── hooks/
│   ├── services/
│   ├── types/
│   └── utils/
│
└── main.tsx
```

Reglas:

```txt
No llames fetch directamente en componentes grandes.
Usa servicios o hooks.
Usa TanStack Query para server state.
Usa Zustand solo para estado cliente/local.
Usa Zod para validar formularios.
Usa React Hook Form para formularios.
Usa SignalR client centralizado.
No dupliques tipos si pueden generarse o compartirse.
```

---

## 14. Respuestas estándar del API

Debes usar esta respuesta estándar:

Regla obligatoria de Result:

```txt
Todas las respuestas del backend deben seguir el patron Result.
El contrato externo debe mantener siempre estas propiedades: isSuccess, data y error.
No devuelvas DTOs crudos, strings, booleanos, listas, null ni excepciones como respuesta HTTP directa.
No uses nombres alternos como success, succeeded, isSuccessful, errors o message suelto.
Para exito: isSuccess = true, data contiene el resultado y error = null.
Para error: isSuccess = false, data = null y error contiene code, message y details cuando aplique.
```

```json
{
  "isSuccess": true,
  "data": {},
  "error": null
}
```

Error:

```json
{
  "isSuccess": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Hay errores de validación.",
    "details": []
  }
}
```

Reglas:

```txt
No expongas stack trace al cliente.
Usa códigos de error consistentes.
Registra errores con Serilog.
Incluye CorrelationId en logs.
```

---

## 15. Seguridad

Debes:

```txt
Usar JWT para autenticación.
Usar refresh tokens.
Validar permisos por rol cuando aplique.
Obtener BusinessId desde el usuario autenticado o contexto seguro.
Obtener UserId desde el JWT o ICurrentUserService.
Obtener BranchId desde el JWT o ICurrentUserService.
El JWT debe incluir UserId, BusinessId, BranchId cuando aplique y roles.
Los handlers deben depender de ICurrentUserService para contexto del usuario actual.
Hashear passwords de forma segura.
Validar entradas con FluentValidation.
Aplicar CORS solo a orígenes permitidos.
```

No debes:

```txt
Confiar en BusinessId enviado desde frontend.
Confiar en BranchId enviado desde frontend como fuente de autorizacion.
Confiar en UserId enviado desde frontend como fuente de autorizacion.
Guardar passwords en texto plano.
Loggear tokens, refresh tokens o información sensible.
Exponer datos de un tenant a otro tenant.
```

---

## 16. Base de datos

Debes:

```txt
Usar PostgreSQL.
Usar EF Core.
Usar migraciones.
Usar configuraciones por entidad.
Registrar CreatedAt y UpdatedAt.
Usar soft delete cuando aplique.
Usar índices para BusinessId, fechas y códigos de búsqueda.
Mantener lógica compleja fuera de controllers y DbContext.
```

Entidades base deben incluir:

```txt
Id
CreatedAt
UpdatedAt
CreatedBy
UpdatedBy
```

Entidades multi-tenant deben incluir:

```txt
BusinessId
```

---

## 17. Logging

Usa Serilog.

Incluye en logs cuando aplique:

```txt
CorrelationId
BusinessId
UserId
SaleId
EventId
EventName
ConsumerName
```

Regla:

> Todo consumer debe loggear inicio, éxito y fallo del procesamiento con CorrelationId.

---

## 18. Idempotencia

Debes hacer que los consumers toleren mensajes duplicados.

Reglas:

```txt
No asumas que un evento llega solo una vez.
Verifica el estado actual antes de modificar entidades.
Usa InboxMessages o registro equivalente para eventos críticos.
No dupliques ventas, pagos, facturas ni movimientos de inventario.
```

Ejemplo:

```txt
Si InventoryDeductedEvent ya fue procesado para SaleId, no descuentas inventario otra vez.
```

---

## 19. Convenciones de nombres

Usa nombres explícitos.

Ejemplos recomendados:

```txt
CreateSaleCommand
CreateSaleHandler
SaleCreatedEvent
SaleCreatedConsumer
SaleStateMachine
SaleSagaState
SaleCompletedEvent
SaleFailedEvent
IRealtimeNotifier
MassTransitEventBus
SignalRRealtimeNotifier
```

Evita nombres genéricos como:

```txt
Manager
Helper
Processor
Service
Utils
```

Solo usa `Service` cuando represente claramente una abstracción de aplicación o infraestructura.

---

## 20. Qué no debes hacer

No debes:

```txt
Meter lógica de negocio en controllers.
Usar DbContext directamente desde API endpoints.
Usar MassTransit directamente desde Domain.
Usar SignalR directamente desde Application.
Crear eventos sin BusinessId.
Crear consultas sin filtro por BusinessId.
Crear clases gigantes con muchas responsabilidades.
Ignorar CancellationToken.
Ignorar validaciones.
Publicar eventos críticos sin Outbox.
Descontar inventario dos veces.
Generar facturas duplicadas.
Mezclar lógica fiscal dentro de la entidad Sale.
Usar Next.js para el dashboard interno del MVP.
Convertir todo en microservicios en el MVP.
Introducir issues nuevos de SonarQube.
Bajar la cobertura del código nuevo por debajo de 80%.
Superar 3% de duplicación en código nuevo.
```

---

## 21. Qué sí debes hacer

Debes:

```txt
Respetar Clean Architecture.
Aplicar SOLID.
Separar API, Application, Domain, Contracts e Infrastructure.
Crear casos de uso pequeños.
Crear consumers delgados.
Usar abstracciones para infraestructura.
Incluir BusinessId en entidades y eventos.
Usar CorrelationId en eventos.
Usar CancellationToken en métodos async.
Usar FluentValidation para requests.
Usar ApiResponse estándar.
Usar logs estructurados.
Pensar en idempotencia.
Mantener código simple y testeable.
Cumplir SonarQube/SonarLint.
Agregar o actualizar pruebas.
```

---

## 22. Checklist antes de aceptar cualquier cambio

Antes de finalizar una tarea, debes validar:

```txt
¿La funcionalidad está en la capa correcta?
¿Domain sigue sin depender de infraestructura?
¿Application usa abstracciones?
¿Infrastructure implementa detalles técnicos?
¿API solo llama casos de uso?
¿Worker solo consume eventos y delega?
¿Los eventos incluyen BusinessId?
¿Los eventos críticos usan Outbox?
¿Las consultas filtran por BusinessId?
¿Se respetan SOLID?
¿Hay validaciones?
¿Hay logs suficientes?
¿Se evita duplicidad/idempotencia?
¿El frontend usa hooks/servicios en vez de lógica desordenada?
¿Compila sin warnings relevantes?
¿Pasan los tests?
¿La cobertura del código nuevo se mantiene >= 80%?
¿La duplicación del código nuevo se mantiene <= 3%?
¿No introduces bugs, vulnerabilities ni code smells?
¿No introduces secretos hardcodeados?
¿No rompes contratos de eventos?
```

---

## 23. Estrategia oficial de pruebas

Cuando implementes una funcionalidad, debes incluir o actualizar pruebas. No consideres completa una tarea si no tiene pruebas mínimas relacionadas con el cambio.

Debes usar esta estrategia:

```txt
Unit Tests
Integration Tests
Contract Tests
Consumer Tests
Saga State Machine Tests
Outbox/Inbox Tests
API Tests
Frontend Component Tests
Frontend Integration Tests
End-to-End Tests
Architecture Tests
```

Regla obligatoria:

> Debes validar reglas de negocio, casos de uso, eventos, consumers, sagas, multi-tenancy, idempotencia y comportamiento visible en React.

---

## 24. Pirámide de pruebas

Usa esta distribución recomendada:

```txt
70% Unit Tests
20% Integration Tests
10% End-to-End Tests
```

Interpretación:

```txt
Unit Tests: reglas rápidas del dominio y casos de uso.
Integration Tests: API, base de datos, eventos, consumers, saga y outbox.
E2E Tests: flujos reales desde React hasta backend.
```

No conviertas todos los tests en E2E. Los E2E son valiosos, pero más lentos y frágiles.

---

## 25. Tests obligatorios por capa

### 25.1 Domain Tests

Debes probar reglas puras de negocio.

Ejemplos:

```txt
Sale no puede completarse si no está en Processing.
Sale no puede tener total negativo.
SaleItem no permite cantidad menor o igual a cero.
Product no permite precio de venta negativo.
CustomerCredit no permite abono mayor a la deuda.
Invoice no permite NCF duplicado dentro del negocio.
InventoryMovement calcula correctamente PreviousStock y NewStock.
```

Reglas:

```txt
No uses base de datos.
No uses mocks de infraestructura.
No uses MassTransit.
No uses SignalR.
Haz tests rápidos y determinísticos.
```

---

### 25.2 Application Tests

Debes probar casos de uso y orquestación.

Ejemplos:

```txt
CreateSaleHandler crea venta en estado Received.
CreateSaleHandler publica SaleCreatedEvent o registra Domain Event.
CreateProductHandler valida nombre, precio y BusinessId.
RegisterCustomerPaymentHandler rechaza abonos inválidos.
AdjustInventoryHandler crea movimiento de inventario.
GenerateInvoiceHandler asigna secuencia correctamente.
```

Reglas:

```txt
Usa repositorios mock cuando solo pruebes orquestación.
Usa FluentAssertions para aserciones legibles.
Valida CancellationToken cuando aplique.
Valida que BusinessId esté presente.
```

---

### 25.3 Infrastructure Tests

Debes probar integraciones reales con tecnología.

Ejemplos:

```txt
EF Core guarda y lee entidades correctamente.
Filtros por BusinessId funcionan.
OutboxMessages se crean al guardar eventos de dominio.
UnitOfWork confirma transacciones.
Repositories no devuelven datos de otro BusinessId.
```

Reglas:

```txt
Usa PostgreSQL real con Testcontainers.
No uses InMemoryDatabase de EF Core para pruebas críticas.
Limpia base de datos entre pruebas con Respawn o recreación controlada.
```

---

### 25.4 API Tests

Debes probar endpoints reales con WebApplicationFactory.

Ejemplos:

```txt
POST /api/auth/login devuelve token con credenciales válidas.
POST /api/auth/login devuelve 401 con credenciales inválidas.
GET /health responde OK.
POST /api/sales requiere autenticación.
POST /api/sales crea venta Received.
Endpoints no exponen datos de otro BusinessId.
Errores devuelven ApiResponse estándar.
Validaciones devuelven 400 con mensajes claros.
```

Reglas:

```txt
Usa WebApplicationFactory.
Usa JWT de prueba o autenticación fake controlada.
No pruebes lógica de dominio en controllers.
Valida status code y estructura de respuesta.
```

---

### 25.5 Worker Tests

Debes probar consumers y procesamiento de mensajes.

Ejemplos:

```txt
SaleCreatedConsumer recibe SaleCreatedEvent.
ValidateStockConsumer publica StockValidated o StockValidationFailed.
DeductInventoryConsumer descuenta inventario una sola vez.
RegisterPaymentConsumer registra pago correcto.
GenerateInvoiceConsumer genera factura o publica InvoiceFailed.
NotificationConsumer llama IRealtimeNotifier.
```

Reglas:

```txt
Usa MassTransit Test Harness para consumers.
Valida eventos publicados.
Valida idempotencia.
Valida CorrelationId.
Valida BusinessId.
```

---

### 25.6 Saga State Machine Tests

Debes probar transiciones de estado y flujos de negocio.

Ejemplos para SaleStateMachine:

```txt
SaleCreated inicia saga en Processing.
StockValidated mueve la saga hacia InventoryPending o equivalente.
InventoryDeducted permite registrar pago.
PaymentRegistered permite generar recibo/factura.
InvoiceGenerated completa la venta.
StockValidationFailed marca venta como Failed.
PaymentFailed marca venta como Failed o CompensationPending.
Eventos duplicados no duplican inventario ni factura.
```

Reglas:

```txt
Usa MassTransit Test Harness.
Valida estado actual de la saga.
Valida eventos publicados por cada transición.
Valida comportamiento ante eventos fuera de orden.
Valida comportamiento ante mensajes duplicados.
```

---

### 25.7 Outbox e Inbox Tests

Debes probar confiabilidad de eventos.

Ejemplos:

```txt
Si se crea una venta, se persiste también el OutboxMessage.
Si falla la transacción, no queda OutboxMessage huérfano.
OutboxPublisher publica mensajes pendientes.
OutboxPublisher marca mensajes como publicados.
Inbox evita procesar dos veces el mismo EventId.
Un evento duplicado no descuenta inventario dos veces.
```

Reglas:

```txt
Usa PostgreSQL real.
Valida transacciones.
Valida idempotencia.
```

---

### 25.8 Contract Tests

Debes asegurar que los contratos de eventos no se rompan.

Ejemplos:

```txt
SaleCreatedEvent mantiene propiedades requeridas.
InventoryDeductedEvent incluye SaleId, BusinessId y CorrelationId.
PaymentRegisteredEvent incluye PaymentId y SaleId.
SaleFailedEvent incluye Reason.
Las propiedades críticas no se renombran sin migración controlada.
```

Reglas:

```txt
No rompas contratos publicados sin versionado.
Si cambias un evento existente de forma incompatible, crea una versión nueva.
Ejemplo: SaleCreatedEventV2.
```

---

### 25.9 Architecture Tests

Debes proteger Clean Architecture y SOLID con pruebas.

Usa NetArchTest o una alternativa similar.

Ejemplos:

```txt
Domain no depende de Infrastructure.
Domain no depende de Application.
Domain no depende de MassTransit.
Domain no depende de EntityFrameworkCore.
Application no depende de Infrastructure.
Application no depende de ASP.NET Core.
Application no depende de SignalR.
Contracts no depende de Infrastructure.
API no es referenciada por Domain, Application ni Infrastructure.
Worker no es referenciado por Domain, Application ni Infrastructure.
```

Regla obligatoria:

> Si una prueba de arquitectura falla, no apruebes el cambio.

---

## 26. Tests obligatorios del frontend

### 26.1 Component Tests

Debes probar componentes visuales y de interacción.

Ejemplos:

```txt
LoginForm muestra errores de validación.
ProductForm valida campos requeridos.
POSCart calcula subtotal y total.
SaleStatusBadge muestra Received, Processing, Completed y Failed.
LowStockBadge aparece cuando stock <= stock mínimo.
```

Reglas:

```txt
Usa React Testing Library.
Prueba comportamiento, no detalles internos.
No dependas de clases CSS para lógica.
```

---

### 26.2 Frontend Integration Tests

Debes probar interacción entre páginas, hooks y API mockeada.

Ejemplos:

```txt
Login exitoso guarda token y redirige al dashboard.
Dashboard consume /api/version.
POS crea venta usando POST /api/sales.
POS muestra estado Processing después de crear venta.
POS actualiza a Completed cuando llega evento SignalR simulado.
Productos lista datos desde API.
```

Reglas:

```txt
Usa MSW para simular API.
Mockea SignalR de forma controlada.
No llames backend real desde tests unitarios/integración frontend.
```

---

### 26.3 End-to-End Tests

Debes probar flujos completos con Playwright.

Flujos mínimos obligatorios:

```txt
Usuario inicia sesión.
Usuario entra al dashboard.
Usuario crea producto.
Usuario registra inventario inicial.
Usuario realiza venta en POS.
Venta pasa de Processing a Completed.
Inventario se descuenta.
Dashboard muestra venta del día actualizada.
Usuario registra cliente.
Usuario realiza venta fiada.
Cuenta por cobrar aumenta.
```

Reglas:

```txt
Usa ambiente controlado de pruebas.
Usa datos seed.
No dependas de datos reales.
No pruebes todos los casos por E2E; usa E2E solo para caminos críticos.
```

---

## 27. Casos críticos que siempre debes probar

Debes crear pruebas cuando modifiques cualquiera de estos flujos:

```txt
Crear venta.
Procesar venta con Saga.
Validar stock.
Descontar inventario.
Registrar pago.
Crear venta fiada.
Registrar abono.
Generar factura.
Cancelar venta.
Ajustar inventario.
Publicar evento.
Consumir evento.
Enviar notificación SignalR.
Filtrar datos por BusinessId.
```

Regla obligatoria:

> Si el cambio toca dinero, inventario, factura, deuda o eventos, debes agregar pruebas automatizadas.

---

## 28. Convenciones de nombres para tests

Usa nombres descriptivos.

Formato recomendado en backend:

```txt
MethodName_ShouldExpectedBehavior_WhenCondition
```

Ejemplos:

```txt
CreateSale_ShouldCreateSaleAsReceived_WhenRequestIsValid
CompleteSale_ShouldThrowDomainException_WhenSaleIsNotProcessing
DeductInventory_ShouldNotRunTwice_WhenMessageIsDuplicated
Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid
```

Formato recomendado en frontend:

```txt
ComponentName should expected behavior when condition
```

Ejemplos:

```txt
LoginForm should show validation errors when fields are empty
POSCart should update total when quantity changes
SaleStatusBadge should render failed state when sale fails
```

---

## 29. Cobertura mínima

Usa esta cobertura mínima para el MVP:

```txt
Domain: 90%
Application: 80%
Infrastructure: 60%
API: 70%
Worker/Consumers/Saga: 75%
Frontend components/hooks críticos: 70%
Código nuevo según SonarQube: mínimo 80%
```

Regla importante:

> No uses cobertura como sustituto de calidad. Una prueba útil vale más que muchas pruebas vacías.

---

## 30. Definition of Done

No consideres terminada una tarea hasta cumplir esto:

```txt
El código compila.
No hay warnings relevantes.
Los tests existentes pasan.
Agregaste tests para la funcionalidad nueva.
Actualizaste tests afectados.
No rompiste contratos de eventos.
No rompiste reglas de arquitectura.
Los endpoints críticos tienen pruebas.
Los consumers críticos tienen pruebas.
Las reglas de negocio están cubiertas.
El frontend tiene pruebas si modificaste UI crítica.
No introdujiste issues nuevos de Sonar.
La cobertura de código nuevo es >= 80%.
La duplicación de código nuevo es <= 3%.
Los Security Hotspots están revisados.
No agregaste deuda técnica sin justificación.
```

---

## 31. Checklist de testing para ti

Antes de finalizar cualquier tarea, debes responder internamente:

```txt
¿Qué regla de negocio cambié?
¿Qué caso de uso cambié?
¿Qué evento publiqué o consumí?
¿Qué pasa si el mensaje llega duplicado?
¿Qué pasa si el mensaje llega fuera de orden?
¿Qué pasa si falla la base de datos?
¿Qué pasa si falla el broker?
¿Qué pasa si el usuario pertenece a otro BusinessId?
¿Qué prueba garantiza que no rompí Clean Architecture?
¿Qué prueba garantiza que el frontend muestra el estado correcto?
¿Qué prueba garantiza que SonarQube no marcará deuda nueva?
```

Si alguna pregunta aplica y no tiene prueba, debes agregarla.

---

## 32. Frase oficial de arquitectura

SaasCommerce RD utiliza Clean Architecture con principios SOLID sobre .NET, separando Domain, Application, Contracts, Infrastructure, API y Worker. Debes manejar los procesos críticos mediante arquitectura orientada a eventos con MassTransit, Saga State Machine, Outbox Pattern y SignalR para notificaciones en tiempo real hacia React.
# Regla de onboarding de negocio

El registro inicial de un negocio debe crear un tenant valido y completo.

No debes permitir registrar un negocio sin:

- Nombre del negocio.
- Tipo de identificacion.
- Numero de identificacion.
- Al menos un telefono.
- Exactamente un telefono principal.
- Nombre del administrador.
- Email del administrador.
- Contrasena valida.
- Sucursal principal.

Las validaciones deben existir en backend con FluentValidation o validadores equivalentes. El frontend debe duplicar esas validaciones con Zod para mejorar la experiencia, pero el backend siempre es la fuente de verdad.

No debes depender del seed como flujo principal de onboarding.

# Regla de settings seguros

Los endpoints de perfil, comercio y sucursal deben usar siempre el contexto autenticado.

Reglas:

- `UserId` debe salir del JWT o de `ICurrentUserService`.
- `BusinessId` debe salir del JWT o de `ICurrentUserService`.
- `BranchId` debe salir del JWT o de `ICurrentUserService`.
- No aceptar `BusinessId`, `BranchId`, `UserId`, roles, tokens ni password hash desde requests de settings.
- `/api/business/current` no debe permitir consultar ni editar otro negocio por id.
- `/api/branches/current` no debe permitir consultar ni editar sucursales fuera del `BusinessId` autenticado.
- Cambios de contrasena deben validar la contrasena actual y guardar solo hash.
- Las respuestas deben usar `ApiResponse` con `isSuccess`, `data` y `error`.
