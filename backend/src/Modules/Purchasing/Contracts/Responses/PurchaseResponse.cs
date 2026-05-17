namespace SaasCommerce.Modules.Purchasing.Contracts.Responses;

public sealed record PurchaseResponse(
  Guid PurchaseId,
  Guid BusinessId,
  Guid BranchId,
  Guid SupplierId,
  string SupplierName,
  Guid UserId,
  string Status,
  string? SupplierInvoiceNumber,
  DateTimeOffset PurchaseDate,
  string? Notes,
  decimal Total,
  IReadOnlyCollection<PurchaseItemResponse> Items,
  IReadOnlyCollection<PurchaseInventoryMovementResponse> Movements,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt,
  DateTimeOffset? ReceivedAt,
  DateTimeOffset? CancelledAt)
{
  public Guid Id => PurchaseId;

  public string Code => PurchaseId.ToString("N")[..8].ToUpperInvariant();
}
