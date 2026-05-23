using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record OperatingExpensePaidEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid ExpenseId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  decimal Amount,
  string PaymentMethod,
  Guid? CashSessionId,
  Guid? CashMovementId,
  DateTimeOffset PaidAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => PaidAt;
}
