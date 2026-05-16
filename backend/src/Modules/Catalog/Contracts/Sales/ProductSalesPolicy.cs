namespace SaasCommerce.Modules.Catalog.Contracts.Sales;

public sealed record ProductSalesPolicy(
  Guid ProductId,
  Guid BusinessId,
  string Name,
  string Sku,
  string? Barcode,
  string ProductType,
  string UnitOfMeasure,
  decimal SalePrice,
  string TaxCategory,
  decimal TaxRate,
  bool IsTaxIncluded,
  bool AllowsDiscount,
  bool TrackInventory,
  bool IsActive,
  bool CanBeSold,
  string? ReasonIfCannotBeSold);
