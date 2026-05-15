using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Catalog.Contracts.Events.V1;

public sealed record ProductUpdatedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid ProductId,
  string Sku,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
