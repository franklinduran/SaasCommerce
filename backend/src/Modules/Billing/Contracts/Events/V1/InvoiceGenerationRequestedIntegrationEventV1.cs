using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Billing.Contracts.Events.V1;

public sealed record InvoiceGenerationRequestedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid SaleId,
  Guid BranchId,
  Guid UserId,
  decimal Total,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
