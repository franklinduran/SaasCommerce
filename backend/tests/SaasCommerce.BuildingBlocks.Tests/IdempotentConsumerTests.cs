using FluentAssertions;
using MassTransit;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events.V1;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class IdempotentConsumerTests
{
  [Fact]
  public async Task ConsumeShouldSkipAlreadyProcessedMessages()
  {
    var message = CreateMessage();
    var inboxStore = Substitute.For<IInboxStore>();
    var clock = Substitute.For<IClock>();
    var context = CreateContext(message);
    var consumer = new TestIdempotentConsumer(inboxStore, clock);

    inboxStore
      .HasProcessedAsync(message.EventId, context.CancellationToken)
      .Returns(true);

    await consumer.Consume(context);

    consumer.ConsumeCount.Should().Be(0);
  }

  [Fact]
  public async Task ConsumeShouldMarkNewMessagesAsProcessed()
  {
    var message = CreateMessage();
    var processedAt = new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);
    var inboxStore = Substitute.For<IInboxStore>();
    var clock = Substitute.For<IClock>();
    var context = CreateContext(message);
    var consumer = new TestIdempotentConsumer(inboxStore, clock);

    clock.UtcNow.Returns(processedAt);
    inboxStore
      .HasProcessedAsync(message.EventId, context.CancellationToken)
      .Returns(false);

    await consumer.Consume(context);

    consumer.ConsumeCount.Should().Be(1);
    await inboxStore.Received(1).MarkProcessedAsync(
      message.EventId,
      message.BusinessId,
      typeof(TechnicalPingIntegrationEventV1).FullName!,
      processedAt,
      context.CancellationToken);
  }

  [Fact]
  public async Task ConsumeShouldNotMarkMessageAsProcessedWhenHandlerFails()
  {
    var message = CreateMessage();
    var inboxStore = Substitute.For<IInboxStore>();
    var clock = Substitute.For<IClock>();
    var context = CreateContext(message);
    var consumer = new FailingIdempotentConsumer(inboxStore, clock);

    inboxStore
      .HasProcessedAsync(message.EventId, context.CancellationToken)
      .Returns(false);

    var act = () => consumer.Consume(context);

    await act.Should().ThrowAsync<InvalidOperationException>();
    await inboxStore.DidNotReceive().MarkProcessedAsync(
      Arg.Any<Guid>(),
      Arg.Any<Guid>(),
      Arg.Any<string>(),
      Arg.Any<DateTimeOffset>(),
      Arg.Any<CancellationToken>());
  }

  private static TechnicalPingIntegrationEventV1 CreateMessage()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      DateTimeOffset.UtcNow);

  private static ConsumeContext<TechnicalPingIntegrationEventV1> CreateContext(
    TechnicalPingIntegrationEventV1 message)
  {
    var context = Substitute.For<ConsumeContext<TechnicalPingIntegrationEventV1>>();
    context.Message.Returns(message);
    context.CancellationToken.Returns(CancellationToken.None);

    return context;
  }

  private sealed class TestIdempotentConsumer(
    IInboxStore inboxStore,
    IClock clock) : IdempotentConsumer<TechnicalPingIntegrationEventV1>(inboxStore, clock)
  {
    public int ConsumeCount { get; private set; }

    protected override Task ConsumeMessageAsync(
      ConsumeContext<TechnicalPingIntegrationEventV1> context)
    {
      ConsumeCount++;

      return Task.CompletedTask;
    }
  }

  private sealed class FailingIdempotentConsumer(
    IInboxStore inboxStore,
    IClock clock) : IdempotentConsumer<TechnicalPingIntegrationEventV1>(inboxStore, clock)
  {
    protected override Task ConsumeMessageAsync(
      ConsumeContext<TechnicalPingIntegrationEventV1> context)
      => throw new InvalidOperationException("Handler failed.");
  }
}
