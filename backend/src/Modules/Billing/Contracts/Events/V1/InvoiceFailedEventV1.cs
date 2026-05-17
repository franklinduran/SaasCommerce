using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Billing.Contracts.Events.V1;

public sealed record InvoiceFailedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  Guid PaymentId,
  string Reason,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
