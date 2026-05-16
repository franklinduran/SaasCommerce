namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryMovementListResponse(
  IReadOnlyCollection<InventoryMovementResponse> Items,
  int Page,
  int PageSize,
  int Total,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
