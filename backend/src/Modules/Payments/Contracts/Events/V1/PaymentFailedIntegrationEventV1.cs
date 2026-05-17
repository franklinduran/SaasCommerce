using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Payments.Contracts.Events.V1;

public sealed record PaymentFailedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid SaleId,
  Guid BranchId,
  Guid UserId,
  string Reason,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
