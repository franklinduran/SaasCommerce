using FluentAssertions;
using MassTransit;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Contracts.Events.V1;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class MassTransitEventBusTests
{
  [Fact]
  public async Task PublishAsyncShouldDelegateGenericMessageToMassTransit()
  {
    var publishEndpoint = Substitute.For<IPublishEndpoint>();
    var message = CreateMessage();
    var cancellationToken = new CancellationTokenSource().Token;
    var eventBus = new MassTransitEventBus(publishEndpoint);

    publishEndpoint
      .Publish(message, cancellationToken)
      .Returns(Task.CompletedTask);

    await eventBus.PublishAsync(message, cancellationToken);

    await publishEndpoint.Received(1).Publish(message, cancellationToken);
  }

  [Fact]
  public async Task PublishAsyncShouldDelegateDynamicMessageToMassTransit()
  {
    var publishEndpoint = Substitute.For<IPublishEndpoint>();
    var message = CreateMessage();
    var cancellationToken = new CancellationTokenSource().Token;
    var eventBus = new MassTransitEventBus(publishEndpoint);

#pragma warning disable CA2263
    publishEndpoint
      .Publish(message, typeof(TechnicalPingIntegrationEventV1), cancellationToken)
      .Returns(Task.CompletedTask);

    await eventBus.PublishAsync(message, typeof(TechnicalPingIntegrationEventV1), cancellationToken);

    await publishEndpoint.Received(1).Publish(
      message,
      typeof(TechnicalPingIntegrationEventV1),
      cancellationToken);
#pragma warning restore CA2263
  }

  [Fact]
  public async Task PublishAsyncShouldRejectNullInputs()
  {
    var publishEndpoint = Substitute.For<IPublishEndpoint>();
    var eventBus = new MassTransitEventBus(publishEndpoint);
    var message = CreateMessage();

    var nullGenericMessage = () => eventBus.PublishAsync<TechnicalPingIntegrationEventV1>(null!);
    var nullDynamicMessage = () => eventBus.PublishAsync(null!, typeof(TechnicalPingIntegrationEventV1));
    var nullMessageType = () => eventBus.PublishAsync(message, null!);

    await nullGenericMessage.Should().ThrowAsync<ArgumentNullException>();
    await nullDynamicMessage.Should().ThrowAsync<ArgumentNullException>();
    await nullMessageType.Should().ThrowAsync<ArgumentNullException>();
  }

  private static TechnicalPingIntegrationEventV1 CreateMessage()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      DateTimeOffset.UtcNow,
      "bus-test");
}
