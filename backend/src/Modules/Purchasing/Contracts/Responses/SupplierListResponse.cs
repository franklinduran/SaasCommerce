namespace SaasCommerce.Modules.Purchasing.Contracts.Responses;

public sealed record SupplierListResponse(
  IReadOnlyCollection<SupplierResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
