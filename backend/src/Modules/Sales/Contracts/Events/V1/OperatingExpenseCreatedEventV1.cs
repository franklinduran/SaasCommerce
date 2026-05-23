using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record OperatingExpenseCreatedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid ExpenseId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  Guid CategoryId,
  string Description,
  decimal Amount,
  string PaymentMethod,
  string Status,
  DateTimeOffset ExpenseDate,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
