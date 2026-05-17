namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed record GetInventoryQuery(
  Guid? ProductId,
  Guid? BranchId,
  string? Search,
  bool LowStockOnly,
  bool OutOfStockOnly,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
