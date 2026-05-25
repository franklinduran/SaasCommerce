using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Catalog.Application.Import;

public static class ImportProductsErrors
{
  public static DomainError EmptyFile { get; } =
    new("IMPORT_EMPTY_FILE", "The CSV file is empty or contains no data rows.");

  public static DomainError InvalidFormat { get; } =
    new("IMPORT_INVALID_FORMAT",
      "CSV format is invalid. Expected columns: Name, Sku, CategoryName (optional), SalePrice, CostPrice, StockQuantity (optional).");

  public static DomainError UserContextRequired { get; } =
    new("IMPORT_USER_CONTEXT_REQUIRED", "User business context is required to import products.");

  public static DomainError AllRowsFailed { get; } =
    new("IMPORT_ALL_ROWS_FAILED", "All rows failed validation. No products were imported.");
}
