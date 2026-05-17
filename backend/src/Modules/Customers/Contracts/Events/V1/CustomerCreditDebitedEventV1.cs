using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Customers.Contracts.Events.V1;

public sealed record CustomerCreditDebitedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid CustomerId,
  Guid SaleId,
  decimal Amount,
  decimal NewBalance,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
