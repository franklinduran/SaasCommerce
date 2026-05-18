namespace SaasCommerce.Modules.Reporting.Application.Reports;

public sealed record GetSalesReportQuery(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  Guid? BranchId,
  string? Status,
  string? PaymentMethod,
  string? Search,
  int Page,
  int PageSize);
