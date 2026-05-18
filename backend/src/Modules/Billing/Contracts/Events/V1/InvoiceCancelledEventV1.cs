using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Billing.Contracts.Events.V1;

public sealed record InvoiceCancelledEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid BranchId,
  Guid SaleId,
  Guid InvoiceId,
  string InvoiceNumber,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
