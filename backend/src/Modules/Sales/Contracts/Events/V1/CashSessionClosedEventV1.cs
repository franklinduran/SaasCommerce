using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record CashSessionClosedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid CashSessionId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  decimal OpeningBalance,
  decimal SystemBalance,
  decimal ClosingBalance,
  decimal Difference,
  string Outcome,
  DateTimeOffset ClosedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => ClosedAt;
}
