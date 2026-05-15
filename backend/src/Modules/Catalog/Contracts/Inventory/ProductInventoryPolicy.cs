namespace SaasCommerce.Modules.Catalog.Contracts.Inventory;

public sealed record ProductInventoryPolicy(
  Guid ProductId,
  Guid BusinessId,
  string ProductType,
  bool TrackInventory,
  bool AllowNegativeStock,
  string UnitOfMeasure);
