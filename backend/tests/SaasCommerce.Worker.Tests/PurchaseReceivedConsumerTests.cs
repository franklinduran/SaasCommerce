#pragma warning disable CA1707

using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.SharedKernel;
using SaasCommerce.Worker.Consumers;

namespace SaasCommerce.Worker.Tests;

public sealed class PurchaseReceivedConsumerTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task PurchaseReceivedConsumer_ShouldIncreaseInventory_WhenEventIsValid()
  {
    var message = PurchaseReceived();
    var useCase = Substitute.For<IProcessPurchaseReceivedEventUseCase>();
    var consumer = new PurchaseReceivedConsumer(
      NullLogger<PurchaseReceivedConsumer>.Instance,
      InboxStore(message.EventId, nameof(PurchaseReceivedConsumer), alreadyProcessed: false),
      Clock(),
      useCase);

    useCase.ExecuteAsync(message, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result.Success()));

    await consumer.Consume(Context(message));

    await useCase.Received(1).ExecuteAsync(message, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task PurchaseReceivedConsumer_ShouldBeIdempotent_WhenMessageIsDuplicated()
  {
    var message = PurchaseReceived();
    var useCase = Substitute.For<IProcessPurchaseReceivedEventUseCase>();
    var consumer = new PurchaseReceivedConsumer(
      NullLogger<PurchaseReceivedConsumer>.Instance,
      InboxStore(message.EventId, nameof(PurchaseReceivedConsumer), alreadyProcessed: true),
      Clock(),
      useCase);

    await consumer.Consume(Context(message));

    await useCase.DidNotReceive().ExecuteAsync(
      Arg.Any<PurchaseReceivedEventV1>(),
      Arg.Any<CancellationToken>());
  }

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
