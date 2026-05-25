namespace SaasCommerce.Modules.Catalog.Application.Import;

/// <summary>
/// Imports products from a CSV stream into the authenticated business's catalog.
/// Optionally creates initial inventory movements when StockQuantity > 0.
/// </summary>
public sealed record ImportProductsCommand(
  Stream CsvStream,
  bool CreateInitialInventory = true);

/// <summary>
/// A single parsed row from the CSV file.
/// </summary>
public sealed record ProductImportRow(
  int RowNumber,
  string? Name,
  string? Sku,
  string? CategoryName,
  decimal? SalePrice,
  decimal? CostPrice,
  decimal? StockQuantity);
