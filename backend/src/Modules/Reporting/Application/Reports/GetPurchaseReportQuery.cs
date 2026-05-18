namespace SaasCommerce.Modules.Reporting.Application.Reports;

public sealed record GetPurchaseReportQuery(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  Guid? SupplierId,
  string? Status,
  Guid? BranchId,
  int Page,
  int PageSize);
