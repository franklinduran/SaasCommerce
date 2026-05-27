using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Worker.Consumers;

namespace SaasCommerce.Worker.Tests;

public sealed class CashRegisterConsumerTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 27, 9, 0, 0, TimeSpan.Zero);

  [Fact]
  public void CashRegisterClosedConsumerShouldBeIdempotent()
  {
    typeof(CashRegisterClosedConsumer)
      .BaseType.Should().NotBeNull()
      .And.BeAssignableTo<IdempotentConsumer<CashRegisterClosedEventV1>>();
  }

  [Fact]
  public void CashRegisterNotificationConsumerShouldBeIdempotent()
  {
    typeof(CashRegisterNotificationConsumer)
      .BaseType.Should().NotBeNull()
      .And.BeAssignableTo<IdempotentConsumer<CashRegisterDifferenceDetectedEventV1>>();
  }

  [Fact]
  public async Task CashRegisterClosedConsumerShouldNotifyBranchGroup()
  {
    var message = CashRegisterClosed();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CashRegisterClosedConsumer(
      InboxStore(message.EventId, nameof(CashRegisterClosedConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CashRegisterClosedConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBranchAsync(
      message.BranchId,
      "cashRegister.closed",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CashRegisterClosedConsumerShouldSkipDuplicateMessage()
  {
    var message = CashRegisterClosed();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CashRegisterClosedConsumer(
      InboxStore(message.EventId, nameof(CashRegisterClosedConsumer), alreadyProcessed: true),
      Clock(),
      realtime,
      NullLogger<CashRegisterClosedConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.DidNotReceive().NotifyBranchAsync(
      Arg.Any<Guid>(),
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CashRegisterNotificationConsumerShouldNotifyBranchGroup()
  {
    var message = new CashRegisterDifferenceDetectedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      -75,
      "Shortage",
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CashRegisterNotificationConsumer(
      InboxStore(message.EventId, nameof(CashRegisterNotificationConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CashRegisterNotificationConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBranchAsync(
      message.BranchId,
      "cashRegister.differenceDetected",
      message,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CashRegisterNotificationConsumerShouldSkipDuplicateMessage()
  {
    var message = new CashRegisterDifferenceDetectedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      120,
      "Surplus",
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CashRegisterNotificationConsumer(
      InboxStore(message.EventId, nameof(CashRegisterNotificationConsumer), alreadyProcessed: true),
      Clock(),
      realtime,
      NullLogger<CashRegisterNotificationConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.DidNotReceive().NotifyBranchAsync(
      Arg.Any<Guid>(),
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

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
      100,
      50,
      25,
      0,
      20,
      10,
      1510,
      1510,
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
}
