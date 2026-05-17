namespace SaasCommerce.Modules.Purchasing.Contracts.Requests;

public sealed record CreatePurchaseRequest(
  Guid SupplierId,
  Guid? BranchId,
  IReadOnlyCollection<CreatePurchaseItemRequest> Items,
  string? SupplierInvoiceNumber,
  DateTimeOffset? PurchaseDate,
  string? Notes,
  bool ReceiveNow = true);

public sealed record CreatePurchaseItemRequest(
  Guid ProductId,
  decimal Quantity,
  decimal UnitCost);
