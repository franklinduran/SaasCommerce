namespace SaasCommerce.BuildingBlocks.Contracts.Events.V1;

public sealed record TechnicalPingIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  DateTimeOffset OccurredAt,
  string Message,
  int Version = 1) : IIntegrationEvent;
