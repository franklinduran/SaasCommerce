using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record SaleFailedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  string Reason,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
