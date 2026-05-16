using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

public sealed class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
  public Task PublishAsync<TMessage>(
    TMessage message,
    CancellationToken cancellationToken = default)
    where TMessage : class
  {
    ArgumentNullException.ThrowIfNull(message);

    return publishEndpoint.Publish(message, cancellationToken);
  }
}
