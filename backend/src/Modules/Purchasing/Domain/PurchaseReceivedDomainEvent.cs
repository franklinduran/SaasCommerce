using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Purchasing.Domain;

public sealed record PurchaseReceivedDomainEvent(
  Guid EventId,
  Guid CorrelationId,
  Guid PurchaseId,
  Guid BusinessId,
  Guid BranchId,
  Guid SupplierId,
  decimal Total,
  DateTimeOffset CreatedAt) : IDomainEvent
{
  public DateTimeOffset OccurredAt => CreatedAt;
}
