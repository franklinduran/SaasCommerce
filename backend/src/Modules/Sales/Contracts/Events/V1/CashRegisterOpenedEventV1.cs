using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record CashRegisterOpenedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid CashRegisterId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  decimal OpeningAmount,
  DateTimeOffset OpenedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => OpenedAt;
}
