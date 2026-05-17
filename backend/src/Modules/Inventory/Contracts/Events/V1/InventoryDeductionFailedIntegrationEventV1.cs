using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record InventoryDeductionFailedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid SaleId,
  Guid BranchId,
  Guid UserId,
  string Reason,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
