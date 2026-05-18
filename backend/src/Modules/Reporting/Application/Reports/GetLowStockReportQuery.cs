namespace SaasCommerce.Modules.Reporting.Application.Reports;

public sealed record GetLowStockReportQuery(
  Guid? BranchId,
  Guid? CategoryId,
  string? Search,
  int Page,
  int PageSize);
