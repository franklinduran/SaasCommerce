#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Catalog.Infrastructure.Inventory;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Application.Suppliers;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.Modules.Purchasing.Infrastructure.Persistence;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class PurchasingTests
{
  [Fact]
  public void Purchase_ShouldThrow_WhenHasNoItems()
  {
    var clock = new FixedClock();
    var action = () => Purchase.Create(
      new PurchaseCreationData(
        Guid.NewGuid(),
        new BusinessId(Guid.NewGuid()),
        new BranchId(Guid.NewGuid()),
        Guid.NewGuid(),
        Guid.NewGuid(),
        null,
        clock.UtcNow,
        null,
        clock.UtcNow),
      []);

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Purchase_ShouldThrow_WhenItemQuantityIsZero()
  {
    var action = () => CreatePurchase([new PurchaseLine(Guid.NewGuid(), 0, 10)]);

    action.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void Purchase_ShouldThrow_WhenUnitCostIsNegative()
  {
    var action = () => CreatePurchase([new PurchaseLine(Guid.NewGuid(), 1, -1)]);

    action.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void Purchase_ShouldMarkAsReceived_WhenValid()
  {
    var purchase = CreatePurchase([new PurchaseLine(Guid.NewGuid(), 2, 15)]);

    purchase.Receive(new FixedClock().UtcNow);

    purchase.Status.Should().Be(PurchaseStatus.Received);
    purchase.ReceivedAt.Should().NotBeNull();
  }

  [Fact]
  public void Purchase_ShouldNotReceiveTwice_WhenAlreadyReceived()
  {
    var purchase = CreatePurchase([new PurchaseLine(Guid.NewGuid(), 2, 15)]);
    purchase.Receive(new FixedClock().UtcNow);

    var action = () => purchase.Receive(new FixedClock().UtcNow);

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public async Task CreateSupplier_ShouldCreateSupplier_WhenRequestIsValid()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = new CreateSupplierHandler(
      new EfSupplierRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

    var result = await handler.Handle(new CreateSupplierCommand(
      "Distribuidora Norte",
      "101-11111-1",
      "809-555-0101",
      "ventas@norte.test",
      "Santiago"));

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().Be(currentUser.BusinessId!.Value);
    result.Value.Rnc.Should().Be("101111111");
    result.Value.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task SupplierRepository_ShouldNotReturnSuppliersFromAnotherBusiness()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var otherBusinessId = new BusinessId(Guid.NewGuid());
    var clock = new FixedClock();
    dbContext.Add(new Supplier(Guid.NewGuid(), businessId, "Proveedor A", new SupplierContactInfo(null, null, null, null), clock.UtcNow));
    dbContext.Add(new Supplier(Guid.NewGuid(), otherBusinessId, "Proveedor B", new SupplierContactInfo(null, null, null, null), clock.UtcNow));
    await dbContext.SaveChangesAsync();

    var result = await new EfSupplierRepository(dbContext)
      .ListAsync(
        businessId,
        new SupplierSearchCriteria(null, null, 1, 10, SupplierSortOption.Name, SupplierSortDirection.Asc));

    result.Should().ContainSingle();
    result.Single().BusinessId.Should().Be(businessId);
  }

  [Fact]
  public async Task ReceivePurchase_ShouldCreateInventoryMovementsAndUpdateAverageCost()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var clock = new FixedClock();
    var businessId = new BusinessId(currentUser.BusinessId!.Value);
    var branchId = new BranchId(currentUser.BranchId!.Value);
    var productId = Guid.NewGuid();
    var supplier = new Supplier(Guid.NewGuid(), businessId, "Distribuidora Norte", new SupplierContactInfo(null, null, null, null), clock.UtcNow);
    var product = Product(productId, businessId, costPrice: 10);
    var stockItem = new StockItem(Guid.NewGuid(), businessId, branchId, productId, clock.UtcNow);
    stockItem.ApplyAdjustment(10, InventoryMovementReason.InitialStock, currentUser.UserId!.Value, false, clock.UtcNow);
    dbContext.Add(supplier);
    dbContext.Add(product);
    dbContext.Add(stockItem);
    await dbContext.SaveChangesAsync();
    var outbox = new RecordingOutboxWriter();
    var handler = CreatePurchaseHandler(dbContext, currentUser, outbox);

    var result = await handler.Handle(new CreatePurchaseCommand(
      supplier.Id,
      currentUser.BranchId,
      [new CreatePurchaseItemCommand(productId, 10, 20)],
      "FAC-001",
      clock.UtcNow,
      null,
      ReceiveNow: true));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Received");
    stockItem.Quantity.Should().Be(20);
    product.CostPrice.Should().Be(15);
    dbContext.Set<InventoryMovement>()
      .Should()
      .ContainSingle(movement =>
        movement.PurchaseId == result.Value.PurchaseId &&
        movement.PreviousStock == 10 &&
        movement.NewStock == 20);
    outbox.Events.OfType<PurchaseReceivedEventV1>().Should().ContainSingle();
    outbox.Events.OfType<InventoryIncreasedEventV1>().Should().ContainSingle();
    outbox.Events.OfType<ProductCostUpdatedEventV1>()
      .Should()
      .ContainSingle(@event => @event.PreviousCost == 10 && @event.NewCost == 15);
  }

  [Fact]
  public async Task PurchaseReceivedConsumerUseCase_ShouldBeIdempotent_WhenMessageIsDuplicated()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var clock = new FixedClock();
    var businessId = new BusinessId(currentUser.BusinessId!.Value);
    var branchId = new BranchId(currentUser.BranchId!.Value);
    var productId = Guid.NewGuid();
    var supplier = new Supplier(Guid.NewGuid(), businessId, "Distribuidora Norte", new SupplierContactInfo(null, null, null, null), clock.UtcNow);
    var purchase = Purchase.Create(
      new PurchaseCreationData(
        Guid.NewGuid(),
        businessId,
        branchId,
        supplier.Id,
        currentUser.UserId!.Value,
        null,
        clock.UtcNow,
        null,
        clock.UtcNow),
      [new PurchaseLine(productId, 3, 12)]);
    purchase.Receive(clock.UtcNow);
    dbContext.Add(supplier);
    dbContext.Add(Product(productId, businessId, costPrice: 10));
    dbContext.Add(purchase);
    await dbContext.SaveChangesAsync();
    var useCase = new ProcessPurchaseReceivedEventUseCase(
      new EfPurchaseRepository(dbContext),
      CreateReceiptProcessor(dbContext, new RecordingOutboxWriter()),
      new EfUnitOfWork(dbContext));
    var message = new PurchaseReceivedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      purchase.Id,
      businessId.Value,
      branchId.Value,
      supplier.Id,
      currentUser.UserId.Value,
      [new PurchaseItemV1(productId, 3, 12, 36)],
      36,
      clock.UtcNow);

    var first = await useCase.ExecuteAsync(message);
    var second = await useCase.ExecuteAsync(message);

    first.IsSuccess.Should().BeTrue();
    second.IsSuccess.Should().BeTrue();
    dbContext.Set<InventoryMovement>()
      .Count(movement => movement.PurchaseId == purchase.Id)
      .Should()
      .Be(1);
  }

  [Fact]
  public async Task CancelPurchase_ShouldCancelPurchase_WhenPurchaseIsPending()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var clock = new FixedClock();
    var businessId = new BusinessId(currentUser.BusinessId!.Value);
    var branchId = new BranchId(currentUser.BranchId!.Value);
    var productId = Guid.NewGuid();
    var supplier = new Supplier(
      Guid.NewGuid(),
      businessId,
      "Proveedor Cancel",
      new SupplierContactInfo(null, null, null, null),
      clock.UtcNow);
    var purchase = Purchase.Create(
      new PurchaseCreationData(
        Guid.NewGuid(),
        businessId,
        branchId,
        supplier.Id,
        currentUser.UserId!.Value,
        null,
        clock.UtcNow,
        null,
        clock.UtcNow),
      [new PurchaseLine(productId, 2, 50)]);
    dbContext.Add(supplier);
    dbContext.Add(Product(productId, businessId, costPrice: 50));
    dbContext.Add(purchase);
    await dbContext.SaveChangesAsync();
    var outbox = new RecordingOutboxWriter();
    var handler = new CancelPurchaseHandler(
      new PurchaseHandlerContext(
        new EfPurchaseRepository(dbContext),
        new EfSupplierRepository(dbContext),
        new EfProductPurchaseReader(dbContext),
        currentUser,
        outbox,
        clock,
        new EfUnitOfWork(dbContext)),
      new FixedCorrelationIdProvider());

    var result = await handler.Handle(new CancelPurchaseCommand(purchase.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Cancelled");
    outbox.Events.OfType<PurchaseCancelledEventV1>().Should().ContainSingle();
  }

  private static Purchase CreatePurchase(IReadOnlyCollection<PurchaseLine> lines)
  {
    var clock = new FixedClock();
    return Purchase.Create(
      new PurchaseCreationData(
        Guid.NewGuid(),
        new BusinessId(Guid.NewGuid()),
        new BranchId(Guid.NewGuid()),
        Guid.NewGuid(),
        Guid.NewGuid(),
        null,
        clock.UtcNow,
        null,
        clock.UtcNow),
      lines);
  }

  private static CreatePurchaseHandler CreatePurchaseHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    IOutboxWriter outbox)
    => new(
      new PurchaseHandlerContext(
        new EfPurchaseRepository(dbContext),
        new EfSupplierRepository(dbContext),
        new EfProductPurchaseReader(dbContext),
        currentUser,
        outbox,
        new FixedClock(),
        new EfUnitOfWork(dbContext)),
      CreateReceiptProcessor(dbContext, outbox),
      new FixedCorrelationIdProvider());

  private static PurchaseReceiptProcessor CreateReceiptProcessor(
    AppDbContext dbContext,
    IOutboxWriter outbox)
    => new(
      new EfInventoryRepository(dbContext),
      new EfProductPurchaseReader(dbContext),
      outbox,
      new FixedClock());

  private static Product Product(Guid productId, BusinessId businessId, decimal costPrice)
    => new(
      new ProductCreationContext(productId, businessId, new FixedClock().UtcNow),
      new ProductIdentity(ProductType.Simple, "Cafe", null, null, null, UnitOfMeasure.Unit),
      new ProductCodes($"SKU-{productId:N}"[..16], null, null, null),
      new ProductPricing(100, costPrice, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, 1, null, null, false),
      new ProductOptions(true, null, null, null));

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow { get; } =
      new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);
  }

  private sealed class FixedCorrelationIdProvider : ICorrelationIdProvider
  {
    public string CorrelationId { get; } = Guid.NewGuid().ToString("D");
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

  private sealed record TestCurrentUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }

    public Guid? BusinessId { get; init; }

    public Guid? BranchId { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsAuthenticated { get; init; }

    public static TestCurrentUser Create()
      => new()
      {
        UserId = Guid.NewGuid(),
        BusinessId = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        Roles = ["Admin"],
        IsAuthenticated = true
      };
  }
}

#pragma warning restore CA1707
