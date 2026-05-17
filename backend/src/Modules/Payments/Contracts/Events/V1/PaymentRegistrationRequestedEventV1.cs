using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Payments.Contracts.Events.V1;

public sealed record PaymentRegistrationRequestedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  decimal Total,
  string PaymentMethod,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
