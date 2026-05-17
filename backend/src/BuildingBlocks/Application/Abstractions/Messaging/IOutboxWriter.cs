using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
  Task AddAsync<TEvent>(
    TEvent integrationEvent,
    CancellationToken cancellationToken = default)
    where TEvent : class, IIntegrationEvent;
}
