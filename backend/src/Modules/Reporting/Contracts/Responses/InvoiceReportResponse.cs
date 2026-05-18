namespace SaasCommerce.Modules.Reporting.Contracts.Responses;

public sealed record InvoiceReportResponse(
  IReadOnlyCollection<InvoiceReportItemDto> Items,
  InvoiceReportSummaryDto Summary,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);

public sealed record InvoiceReportItemDto(
  Guid InvoiceId,
  string InvoiceNumber,
  Guid SaleId,
  string? CustomerName,
  string Status,
  decimal Subtotal,
  decimal TaxTotal,
  decimal Total,
  DateTimeOffset CreatedAt);

public sealed record InvoiceReportSummaryDto(
  int TotalCount,
  decimal TotalAmount);
