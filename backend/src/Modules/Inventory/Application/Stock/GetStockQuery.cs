namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed record GetStockQuery(
  Guid? ProductId,
  Guid? BranchId,
  string? Search,
  bool LowStockOnly,
  bool OutOfStockOnly,
  string? ProductType,
  Guid? CategoryId,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
