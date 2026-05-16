namespace SaasCommerce.Modules.Catalog.Contracts.Inventory;

public sealed record ProductInventoryPolicy(
  Guid ProductId,
  Guid BusinessId,
  string ProductType,
  bool TrackInventory,
  bool AllowNegativeStock,
  string UnitOfMeasure);

public sealed record InventoryProductLookup(
  Guid ProductId,
  Guid BusinessId,
  string Name,
  string Sku,
  string? Barcode,
  string ProductType,
  Guid? CategoryId,
  string UnitOfMeasure,
  decimal? MinimumStock,
  decimal? ReorderPoint);
