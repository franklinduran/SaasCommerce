using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record InventoryTransferFailedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid TransferId,
  Guid SourceBranchId,
  Guid TargetBranchId,
  string Reason,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
