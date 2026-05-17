namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed record GetPurchasesQuery(
  Guid? SupplierId,
  Guid? BranchId,
  string? Status,
  string? Query,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
