using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record CashMovementRegisteredEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid MovementId,
  Guid CashSessionId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  string MovementType,
  decimal Amount,
  string Description,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
