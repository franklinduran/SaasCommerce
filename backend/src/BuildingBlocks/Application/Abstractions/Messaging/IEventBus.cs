namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IEventBus
{
  Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    where TMessage : class;

  Task PublishAsync(
    object message,
    Type messageType,
    CancellationToken cancellationToken = default);
}
