using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record InventoryAdjustedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid ProductId,
  decimal Quantity,
  decimal NewStock,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
