#pragma warning disable CA1707

using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaasCommerce.Api.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

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
}

#pragma warning restore CA1707
