using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record InventoryDeductionRequestedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid SaleId,
  Guid BranchId,
  Guid UserId,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
