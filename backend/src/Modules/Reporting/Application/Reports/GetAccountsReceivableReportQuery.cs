namespace SaasCommerce.Modules.Reporting.Application.Reports;

public sealed record GetAccountsReceivableReportQuery(
  Guid? CustomerId,
  string? Status,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);
