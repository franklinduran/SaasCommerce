using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Worker.Consumers;

namespace SaasCommerce.Worker.Tests;

public sealed class TechnicalPingConsumerTests
{
  [Fact]
  public async Task TechnicalPingConsumerShouldConsumePublishedEvent()
  {
    var inboxStore = Substitute.For<IInboxStore>();
    var clock = Substitute.For<IClock>();
    var message = new TechnicalPingIntegrationEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      DateTimeOffset.UtcNow,
      "harness-ping");
    await using var provider = new ServiceCollection()
      .AddLogging()
      .AddSingleton(inboxStore)
      .AddSingleton(clock)
      .AddMassTransitTestHarness(configurator =>
      {
        configurator.AddConsumer<TechnicalPingConsumer>();
      })
      .BuildServiceProvider(true);
    var harness = provider.GetRequiredService<ITestHarness>();

    clock.UtcNow.Returns(DateTimeOffset.UtcNow);
    inboxStore
      .HasProcessedAsync(message.EventId, nameof(TechnicalPingConsumer), Arg.Any<CancellationToken>())
      .Returns(false);
    inboxStore
      .MarkProcessedAsync(
        message.EventId,
        nameof(TechnicalPingConsumer),
        message.BusinessId,
        message.CorrelationId,
        Arg.Any<DateTimeOffset>(),
        Arg.Any<CancellationToken>())
      .Returns(Task.CompletedTask);

    await harness.Start();

    try
    {
      await harness.Bus.Publish(message);

      (await harness.Consumed.Any<TechnicalPingIntegrationEventV1>()).Should().BeTrue();
      (await harness
        .GetConsumerHarness<TechnicalPingConsumer>()
        .Consumed
        .Any<TechnicalPingIntegrationEventV1>()).Should().BeTrue();
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleCreatedConsumerShouldUseIdempotentLoggingBase()
  {
    var inboxStore = Substitute.For<IInboxStore>();
    var clock = Substitute.For<IClock>();
    var message = new SaleCreatedIntegrationEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      150.25m,
      DateTimeOffset.UtcNow);
    await using var provider = new ServiceCollection()
      .AddLogging()
      .AddSingleton(inboxStore)
      .AddSingleton(clock)
      .AddMassTransitTestHarness(configurator =>
      {
        configurator.AddConsumer<SaleCreatedConsumer>();
      })
      .BuildServiceProvider(true);
    var harness = provider.GetRequiredService<ITestHarness>();

    clock.UtcNow.Returns(DateTimeOffset.UtcNow);
    inboxStore
      .HasProcessedAsync(message.EventId, nameof(SaleCreatedConsumer), Arg.Any<CancellationToken>())
      .Returns(false);
    inboxStore
      .MarkProcessedAsync(
        message.EventId,
        nameof(SaleCreatedConsumer),
        message.BusinessId,
        message.CorrelationId,
        Arg.Any<DateTimeOffset>(),
        Arg.Any<CancellationToken>())
      .Returns(Task.CompletedTask);

    await harness.Start();

    try
    {
      await harness.Bus.Publish(message);

      (await harness.Consumed.Any<SaleCreatedIntegrationEventV1>()).Should().BeTrue();
      (await harness
        .GetConsumerHarness<SaleCreatedConsumer>()
        .Consumed
        .Any<SaleCreatedIntegrationEventV1>()).Should().BeTrue();
      await inboxStore.Received(1).MarkProcessedAsync(
        message.EventId,
        nameof(SaleCreatedConsumer),
        message.BusinessId,
        message.CorrelationId,
        Arg.Any<DateTimeOffset>(),
        Arg.Any<CancellationToken>());
    }
    finally
    {
      await harness.Stop();
    }
  }
}
