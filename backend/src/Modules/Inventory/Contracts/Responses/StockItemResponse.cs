namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record StockItemResponse(
  Guid Id,
  Guid BusinessId,
  Guid BranchId,
  Guid ProductId,
  string ProductName,
  string Sku,
  string? Barcode,
  string UnitOfMeasure,
  decimal Quantity,
  decimal? MinimumStock,
  decimal? ReorderPoint,
  bool IsLowStock);
