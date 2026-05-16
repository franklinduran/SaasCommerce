namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed record GetStockQuery(
  string? Search,
  bool LowStockOnly,
  string? ProductType,
  Guid? CategoryId,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
