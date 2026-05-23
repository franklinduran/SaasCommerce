using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record CashSessionOpenedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid CashSessionId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  decimal OpeningBalance,
  DateTimeOffset OpenedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => OpenedAt;
}
