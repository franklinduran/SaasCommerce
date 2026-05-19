#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class SalesUseCaseTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);
  private readonly Guid businessId = Guid.NewGuid();
  private readonly Guid branchId = Guid.NewGuid();
  private readonly Guid userId = Guid.NewGuid();
  private readonly Guid productId = Guid.NewGuid();
  private readonly Guid saleId = Guid.NewGuid();

  // ── ListSalesUseCase ─────────────────────────────────────────────────────

  [Fact]
  public async Task ListSales_ShouldFail_WhenNoBusinessContext()
  {
    var uc = new ListSalesUseCase(new StubSaleReadRepo(), Anonymous());

    var result = await uc.ExecuteAsync(new ListSalesQuery(null, null, null, null, null, 1, 10, null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.UserContextRequired);
  }

  [Fact]
  public async Task ListSales_ShouldFail_WhenPageInvalid()
  {
    var uc = new ListSalesUseCase(new StubSaleReadRepo(), Authed());

    var result = await uc.ExecuteAsync(new ListSalesQuery(null, null, null, null, null, 0, 10, null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task ListSales_ShouldFail_WhenStatusInvalid()
  {
    var uc = new ListSalesUseCase(new StubSaleReadRepo(), Authed());

    var result = await uc.ExecuteAsync(
      new ListSalesQuery(null, "NotAStatus", null, null, null, 1, 10, null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task ListSales_ShouldFail_WhenSortInvalid()
  {
    var uc = new ListSalesUseCase(new StubSaleReadRepo(), Authed());

    var result = await uc.ExecuteAsync(
      new ListSalesQuery(null, null, null, null, null, 1, 10, "Nope", null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task ListSales_ShouldReturnSales_WhenValid()
  {
    var repo = new StubSaleReadRepo();
    repo.Items.Add(SampleResponse());
    var uc = new ListSalesUseCase(repo, Authed());

    var result = await uc.ExecuteAsync(
      new ListSalesQuery(branchId, "Completed", "abc", Now.AddDays(-1), Now, 1, 25, "Total", "Asc"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.TotalItems.Should().Be(1);
  }

  // ── ValidateSaleStockUseCase ─────────────────────────────────────────────

  [Fact]
  public async Task ValidateStock_ShouldPublishFailed_WhenSaleNotFound()
  {
    var ctx = new Ctx();
    var uc = ctx.Create();

    var result = await uc.ExecuteAsync(Request(ctx, [Item(1)]));

    result.IsSuccess.Should().BeTrue();
    ctx.Outbox.Events.OfType<StockValidationFailedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldMarkProcessingAndNotify_WhenReceived()
  {
    var ctx = new Ctx();
    var sale = ctx.SeedSale(SaleState.Received);
    ctx.Policies.Set(productId, MakePolicy(canBeSold: true, track: true));
    ctx.Availability.Available = true;
    var uc = ctx.Create();

    var result = await uc.ExecuteAsync(Request(ctx, [Item(2)]));

    result.IsSuccess.Should().BeTrue();
    sale.Status.Should().Be(SaleStatus.Processing);
    ctx.Realtime.Notifications.Should().NotBeEmpty();
    ctx.Outbox.Events.OfType<StockValidatedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldPublishFailed_WhenSaleNotProcessable()
  {
    var ctx = new Ctx();
    ctx.SeedSale(SaleState.Completed);
    var uc = ctx.Create();

    var result = await uc.ExecuteAsync(Request(ctx, [Item(1)]));

    result.IsSuccess.Should().BeTrue();
    ctx.Outbox.Events.OfType<StockValidationFailedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldFail_WhenNoItems()
  {
    var ctx = new Ctx();
    ctx.SeedSale(SaleState.Processing);
    var uc = ctx.Create();

    var result = await uc.ExecuteAsync(Request(ctx, []));

    ctx.Outbox.Events.OfType<StockValidationFailedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldFail_WhenQuantityInvalid()
  {
    var ctx = new Ctx();
    ctx.SeedSale(SaleState.Processing);
    var uc = ctx.Create();

    await uc.ExecuteAsync(Request(ctx, [Item(0)]));

    ctx.Outbox.Events.OfType<StockValidationFailedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldFail_WhenProductPolicyMissing()
  {
    var ctx = new Ctx();
    ctx.SeedSale(SaleState.Processing);
    var uc = ctx.Create();

    await uc.ExecuteAsync(Request(ctx, [Item(1)]));

    ctx.Outbox.Events.OfType<StockValidationFailedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldFail_WhenProductCannotBeSold()
  {
    var ctx = new Ctx();
    ctx.SeedSale(SaleState.Processing);
    ctx.Policies.Set(productId, MakePolicy(canBeSold: false, track: true));
    var uc = ctx.Create();

    await uc.ExecuteAsync(Request(ctx, [Item(1)]));

    ctx.Outbox.Events.OfType<StockValidationFailedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldFail_WhenStockUnavailable()
  {
    var ctx = new Ctx();
    ctx.SeedSale(SaleState.Processing);
    ctx.Policies.Set(productId, MakePolicy(canBeSold: true, track: true));
    ctx.Availability.Available = false;
    var uc = ctx.Create();

    await uc.ExecuteAsync(Request(ctx, [Item(5)]));

    ctx.Outbox.Events.OfType<StockValidationFailedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task ValidateStock_ShouldSucceed_WhenProcessingAndStockAvailable()
  {
    var ctx = new Ctx();
    ctx.SeedSale(SaleState.Processing);
    ctx.Policies.Set(productId, MakePolicy(canBeSold: true, track: true));
    ctx.Availability.Available = true;
    var uc = ctx.Create();

    var result = await uc.ExecuteAsync(Request(ctx, [Item(3)]));

    result.IsSuccess.Should().BeTrue();
    ctx.Outbox.Events.OfType<StockValidatedEventV1>().Should().ContainSingle();
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private SaleItemV1 Item(decimal qty) => new(productId, qty, 100);

  private StockValidationRequestedEventV1 Request(Ctx ctx, IReadOnlyCollection<SaleItemV1> items)
    => new(Guid.NewGuid(), Guid.NewGuid(), saleId, businessId, branchId, userId,
      items, items.Sum(i => i.Quantity * i.UnitPrice), "Cash", Now);

  private ProductSalesPolicy MakePolicy(bool canBeSold, bool track)
    => new(productId, businessId, "P", "SKU", null, "Simple", "Unit", 100, "Itbis18", 18,
      true, true, track, false, true, canBeSold, canBeSold ? null : "no vendible");

  private SaleResponse SampleResponse()
    => new(saleId, businessId, branchId, userId, null, null, null, "Completed", "Cash", 100,
      [], Now, Now, Now, null, null, null, null);

  private static FakeUser Authed()
    => new() { BusinessId = Guid.NewGuid(), UserId = Guid.NewGuid(), BranchId = Guid.NewGuid(), IsAuthenticated = true };

  private static FakeUser Anonymous()
    => new() { IsAuthenticated = false };

  private enum SaleState { Received, Processing, Completed }

  private sealed class Ctx
  {
    public StubSaleRepo Sales { get; } = new();
    public StubPolicyReader Policies { get; } = new();
    public StubAvailability Availability { get; } = new();
    public RecordingOutbox Outbox { get; } = new();
    public RecordingRealtime Realtime { get; } = new();
    public Guid BusinessId { get; } = Guid.NewGuid();
    public Guid BranchId { get; } = Guid.NewGuid();
    public Guid UserId { get; } = Guid.NewGuid();

    public ValidateSaleStockUseCase Create()
      => new(Sales, Policies, Availability, Outbox, Realtime, new FixedClock(), new NoopUow());

    public Sale SeedSale(SaleState state)
    {
      var pid = Guid.NewGuid();
      var sale = Sale.Create(
        Guid.NewGuid(), new BusinessId(Guid.NewGuid()), new BranchId(Guid.NewGuid()),
        Guid.NewGuid(), [new SaleLine(pid, 1, 100)], "Cash", Now);
      if (state is SaleState.Processing or SaleState.Completed)
      {
        sale.MarkAsProcessing(Now);
      }

      if (state is SaleState.Completed)
      {
        sale.Complete(Now);
      }

      Sales.Stored = sale;
      return sale;
    }
  }

  private sealed class FakeUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }
    public Guid? BusinessId { get; init; }
    public Guid? BranchId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class NoopUow : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
  }

  private sealed class RecordingOutbox : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class RecordingRealtime : IRealtimeNotifier
  {
    public List<object> Notifications { get; } = [];

    public Task NotifyBusinessAsync(Guid businessId, string eventName, object payload, CancellationToken cancellationToken = default)
    {
      Notifications.Add(payload);
      return Task.CompletedTask;
    }

    public Task NotifyBranchAsync(Guid branchId, string eventName, object payload, CancellationToken cancellationToken = default)
      => Task.CompletedTask;

    public Task NotifyUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class StubSaleRepo : ISaleRepository
  {
    public Sale? Stored { get; set; }

    public Task<Sale?> GetAsync(BusinessId businessId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(Stored);

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<int> CountAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<Sale>> ListAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Sale>>([]);
  }

  private sealed class StubSaleReadRepo : ISaleReadRepository
  {
    public List<SaleResponse> Items { get; } = [];

    public Task<SaleResponse?> GetAsync(BusinessId businessId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(Items.FirstOrDefault());

    public Task<int> CountAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(Items.Count);

    public Task<IReadOnlyCollection<SaleResponse>> ListAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<SaleResponse>>(Items.ToArray());
  }

  private sealed class StubPolicyReader : IProductSalesPolicyReader
  {
    private readonly Dictionary<Guid, ProductSalesPolicy> map = [];

    public void Set(Guid id, ProductSalesPolicy policy) => map[id] = policy;

    public Task<ProductSalesPolicy?> GetSalesPolicyAsync(Guid businessId, Guid productId, CancellationToken cancellationToken = default)
      => Task.FromResult(map.TryGetValue(productId, out var p) ? p : null);
  }

  private sealed class StubAvailability : IInventoryAvailabilityService
  {
    public bool Available { get; set; } = true;

    public Task<InventoryAvailabilityResult> ValidateStockAsync(InventoryAvailabilityRequest request, CancellationToken cancellationToken = default)
      => Task.FromResult(new InventoryAvailabilityResult(
        request.BusinessId, request.BranchId, request.ProductId,
        request.Quantity, Available ? request.Quantity : 0, Available,
        Available ? null : "sin stock"));
  }
}

#pragma warning restore CA1707
