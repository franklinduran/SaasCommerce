namespace SaasCommerce.Modules.Reporting.Contracts.Responses;

public sealed record SalesReportResponse(
  IReadOnlyCollection<SalesReportItemDto> Items,
  SalesReportSummaryDto Summary,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);

public sealed record SalesReportItemDto(
  Guid SaleId,
  string Code,
  string? CustomerName,
  string? BranchName,
  string Status,
  string PaymentMethod,
  decimal Total,
  DateTimeOffset CreatedAt,
  DateTimeOffset? CompletedAt);

public sealed record SalesReportSummaryDto(
  int TotalCount,
  decimal TotalAmount,
  decimal AverageAmount);
