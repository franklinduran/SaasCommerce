using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record SaleReturnFailedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid BranchId,
  Guid SaleId,
  Guid SaleReturnId,
  Guid UserId,
  string Reason,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
