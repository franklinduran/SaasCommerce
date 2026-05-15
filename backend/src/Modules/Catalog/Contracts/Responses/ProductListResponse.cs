namespace SaasCommerce.Modules.Catalog.Contracts.Responses;

public sealed record ProductListResponse(
  IReadOnlyCollection<ProductResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages);
