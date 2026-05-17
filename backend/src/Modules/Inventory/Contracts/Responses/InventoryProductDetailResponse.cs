namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryProductDetailResponse(
  Guid ProductId,
  string ProductName,
  string Sku,
  string? Barcode,
  string UnitOfMeasure,
  decimal? MinimumStock,
  decimal? ReorderPoint,
  IReadOnlyCollection<InventoryBranchStockResponse> Branches,
  IReadOnlyCollection<InventoryMovementResponse> RecentMovements,
  IReadOnlyCollection<InventoryAlertResponse> Alerts);

public sealed record InventoryBranchStockResponse(
  Guid BranchId,
  string BranchName,
  decimal CurrentStock,
  decimal? MinimumStock,
  bool IsLowStock,
  bool IsOutOfStock,
  string Status,
  DateTimeOffset? LastUpdatedAt);

public sealed record InventoryAlertResponse(
  Guid BranchId,
  string BranchName,
  Guid ProductId,
  string ProductName,
  decimal CurrentStock,
  decimal MinimumStock,
  string Severity,
  DateTimeOffset DetectedAt);
