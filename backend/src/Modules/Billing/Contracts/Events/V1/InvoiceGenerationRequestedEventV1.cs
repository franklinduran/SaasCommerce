using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Billing.Contracts.Events.V1;

public sealed record InvoiceGenerationRequestedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  Guid PaymentId,
  decimal Total,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
