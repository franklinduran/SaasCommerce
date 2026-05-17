namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record SaleListResponse(
  IReadOnlyCollection<SaleResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
