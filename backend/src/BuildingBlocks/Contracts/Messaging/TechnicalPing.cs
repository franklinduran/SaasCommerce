using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.BuildingBlocks.Contracts.Messaging;

public sealed record TechnicalPing(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
