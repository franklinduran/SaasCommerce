namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryListResponse(
  IReadOnlyCollection<StockItemResponse> Items,
  int Page,
  int PageSize,
  int Total,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
