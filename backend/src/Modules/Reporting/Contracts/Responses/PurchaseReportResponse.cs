namespace SaasCommerce.Modules.Reporting.Contracts.Responses;

public sealed record PurchaseReportResponse(
  IReadOnlyCollection<PurchaseReportItemDto> Items,
  PurchaseReportSummaryDto Summary,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);

public sealed record PurchaseReportItemDto(
  Guid PurchaseId,
  string? SupplierName,
  string? SupplierInvoiceNumber,
  string Status,
  int ItemCount,
  decimal Total,
  DateTimeOffset PurchaseDate,
  DateTimeOffset CreatedAt);

public sealed record PurchaseReportSummaryDto(
  int TotalCount,
  decimal TotalAmount);
