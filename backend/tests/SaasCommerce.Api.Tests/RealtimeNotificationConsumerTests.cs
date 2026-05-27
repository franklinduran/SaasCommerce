#pragma warning disable CA1707

using MassTransit;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaasCommerce.Api.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Customers.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Api.Tests;

public sealed class RealtimeNotificationConsumerTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task SaleCompletedRealtimeConsumer_ShouldNotifyCompletedStatus()
  {
    var message = SaleCompleted();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new SaleCompletedRealtimeConsumer(
      InboxStore(message.EventId, nameof(SaleCompletedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<SaleCompletedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "sale.statusChanged",
      Arg.Is<SaleStatusChangedNotificationV1>(notification =>
        notification.SaleId == message.SaleId &&
        notification.Status == "Completed"),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task SaleFailedRealtimeConsumer_ShouldNotifyFailureReason()
  {
    var message = new SaleFailedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "Insufficient stock.",
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new SaleFailedRealtimeConsumer(
      InboxStore(message.EventId, nameof(SaleFailedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<SaleFailedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "sale.statusChanged",
      Arg.Is<SaleStatusChangedNotificationV1>(notification =>
        notification.SaleId == message.SaleId &&
        notification.Status == "Failed" &&
        notification.Reason == message.Reason),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InventoryDeductedRealtimeConsumer_ShouldNotifyStockChanged()
  {
    var message = InventoryDeducted();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InventoryDeductedRealtimeConsumer(
      InboxStore(message.EventId, nameof(InventoryDeductedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<InventoryDeductedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.stockChanged",
      Arg.Is<InventoryStockChangedNotificationV1>(notification =>
        notification.SaleId == message.SaleId &&
        notification.Reason == "SaleDeduction"),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InventoryAdjustedRealtimeConsumer_ShouldNotifyAdjustedAndStockChanged()
  {
    var message = new InventoryAdjustedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      3,
      7,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InventoryAdjustedRealtimeConsumer(
      InboxStore(message.EventId, nameof(InventoryAdjustedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<InventoryAdjustedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.adjusted",
      message,
      Arg.Any<CancellationToken>());
    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.stockChanged",
      Arg.Is<InventoryStockChangedNotificationV1>(notification =>
        notification.ProductId == message.ProductId &&
        notification.Reason == "ManualAdjustment"),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task LowStockDetectedRealtimeConsumer_ShouldNotifyLowStock()
  {
    var message = new LowStockDetectedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "Salami Induveca 1 lb",
      1,
      2,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new LowStockDetectedRealtimeConsumer(
      InboxStore(message.EventId, nameof(LowStockDetectedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<LowStockDetectedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.lowStockDetected",
      Arg.Is<LowStockDetectedNotificationV1>(notification =>
        notification.ProductId == message.ProductId &&
        notification.ProductName == message.ProductName &&
        notification.CurrentStock == message.CurrentStock),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task PurchaseReceivedRealtimeConsumer_ShouldNotifyBusinessAndBranch()
  {
    var message = PurchaseReceived();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new PurchaseReceivedRealtimeConsumer(
      InboxStore(message.EventId, nameof(PurchaseReceivedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<PurchaseReceivedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "purchase.received",
      message,
      Arg.Any<CancellationToken>());
    await realtime.Received(1).NotifyBranchAsync(
      message.BranchId,
      "purchase.received",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task PurchaseInventoryUpdatedRealtimeConsumer_ShouldNotifyBusinessCompleted()
  {
    var message = new PurchaseInventoryUpdatedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      240,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new PurchaseInventoryUpdatedRealtimeConsumer(
      InboxStore(message.EventId, nameof(PurchaseInventoryUpdatedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<PurchaseInventoryUpdatedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "purchase.completed",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task PurchaseFailedRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = new PurchaseFailedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "Product missing.",
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new PurchaseFailedRealtimeConsumer(
      InboxStore(message.EventId, nameof(PurchaseFailedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<PurchaseFailedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "purchase.failed",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InventoryIncreasedRealtimeConsumer_ShouldPublishInventoryUpdatedEvent()
  {
    var message = new InventoryIncreasedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      10,
      15,
      5,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InventoryIncreasedRealtimeConsumer(
      InboxStore(message.EventId, nameof(InventoryIncreasedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<InventoryIncreasedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.updated",
      message,
      Arg.Any<CancellationToken>());
    await realtime.Received(1).NotifyBranchAsync(
      message.BranchId,
      "inventory.updated",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ProductCostUpdatedRealtimeConsumer_ShouldPublishCostUpdatedEvent()
  {
    var message = new ProductCostUpdatedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      80,
      95,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new ProductCostUpdatedRealtimeConsumer(
      InboxStore(message.EventId, nameof(ProductCostUpdatedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<ProductCostUpdatedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "product.costUpdated",
      message,
      Arg.Any<CancellationToken>());
    await realtime.Received(1).NotifyBranchAsync(
      message.BranchId,
      "product.costUpdated",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InventoryDeductedRealtimeConsumer_ShouldSkipDuplicateEvent()
  {
    var message = InventoryDeducted();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InventoryDeductedRealtimeConsumer(
      InboxStore(message.EventId, nameof(InventoryDeductedRealtimeConsumer), alreadyProcessed: true),
      Clock(),
      realtime,
      NullLogger<InventoryDeductedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.DidNotReceive().NotifyBusinessAsync(
      Arg.Any<Guid>(),
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CustomerCreditDebitedRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = new CustomerCreditDebitedEventV1(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), 150, 350, Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CustomerCreditDebitedRealtimeConsumer(
      InboxStore(message.EventId, nameof(CustomerCreditDebitedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CustomerCreditDebitedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CustomerPaymentRegisteredRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = new CustomerPaymentRegisteredEventV1(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), 75, 275, Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CustomerPaymentRegisteredRealtimeConsumer(
      InboxStore(message.EventId, nameof(CustomerPaymentRegisteredRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CustomerPaymentRegisteredRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InvoiceGeneratedRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = new InvoiceGeneratedEventV1(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 236, Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InvoiceGeneratedRealtimeConsumer(
      InboxStore(message.EventId, nameof(InvoiceGeneratedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<InvoiceGeneratedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InvoiceCancelledRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = new InvoiceCancelledEventV1(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      Guid.NewGuid(), Guid.NewGuid(), "RI-00000001", Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InvoiceCancelledRealtimeConsumer(
      InboxStore(message.EventId, nameof(InvoiceCancelledRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<InvoiceCancelledRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CashRegisterOpenedRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = CashRegisterOpened();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CashRegisterOpenedRealtimeConsumer(
      InboxStore(message.EventId, nameof(CashRegisterOpenedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CashRegisterOpenedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "cashRegister.opened",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CashRegisterMovementRegisteredRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = new CashRegisterMovementRegisteredEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "CashIn",
      250,
      "Reposicion",
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CashRegisterMovementRegisteredRealtimeConsumer(
      InboxStore(message.EventId, nameof(CashRegisterMovementRegisteredRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CashRegisterMovementRegisteredRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "cashRegister.movementRegistered",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CashRegisterClosedRealtimeConsumer_ShouldNotifyBusiness()
  {
    var message = CashRegisterClosed();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CashRegisterClosedRealtimeConsumer(
      InboxStore(message.EventId, nameof(CashRegisterClosedRealtimeConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CashRegisterClosedRealtimeConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "cashRegister.closed",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CashRegisterDifferenceNotificationConsumer_ShouldNotifyBusinessAndCreateWarning()
  {
    var message = new CashRegisterDifferenceDetectedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      -125,
      "Shortage",
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var notifications = new RecordingNotificationRepository();
    var consumer = new CashRegisterDifferenceNotificationConsumer(
      InboxStore(message.EventId, nameof(CashRegisterDifferenceNotificationConsumer), alreadyProcessed: false),
      Clock(),
      new CreateNotificationHandler(notifications, Clock()),
      realtime,
      NullLogger<CashRegisterDifferenceNotificationConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "cashRegister.differenceDetected",
      message,
      Arg.Any<CancellationToken>());
    notifications.Added.Should().ContainSingle(notification =>
      notification.Type == OperationalNotificationType.CashDifference &&
      notification.Severity == OperationalNotificationSeverity.Warning);
  }

  [Fact]
  public async Task CashRegisterDifferenceNotificationConsumer_ShouldNotCreateNotificationForSmallDifference()
  {
    var message = new CashRegisterDifferenceDetectedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      20,
      "Surplus",
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var notifications = new RecordingNotificationRepository();
    var consumer = new CashRegisterDifferenceNotificationConsumer(
      InboxStore(message.EventId, nameof(CashRegisterDifferenceNotificationConsumer), alreadyProcessed: false),
      Clock(),
      new CreateNotificationHandler(notifications, Clock()),
      realtime,
      NullLogger<CashRegisterDifferenceNotificationConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "cashRegister.differenceDetected",
      message,
      Arg.Any<CancellationToken>());
    notifications.Added.Should().BeEmpty();
  }

  private static SaleCompletedEventV1 SaleCompleted()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      200,
      Now);

  private static InventoryDeductedEventV1 InventoryDeducted()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      [new SaleItemV1(Guid.NewGuid(), 2, 100)],
      200,
      "Cash",
      Now);

  private static PurchaseReceivedEventV1 PurchaseReceived()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      [new PurchaseItemV1(Guid.NewGuid(), 2, 120, 240)],
      240,
      Now);

  private static CashRegisterOpenedEventV1 CashRegisterOpened()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      1000,
      Now);

  private static CashRegisterClosedEventV1 CashRegisterClosed()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      1000,
      500,
      200,
      150,
      100,
      50,
      25,
      10,
      1465,
      1465,
      0,
      "Balanced",
      Now);

  private static ConsumeContext<TMessage> Context<TMessage>(TMessage message)
    where TMessage : class
  {
    var context = Substitute.For<ConsumeContext<TMessage>>();
    context.Message.Returns(message);
    context.CancellationToken.Returns(CancellationToken.None);

    return context;
  }

  private static IInboxStore InboxStore(Guid eventId, string consumerName, bool alreadyProcessed)
  {
    var inboxStore = Substitute.For<IInboxStore>();
    inboxStore
      .HasProcessedAsync(eventId, consumerName, Arg.Any<CancellationToken>())
      .Returns(alreadyProcessed);
    inboxStore
      .MarkProcessedAsync(
        eventId,
        consumerName,
        Arg.Any<Guid>(),
        Arg.Any<Guid>(),
        Arg.Any<DateTimeOffset>(),
        Arg.Any<CancellationToken>())
      .Returns(Task.CompletedTask);

    return inboxStore;
  }

  private static IClock Clock()
  {
    var clock = Substitute.For<IClock>();
    clock.UtcNow.Returns(Now);
    return clock;
  }

  private sealed class RecordingNotificationRepository : IOperationalNotificationRepository
  {
    public List<OperationalNotification> Added { get; } = [];

    public Task AddAsync(OperationalNotification notification, CancellationToken cancellationToken = default)
    {
      Added.Add(notification);
      return Task.CompletedTask;
    }

    public Task<OperationalNotification?> GetByIdAsync(
      Guid id,
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult<OperationalNotification?>(null);

    public Task<(IReadOnlyList<OperationalNotification> Items, int TotalCount)> GetPagedAsync(
      BusinessId businessId,
      Guid? branchId,
      OperationalNotificationStatus? status,
      OperationalNotificationType? type,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
      => Task.FromResult<(IReadOnlyList<OperationalNotification>, int)>(([], 0));

    public Task<int> GetUnreadCountAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }
}

#pragma warning restore CA1707
