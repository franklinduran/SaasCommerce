namespace SaasCommerce.Modules.Purchasing.Contracts.Responses;

public sealed record PurchaseListResponse(
  IReadOnlyCollection<PurchaseResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage,
  decimal TotalPurchased);
