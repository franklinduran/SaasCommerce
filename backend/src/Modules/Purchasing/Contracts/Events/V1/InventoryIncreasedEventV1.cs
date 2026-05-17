using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Purchasing.Contracts.Events.V1;

public sealed record InventoryIncreasedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid PurchaseId,
  Guid BusinessId,
  Guid BranchId,
  Guid ProductId,
  Guid InventoryMovementId,
  decimal PreviousStock,
  decimal NewStock,
  decimal Quantity,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
