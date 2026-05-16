namespace SaasCommerce.Modules.Catalog.Application.Products;

public sealed record GetProductsQuery(
  string? Query,
  string? ProductType,
  Guid? CategoryId,
  bool? IsActive,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
