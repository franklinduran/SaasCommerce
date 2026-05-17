using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record StockValidationRequestedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  IReadOnlyCollection<SaleItemV1> Items,
  decimal Total,
  string PaymentMethod,
  DateTimeOffset CreatedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
