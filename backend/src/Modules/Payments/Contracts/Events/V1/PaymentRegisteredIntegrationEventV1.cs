using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Payments.Contracts.Events.V1;

public sealed record PaymentRegisteredIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid SaleId,
  Guid PaymentId,
  decimal Amount,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
