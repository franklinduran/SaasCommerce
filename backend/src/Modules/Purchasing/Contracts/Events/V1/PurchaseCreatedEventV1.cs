using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Purchasing.Contracts.Events.V1;

public sealed record PurchaseCreatedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid PurchaseId,
  Guid BusinessId,
  Guid BranchId,
  Guid SupplierId,
  Guid UserId,
  IReadOnlyCollection<PurchaseItemV1> Items,
  decimal Total,
  string Status,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
