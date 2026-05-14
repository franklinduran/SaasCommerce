using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Messaging;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using FluentAssertions;
using MassTransit;
using NSubstitute;

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
      typeof(TechnicalPing).FullName!,
      processedAt,
      context.CancellationToken);
  }

  private static TechnicalPing CreateMessage()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      DateTimeOffset.UtcNow);

  private static ConsumeContext<TechnicalPing> CreateContext(TechnicalPing message)
  {
    var context = Substitute.For<ConsumeContext<TechnicalPing>>();
    context.Message.Returns(message);
    context.CancellationToken.Returns(CancellationToken.None);

    return context;
  }

  private sealed class TestIdempotentConsumer(
    IInboxStore inboxStore,
    IClock clock) : IdempotentConsumer<TechnicalPing>(inboxStore, clock)
  {
    public int ConsumeCount { get; private set; }

    protected override Task ConsumeMessageAsync(ConsumeContext<TechnicalPing> context)
    {
      ConsumeCount++;

      return Task.CompletedTask;
    }
  }
}
