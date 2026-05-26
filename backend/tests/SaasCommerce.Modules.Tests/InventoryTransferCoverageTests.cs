using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Application.Transfers;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Modules.Tests;

public sealed class InventoryTransferCoverageTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 25, 15, 0, 0, TimeSpan.Zero);

  [Fact]
  public void InventoryTransfer_ShouldValidateDraft()
  {
    var draft = Draft();

    var transfer = new InventoryTransfer(draft);

    transfer.Status.Should().Be(InventoryTransferStatus.Pending);
    transfer.Note.Should().Be("Mover urgente");
    transfer.Items.Should().ContainSingle();

    FluentActions.Invoking(() => new InventoryTransfer(Draft(
        sourceBranchId: draft.SourceBranchId.Value,
        targetBranchId: draft.SourceBranchId.Value)))
      .Should().Throw<InvalidOperationException>()
      .WithMessage("*different*");
    FluentActions.Invoking(() => new InventoryTransfer(Draft(items: [])))
      .Should().Throw<InvalidOperationException>()
      .WithMessage("*at least one item*");
    FluentActions.Invoking(() => new InventoryTransferItem(Guid.NewGuid(), Guid.NewGuid(), 0))
      .Should().Throw<ArgumentOutOfRangeException>()
      .WithMessage("*greater than zero*");
  }

  [Fact]
  public void InventoryTransfer_ShouldCompleteFailAndCancelOnlyFromAllowedStatuses()
  {
    var completed = new InventoryTransfer(Draft());
    completed.Complete(Now.AddMinutes(1));
    completed.Status.Should().Be(InventoryTransferStatus.Completed);
    FluentActions.Invoking(() => completed.Complete(Now)).Should().Throw<InvalidOperationException>();
    FluentActions.Invoking(() => completed.Fail("bad", Now)).Should().Throw<InvalidOperationException>();
    FluentActions.Invoking(() => completed.Cancel(Now)).Should().Throw<InvalidOperationException>();

    var failed = new InventoryTransfer(Draft());
    failed.Fail("No stock", Now.AddMinutes(2));
    failed.Status.Should().Be(InventoryTransferStatus.Failed);
    failed.FailureReason.Should().Be("No stock");
    failed.Cancel(Now.AddMinutes(3));
    failed.Status.Should().Be(InventoryTransferStatus.Cancelled);

    var cancelled = new InventoryTransfer(Draft());
    cancelled.Cancel(Now);
    FluentActions.Invoking(() => cancelled.Cancel(Now)).Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public async Task CreateInventoryTransfer_ShouldPersistAndPublish_WhenValid()
  {
    var scenario = TransferScenario.Create();
    var handler = scenario.CreateHandler();

    var result = await handler.Handle(new CreateInventoryTransferCommand(
      scenario.SourceBranchId,
      scenario.TargetBranchId,
      [new CreateInventoryTransferItemCommand(scenario.ProductId, 3)],
      "  nota  "));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle(item => item.ProductId == scenario.ProductId && item.Quantity == 3);
    scenario.Transfers.Items.Should().ContainSingle();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InventoryTransferRequestedEventV1);
    scenario.UnitOfWork.SaveCount.Should().Be(1);
  }

  [Fact]
  public async Task CreateInventoryTransfer_ShouldReturnValidationFailures()
  {
    var scenario = TransferScenario.Create();
    var handler = scenario.CreateHandler();

    scenario.CurrentUser.BusinessIdValue = null;
    (await handler.Handle(ValidCreateCommand(scenario))).Error.Should().Be(TransferErrors.UserContextRequired);

    scenario = TransferScenario.Create();
    scenario.LimitChecker.Allowed = false;
    (await scenario.CreateHandler().Handle(ValidCreateCommand(scenario))).Error.Code.Should().Be("subscription.feature_not_available");

    scenario = TransferScenario.Create();
    (await scenario.CreateHandler().Handle(ValidCreateCommand(scenario) with { TargetBranchId = scenario.SourceBranchId }))
      .Error.Should().Be(TransferErrors.SameBranch);
    (await scenario.CreateHandler().Handle(ValidCreateCommand(scenario) with { Items = [] }))
      .Error.Should().Be(TransferErrors.NoItems);
    (await scenario.CreateHandler().Handle(ValidCreateCommand(scenario) with { Items = [new CreateInventoryTransferItemCommand(scenario.ProductId, -1)] }))
      .Error.Should().Be(TransferErrors.InvalidQuantity);
  }

  [Fact]
  public async Task CancelInventoryTransfer_ShouldCancelPendingTransfer()
  {
    var scenario = TransferScenario.Create();
    var transfer = scenario.PendingTransfer();
    scenario.Transfers.Items.Add(transfer);
    var handler = new CancelInventoryTransferHandler(
      scenario.Transfers,
      scenario.CurrentUser,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await handler.Handle(new CancelInventoryTransferCommand(transfer.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be(nameof(InventoryTransferStatus.Cancelled));
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InventoryTransferCancelledEventV1);
    scenario.UnitOfWork.SaveCount.Should().Be(1);
  }

  [Fact]
  public async Task CancelInventoryTransfer_ShouldReturnFailures()
  {
    var scenario = TransferScenario.Create();
    var handler = new CancelInventoryTransferHandler(
      scenario.Transfers,
      scenario.CurrentUser,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    scenario.CurrentUser.BusinessIdValue = null;
    (await handler.Handle(new CancelInventoryTransferCommand(Guid.NewGuid()))).Error.Should().Be(TransferErrors.UserContextRequired);

    scenario.CurrentUser.BusinessIdValue = scenario.BusinessId;
    (await handler.Handle(new CancelInventoryTransferCommand(Guid.NewGuid()))).Error.Should().Be(TransferErrors.NotFound);

    var completed = scenario.PendingTransfer();
    completed.Complete(Now);
    scenario.Transfers.Items.Add(completed);
    (await handler.Handle(new CancelInventoryTransferCommand(completed.Id))).Error.Should().Be(TransferErrors.CannotCancel);
  }

  [Fact]
  public async Task ProcessInventoryTransfer_ShouldMoveStockAndPublishCompleted()
  {
    var scenario = TransferScenario.Create();
    var transfer = scenario.PendingTransfer();
    scenario.Transfers.Items.Add(transfer);
    var sourceStock = new StockItem(Guid.NewGuid(), new BusinessId(scenario.BusinessId), new BranchId(scenario.SourceBranchId), scenario.ProductId, Now);
    sourceStock.ApplyAdjustment(10, InventoryMovementReason.ManualAdjustment, scenario.UserId, false, Now);
    scenario.Inventory.Stock.Add(sourceStock);
    var useCase = scenario.CreateProcessUseCase();

    var result = await useCase.ExecuteAsync(scenario.EventFor(transfer));

    result.IsSuccess.Should().BeTrue();
    transfer.Status.Should().Be(InventoryTransferStatus.Completed);
    scenario.Inventory.Stock.Should().HaveCount(2);
    scenario.Inventory.Movements.Should().HaveCount(2);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InventoryTransferCompletedEventV1);
  }

  [Fact]
  public async Task ProcessInventoryTransfer_ShouldFail_WhenSourceStockMissingOrInsufficient()
  {
    var scenario = TransferScenario.Create();
    var missing = scenario.PendingTransfer();
    scenario.Transfers.Items.Add(missing);

    await scenario.CreateProcessUseCase().ExecuteAsync(scenario.EventFor(missing));

    missing.Status.Should().Be(InventoryTransferStatus.Failed);
    missing.FailureReason.Should().Contain("no stock record");

    scenario = TransferScenario.Create();
    var insufficient = scenario.PendingTransfer(quantity: 20);
    scenario.Transfers.Items.Add(insufficient);
    var sourceStock = new StockItem(Guid.NewGuid(), new BusinessId(scenario.BusinessId), new BranchId(scenario.SourceBranchId), scenario.ProductId, Now);
    sourceStock.ApplyAdjustment(5, InventoryMovementReason.ManualAdjustment, scenario.UserId, false, Now);
    scenario.Inventory.Stock.Add(sourceStock);

    await scenario.CreateProcessUseCase().ExecuteAsync(scenario.EventFor(insufficient));

    insufficient.Status.Should().Be(InventoryTransferStatus.Failed);
    insufficient.FailureReason.Should().Contain("Insufficient stock");
    scenario.Outbox.Events.Should().Contain(@event => @event is InventoryTransferFailedEventV1);
  }

  [Fact]
  public async Task ProcessInventoryTransfer_ShouldIgnoreMissingOrNonPendingTransfer()
  {
    var scenario = TransferScenario.Create();
    var useCase = scenario.CreateProcessUseCase();

    (await useCase.ExecuteAsync(scenario.EventFor(scenario.PendingTransfer()))).IsSuccess.Should().BeTrue();

    var completed = scenario.PendingTransfer();
    completed.Complete(Now);
    scenario.Transfers.Items.Add(completed);

    (await useCase.ExecuteAsync(scenario.EventFor(completed))).IsSuccess.Should().BeTrue();
    scenario.UnitOfWork.SaveCount.Should().Be(0);
  }

  private static InventoryTransferDraft Draft(
    Guid? sourceBranchId = null,
    Guid? targetBranchId = null,
    IReadOnlyCollection<InventoryTransferItem>? items = null)
  {
    var transferId = Guid.NewGuid();
    return new InventoryTransferDraft
    {
      Id = transferId,
      BusinessId = new BusinessId(Guid.NewGuid()),
      SourceBranchId = new BranchId(sourceBranchId ?? Guid.NewGuid()),
      TargetBranchId = new BranchId(targetBranchId ?? Guid.NewGuid()),
      CreatedByUserId = Guid.NewGuid(),
      Items = items ?? [new InventoryTransferItem(transferId, Guid.NewGuid(), 2)],
      CreatedAt = Now,
      Note = "  Mover urgente  "
    };
  }

  private static CreateInventoryTransferCommand ValidCreateCommand(TransferScenario scenario)
    => new(
      scenario.SourceBranchId,
      scenario.TargetBranchId,
      [new CreateInventoryTransferItemCommand(scenario.ProductId, 3)],
      "nota");

  private sealed class TransferScenario
  {
    public Guid BusinessId { get; } = Guid.NewGuid();
    public Guid UserId { get; } = Guid.NewGuid();
    public Guid SourceBranchId { get; } = Guid.NewGuid();
    public Guid TargetBranchId { get; } = Guid.NewGuid();
    public Guid ProductId { get; } = Guid.NewGuid();
    public TestTransferRepository Transfers { get; } = new();
    public TestInventoryRepository Inventory { get; } = new();
    public TestOutbox Outbox { get; } = new();
    public TestClock Clock { get; } = new();
    public TestUnitOfWork UnitOfWork { get; } = new();
    public TestLimitChecker LimitChecker { get; } = new();
    public TestCurrentUser CurrentUser { get; private set; } = null!;

    public static TransferScenario Create()
    {
      var scenario = new TransferScenario();
      scenario.CurrentUser = new TestCurrentUser(scenario.BusinessId, scenario.UserId);
      return scenario;
    }

    public CreateInventoryTransferHandler CreateHandler()
      => new(Transfers, CurrentUser, Outbox, Clock, UnitOfWork, LimitChecker);

    public ProcessInventoryTransferUseCase CreateProcessUseCase()
      => new(Transfers, Inventory, Outbox, Clock, UnitOfWork);

    public InventoryTransfer PendingTransfer(decimal quantity = 3)
    {
      var transferId = Guid.NewGuid();
      return new InventoryTransfer(new InventoryTransferDraft
      {
        Id = transferId,
        BusinessId = new BusinessId(BusinessId),
        SourceBranchId = new BranchId(SourceBranchId),
        TargetBranchId = new BranchId(TargetBranchId),
        CreatedByUserId = UserId,
        Items = [new InventoryTransferItem(transferId, ProductId, quantity)],
        CreatedAt = Now,
        Note = null
      });
    }

    public InventoryTransferRequestedEventV1 EventFor(InventoryTransfer transfer)
      => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        BusinessId,
        transfer.Id,
        SourceBranchId,
        TargetBranchId,
        UserId,
        Now);
  }

  private sealed class TestTransferRepository : IInventoryTransferRepository
  {
    public List<InventoryTransfer> Items { get; } = [];

    public Task AddAsync(InventoryTransfer transfer, CancellationToken cancellationToken = default)
    {
      Items.Add(transfer);
      return Task.CompletedTask;
    }

    public Task<InventoryTransfer?> GetByIdAsync(BusinessId businessId, Guid transferId, CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(item => item.BusinessId == businessId && item.Id == transferId));

    public Task<(IReadOnlyCollection<InventoryTransfer> Items, int Total)> ListAsync(
      BusinessId businessId,
      Guid? sourceBranchId,
      Guid? targetBranchId,
      InventoryTransferStatus? status,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
      => Task.FromResult(((IReadOnlyCollection<InventoryTransfer>)Items, Items.Count));
  }

  private sealed class TestInventoryRepository : IInventoryRepository
  {
    public List<StockItem> Stock { get; } = [];
    public List<InventoryMovement> Movements { get; } = [];

    public Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
      Movements.Add(movement);
      return Task.CompletedTask;
    }

    public Task AddStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default)
    {
      Stock.Add(stockItem);
      return Task.CompletedTask;
    }

    public Task<int> CountMovementsAsync(BusinessId businessId, BranchId branchId, InventoryMovementSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<int> CountStockAsync(BusinessId businessId, BranchId? branchId, StockSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(Stock.Count);

    public Task<StockItem?> GetStockItemAsync(BusinessId businessId, BranchId branchId, Guid productId, CancellationToken cancellationToken = default)
      => Task.FromResult(Stock.SingleOrDefault(item => item.BusinessId == businessId && item.BranchId == branchId && item.ProductId == productId));

    public Task<bool> HasPurchaseMovementAsync(BusinessId businessId, BranchId branchId, Guid purchaseId, CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<bool> HasSaleMovementAsync(BusinessId businessId, BranchId branchId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<IReadOnlyCollection<InventoryMovement>> ListMovementsAsync(BusinessId businessId, BranchId branchId, InventoryMovementSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<InventoryMovement>>(Movements);

    public Task<IReadOnlyCollection<StockItem>> ListStockAsync(BusinessId businessId, BranchId? branchId, StockSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<StockItem>>(Stock);
  }

  private sealed class TestOutbox : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class TestCurrentUser(Guid businessId, Guid userId) : ICurrentUserService
  {
    public Guid? BusinessIdValue { get; set; } = businessId;
    public Guid? UserIdValue { get; set; } = userId;
    public Guid? UserId => UserIdValue;
    public Guid? BusinessId => BusinessIdValue;
    public Guid? BranchId => null;
    public IReadOnlyCollection<string> Roles => ["Admin"];
    public bool IsAuthenticated => true;
  }

  private sealed class TestClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class TestUnitOfWork : IUnitOfWork
  {
    public int SaveCount { get; private set; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveCount++;
      return Task.FromResult(1);
    }
  }

  private sealed class TestLimitChecker : ISubscriptionLimitChecker
  {
    public bool Allowed { get; set; } = true;

    public Task<SubscriptionLimitCheckResult> CanUseInventoryTransfersAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(new SubscriptionLimitCheckResult(Allowed, Allowed ? "OK" : "NO_FEATURE", Allowed ? "Allowed" : "Not available", 0, 1));

    public Task<SubscriptionLimitCheckResult> CanCreateBranchAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanCreateUserAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanCreateProductAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanCreateSaleAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanUseAdvancedReportsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanUseAuditLogsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
  }
}

#pragma warning restore CA1707
