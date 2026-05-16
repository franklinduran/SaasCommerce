using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Catalog.Contracts.Events.V1;

public sealed record ProductActivatedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid ProductId,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
