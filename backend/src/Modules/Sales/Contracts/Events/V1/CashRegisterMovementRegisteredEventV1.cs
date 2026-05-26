using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record CashRegisterMovementRegisteredEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid MovementId,
  Guid CashRegisterId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  string MovementType,
  decimal Amount,
  string Reason,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
