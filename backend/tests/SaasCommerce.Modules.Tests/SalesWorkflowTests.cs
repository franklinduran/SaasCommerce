#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Sales.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class SalesWorkflowTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 16, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task CreateSale_ShouldPersistSaleAndQueueSaleCreated()
  {
    var scenario = TestScenario.Create();
    var saleCreated = SaleCreated(scenario);
    var useCase = new CreateSaleUseCase(
      scenario.Sales,
      scenario.Outbox,
      scenario.Realtime,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(saleCreated);
    var persisted = await scenario.Sales.GetAsync(new BusinessId(scenario.BusinessId), scenario.SaleId);

    result.IsSuccess.Should().BeTrue();
    persisted.Should().NotBeNull();
    persisted!.Status.Should().Be(SaleStatus.Processing);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is SaleCreatedEventV1);
    scenario.Realtime.Notifications.Should().ContainSingle();
  }

  [Fact]
  public async Task CreateSale_ShouldIgnoreDuplicateSale()
  {
    var scenario = TestScenario.Create();
    await scenario.Sales.AddAsync(CreateProcessingSale(scenario));
    var useCase = new CreateSaleUseCase(
      scenario.Sales,
      scenario.Outbox,
      scenario.Realtime,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(SaleCreated(scenario));

    result.IsSuccess.Should().BeTrue();
    scenario.Outbox.Events.Should().BeEmpty();
    scenario.Realtime.Notifications.Should().BeEmpty();
  }

  [Fact]
  public async Task CreateSale_ShouldRejectInvalidSale()
  {
    var scenario = TestScenario.Create();
    var useCase = new CreateSaleUseCase(
      scenario.Sales,
      scenario.Outbox,
      scenario.Realtime,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new SaleCreatedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      [],
      scenario.Total,
      "Cash",
      Now));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task EfSaleRepository_ShouldPersistAndLoadSaleItems()
  {
    await using var dbContext = CreateDbContext();
    var repository = new EfSaleRepository(dbContext);
    var scenario = TestScenario.Create();
    var sale = CreateProcessingSale(scenario);

    await repository.AddAsync(sale);
    await dbContext.SaveChangesAsync();

    var loaded = await repository.GetAsync(new BusinessId(scenario.BusinessId), scenario.SaleId);

    loaded.Should().NotBeNull();
    loaded!.Items.Should().ContainSingle();
    loaded.Status.Should().Be(SaleStatus.Processing);
  }

  [Fact]
  public async Task ValidateSaleStock_ShouldPublishStockValidated_WhenStockIsAvailable()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, true));
    scenario.InventoryAvailability.Results[scenario.ProductId] = Availability(scenario, true);
    var useCase = new ValidateSaleStockUseCase(
      scenario.ProductPolicies,
      scenario.InventoryAvailability,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(StockValidationRequested(scenario));

    result.IsSuccess.Should().BeTrue();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is StockValidatedEventV1);
  }

  [Fact]
  public async Task ValidateSaleStock_ShouldPublishStockValidationFailed_WhenStockIsInsufficient()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, true));
    scenario.InventoryAvailability.Results[scenario.ProductId] =
      Availability(scenario, false, "Insufficient stock.");
    var useCase = new ValidateSaleStockUseCase(
      scenario.ProductPolicies,
      scenario.InventoryAvailability,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(StockValidationRequested(scenario));

    result.IsSuccess.Should().BeTrue();
    scenario.Outbox.Events
      .OfType<StockValidationFailedEventV1>()
      .Should()
      .ContainSingle(failed => failed.Reason == "Insufficient stock.");
  }

  [Fact]
  public async Task DeductSaleInventory_ShouldPublishInventoryDeducted_WhenStockIsAvailable()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, true));
    scenario.InventoryAvailability.Results[scenario.ProductId] = Availability(scenario, true, available: 10);
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(scenario.BusinessId),
      new BranchId(scenario.BranchId),
      scenario.ProductId,
      Now);
    stockItem.ApplyAdjustment(10, InventoryMovementReason.InitialLoad, scenario.UserId, false, Now);
    await scenario.Inventory.AddStockItemAsync(stockItem);
    var useCase = new DeductSaleInventoryUseCase(
      scenario.ProductPolicies,
      scenario.InventoryAvailability,
      scenario.Inventory,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(InventoryDeductionRequested(scenario));

    result.IsSuccess.Should().BeTrue();
    stockItem.Quantity.Should().Be(8);
    scenario.Inventory.Movements.Should().ContainSingle(movement => movement.Reason == InventoryMovementReason.Sale);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InventoryDeductedEventV1);
  }

  [Fact]
  public async Task RegisterSalePayment_ShouldPublishPaymentRegistered_WhenPaymentIsValid()
  {
    var scenario = TestScenario.Create();
    var useCase = new RegisterSalePaymentUseCase(
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new PaymentRegistrationRequestedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      scenario.Total,
      "Cash",
      Now));

    result.IsSuccess.Should().BeTrue();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is PaymentRegisteredEventV1);
  }

  [Fact]
  public async Task RegisterSalePayment_ShouldPublishPaymentFailed_WhenPaymentIsInvalid()
  {
    var scenario = TestScenario.Create();
    var useCase = new RegisterSalePaymentUseCase(
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new PaymentRegistrationRequestedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      0,
      "",
      Now));

    result.IsSuccess.Should().BeTrue();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is PaymentFailedEventV1);
  }

  [Fact]
  public async Task GenerateSaleInvoice_ShouldPublishInvoiceGenerated_WhenInvoiceIsCreated()
  {
    var scenario = TestScenario.Create();
    var useCase = new GenerateSaleInvoiceUseCase(
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new InvoiceGenerationRequestedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      Guid.NewGuid(),
      scenario.Total,
      Now));

    result.IsSuccess.Should().BeTrue();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InvoiceGeneratedEventV1);
  }

  [Fact]
  public async Task GenerateSaleInvoice_ShouldPublishInvoiceFailed_WhenPaymentIdIsMissing()
  {
    var scenario = TestScenario.Create();
    var useCase = new GenerateSaleInvoiceUseCase(
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new InvoiceGenerationRequestedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      Guid.Empty,
      scenario.Total,
      Now));

    result.IsSuccess.Should().BeTrue();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InvoiceFailedEventV1);
  }

  [Fact]
  public async Task CompleteSale_ShouldNotifyBusiness_WhenSaleIsCompleted()
  {
    var scenario = TestScenario.Create();
    var sale = CreateProcessingSale(scenario);
    await scenario.Sales.AddAsync(sale);
    var useCase = new CompleteSaleUseCase(
      scenario.Sales,
      scenario.Realtime,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new SaleCompletedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      Guid.NewGuid(),
      Guid.NewGuid(),
      scenario.Total,
      Now));

    result.IsSuccess.Should().BeTrue();
    sale.Status.Should().Be(SaleStatus.Completed);
    scenario.Realtime.Notifications.Should().ContainSingle(notification =>
      notification.BusinessId == scenario.BusinessId &&
      notification.EventName == SaleRealtimeEvents.StatusChanged);
  }

  [Fact]
  public async Task CompleteSale_ShouldReturnFailure_WhenSaleIsMissing()
  {
    var scenario = TestScenario.Create();
    var useCase = new CompleteSaleUseCase(
      scenario.Sales,
      scenario.Realtime,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new SaleCompletedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      Guid.NewGuid(),
      Guid.NewGuid(),
      scenario.Total,
      Now));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.SaleNotFound);
  }

  [Fact]
  public async Task FailSale_ShouldNotifyBusiness_WhenSaleFails()
  {
    var scenario = TestScenario.Create();
    var sale = CreateProcessingSale(scenario);
    await scenario.Sales.AddAsync(sale);
    var useCase = new FailSaleUseCase(
      scenario.Sales,
      scenario.Realtime,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new SaleFailedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      "invoice failed",
      Now));

    result.IsSuccess.Should().BeTrue();
    sale.Status.Should().Be(SaleStatus.Failed);
    scenario.Realtime.Notifications.Should().ContainSingle(notification =>
      notification.BusinessId == scenario.BusinessId &&
      notification.EventName == SaleRealtimeEvents.StatusChanged);
  }

  [Fact]
  public async Task FailSale_ShouldIgnoreCompletedSale()
  {
    var scenario = TestScenario.Create();
    var sale = CreateProcessingSale(scenario);
    sale.Complete(Now);
    await scenario.Sales.AddAsync(sale);
    var useCase = new FailSaleUseCase(
      scenario.Sales,
      scenario.Realtime,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new SaleFailedEventV1(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      "late failure",
      Now));

    result.IsSuccess.Should().BeTrue();
    sale.Status.Should().Be(SaleStatus.Completed);
    scenario.Realtime.Notifications.Should().BeEmpty();
  }

  private static SaleCreatedEventV1 SaleCreated(TestScenario scenario)
    => new(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      [new SaleItemV1(scenario.ProductId, 2, 100)],
      scenario.Total,
      "Cash",
      Now);

  private static Sale CreateProcessingSale(TestScenario scenario)
  {
    var sale = Sale.Create(
      scenario.SaleId,
      new BusinessId(scenario.BusinessId),
      new BranchId(scenario.BranchId),
      scenario.UserId,
      [new SaleLine(scenario.ProductId, 2, 100)],
      "Cash",
      Now);
    sale.MarkAsProcessing(Now);
    return sale;
  }

  private static StockValidationRequestedEventV1 StockValidationRequested(TestScenario scenario)
    => new(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      [new SaleItemV1(scenario.ProductId, 2, 100)],
      scenario.Total,
      "Cash",
      Now);

  private static InventoryDeductionRequestedEventV1 InventoryDeductionRequested(TestScenario scenario)
    => new(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.SaleId,
      scenario.BusinessId,
      scenario.BranchId,
      scenario.UserId,
      [new SaleItemV1(scenario.ProductId, 2, 100)],
      scenario.Total,
      "Cash",
      Now);

  private static ProductSalesPolicy Policy(Guid productId, Guid businessId, bool trackInventory)
    => new(
      productId,
      businessId,
      "Cafe",
      "SKU-001",
      null,
      "Simple",
      "Unit",
      100,
      "Itbis18",
      18,
      true,
      true,
      trackInventory,
      false,
      true,
      true,
      null);

  private static InventoryAvailabilityResult Availability(
    TestScenario scenario,
    bool isAvailable,
    string? reason = null,
    decimal available = 10)
    => new(
      scenario.BusinessId,
      scenario.BranchId,
      scenario.ProductId,
      2,
      available,
      isAvailable,
      reason);

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private sealed class TestScenario
  {
    private TestScenario()
    {
    }

    public Guid BusinessId { get; } = Guid.NewGuid();

    public Guid BranchId { get; } = Guid.NewGuid();

    public Guid UserId { get; } = Guid.NewGuid();

    public Guid SaleId { get; } = Guid.NewGuid();

    public Guid ProductId { get; } = Guid.NewGuid();

    public Guid CorrelationId { get; } = Guid.NewGuid();

    public decimal Total { get; } = 200;

    public RecordingOutboxWriter Outbox { get; } = new();

    public FixedClock Clock { get; } = new();

    public NoopUnitOfWork UnitOfWork { get; } = new();

    public FakeProductSalesPolicyReader ProductPolicies { get; } = new();

    public FakeInventoryAvailabilityService InventoryAvailability { get; } = new();

    public InMemoryInventoryRepository Inventory { get; } = new();

    public InMemorySaleRepository Sales { get; } = new();

    public RecordingRealtimeNotifier Realtime { get; } = new();

    public static TestScenario Create() => new();
  }

  private sealed class RecordingOutboxWriter : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(
      TEvent integrationEvent,
      CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class NoopUnitOfWork : IUnitOfWork
  {
    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveChangesCount++;
      return Task.FromResult(1);
    }
  }

  private sealed class FakeProductSalesPolicyReader : IProductSalesPolicyReader
  {
    private readonly Dictionary<Guid, ProductSalesPolicy> policies = [];

    public void Add(ProductSalesPolicy policy) => policies[policy.ProductId] = policy;

    public Task<ProductSalesPolicy?> GetSalesPolicyAsync(
      Guid businessId,
      Guid productId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        policies.TryGetValue(productId, out var policy) && policy.BusinessId == businessId
          ? policy
          : null);
  }

  private sealed class FakeInventoryAvailabilityService : IInventoryAvailabilityService
  {
    public Dictionary<Guid, InventoryAvailabilityResult> Results { get; } = [];

    public Task<InventoryAvailabilityResult> ValidateStockAsync(
      InventoryAvailabilityRequest request,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        Results.TryGetValue(request.ProductId, out var result)
          ? result
          : new InventoryAvailabilityResult(
            request.BusinessId,
            request.BranchId,
            request.ProductId,
            request.Quantity,
            request.Quantity,
            true,
            null));
  }

  private sealed class InMemoryInventoryRepository : IInventoryRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid BranchId, Guid ProductId), StockItem> stockItems = [];

    public List<InventoryMovement> Movements { get; } = [];

    public Task<StockItem?> GetStockItemAsync(
      BusinessId businessId,
      BranchId branchId,
      Guid productId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        stockItems.TryGetValue((businessId.Value, branchId.Value, productId), out var stockItem)
          ? stockItem
          : null);

    public Task AddStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default)
    {
      stockItems[(stockItem.BusinessId.Value, stockItem.BranchId.Value, stockItem.ProductId)] = stockItem;
      return Task.CompletedTask;
    }

    public Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
      Movements.Add(movement);
      return Task.CompletedTask;
    }

    public Task<int> CountStockAsync(
      BusinessId businessId,
      BranchId branchId,
      StockSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<StockItem>> ListStockAsync(
      BusinessId businessId,
      BranchId branchId,
      StockSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<StockItem>>([]);

    public Task<int> CountMovementsAsync(
      BusinessId businessId,
      BranchId branchId,
      InventoryMovementSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<InventoryMovement>> ListMovementsAsync(
      BusinessId businessId,
      BranchId branchId,
      InventoryMovementSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<InventoryMovement>>([]);
  }

  private sealed class InMemorySaleRepository : ISaleRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid SaleId), Sale> sales = [];

    public Task<Sale?> GetAsync(
      BusinessId businessId,
      Guid saleId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        sales.TryGetValue((businessId.Value, saleId), out var sale)
          ? sale
          : null);

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
      sales[(sale.BusinessId.Value, sale.Id)] = sale;
      return Task.CompletedTask;
    }
  }

  private sealed class RecordingRealtimeNotifier : IRealtimeNotifier
  {
    public List<RealtimeNotification> Notifications { get; } = [];

    public Task NotifyBusinessAsync(
      Guid businessId,
      string eventName,
      object payload,
      CancellationToken cancellationToken = default)
    {
      Notifications.Add(new RealtimeNotification(businessId, eventName, payload));
      return Task.CompletedTask;
    }

    public Task NotifyBranchAsync(
      Guid branchId,
      string eventName,
      object payload,
      CancellationToken cancellationToken = default)
      => Task.CompletedTask;

    public Task NotifyUserAsync(
      Guid userId,
      string eventName,
      object payload,
      CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed record RealtimeNotification(Guid BusinessId, string EventName, object Payload);
}

#pragma warning restore CA1707
