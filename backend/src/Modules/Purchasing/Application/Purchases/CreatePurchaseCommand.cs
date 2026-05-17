namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed record CreatePurchaseCommand(
  Guid SupplierId,
  Guid? BranchId,
  IReadOnlyCollection<CreatePurchaseItemCommand> Items,
  string? SupplierInvoiceNumber,
  DateTimeOffset? PurchaseDate,
  string? Notes,
  bool ReceiveNow);

public sealed record CreatePurchaseItemCommand(
  Guid ProductId,
  decimal Quantity,
  decimal UnitCost);
