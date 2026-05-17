using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Billing.Contracts.Events.V1;

public sealed record InvoiceGeneratedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid SaleId,
  Guid BranchId,
  Guid UserId,
  Guid InvoiceId,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
