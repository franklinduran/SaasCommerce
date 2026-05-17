namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

public sealed record GetSuppliersQuery(
  string? Query,
  bool? IsActive,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
