namespace SaasCommerce.Modules.Reporting.Contracts.Responses;

public sealed record LowStockReportResponse(
  IReadOnlyCollection<LowStockReportItemDto> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);

public sealed record LowStockReportItemDto(
  Guid ProductId,
  string ProductName,
  string? Sku,
  string? CategoryName,
  string? BranchName,
  decimal CurrentStock,
  decimal MinimumStock,
  decimal SuggestedRestock,
  decimal SalePrice,
  DateTimeOffset? LastMovementAt);
