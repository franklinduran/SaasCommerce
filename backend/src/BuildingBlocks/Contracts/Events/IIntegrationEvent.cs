namespace SaasCommerce.BuildingBlocks.Contracts.Events;

public interface IIntegrationEvent
{
  Guid EventId { get; }

  Guid CorrelationId { get; }

  Guid BusinessId { get; }

  DateTimeOffset OccurredAt { get; }

  int Version { get; }
}
