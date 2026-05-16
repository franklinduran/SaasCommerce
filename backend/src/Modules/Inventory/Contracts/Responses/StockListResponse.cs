namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record StockListResponse(
  IReadOnlyCollection<StockItemResponse> Items,
  int Page,
  int PageSize,
  int Total,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
