namespace SaasCommerce.Modules.Reporting.Application.Reports;

public sealed record GetInvoiceReportQuery(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  string? Status,
  Guid? CustomerId,
  string? Search,
  int Page,
  int PageSize);
