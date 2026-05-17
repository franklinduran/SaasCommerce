using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Purchasing.Contracts.Events.V1;

public sealed record ProductCostUpdatedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid PurchaseId,
  Guid BusinessId,
  Guid BranchId,
  Guid ProductId,
  decimal PreviousCost,
  decimal NewCost,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
