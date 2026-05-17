using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record SaleStatusChangedNotificationV1(
  Guid EventId,
  Guid CorrelationId,
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  string Status,
  string? Reason,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
