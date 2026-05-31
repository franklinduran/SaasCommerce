using SaasCommerce.Modules.Reporting.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Reporting.Application.Abstractions;

public interface IReportsReadRepository
{
  Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
    BusinessId businessId,
    DateTimeOffset today,
    DateTimeOffset thirtyDaysAgo,
    CancellationToken cancellationToken = default);

  Task<SalesReportResponse> GetSalesReportAsync(
    BusinessId businessId,
    SalesReportCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<InvoiceReportResponse> GetInvoiceReportAsync(
    BusinessId businessId,
    InvoiceReportCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<AccountsReceivableReportResponse> GetAccountsReceivableReportAsync(
    BusinessId businessId,
    AccountsReceivableCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<LowStockReportResponse> GetLowStockReportAsync(
    BusinessId businessId,
    LowStockCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<PurchaseReportResponse> GetPurchaseReportAsync(
    BusinessId businessId,
    PurchaseReportCriteria criteria,
    CancellationToken cancellationToken = default);
}

public sealed record SalesReportCriteria(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  Guid? BranchId,
  string? Status,
  string? PaymentMethod,
  string? Search,
  int Page,
  int PageSize);

public sealed record InvoiceReportCriteria(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  string? Status,
  Guid? CustomerId,
  string? Search,
  int Page,
  int PageSize);

public sealed record AccountsReceivableCriteria(
  Guid? CustomerId,
  string? Status,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);

public sealed record LowStockCriteria(
  Guid? BranchId,
  Guid? CategoryId,
  string? Search,
  int Page,
  int PageSize);

public sealed record PurchaseReportCriteria(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  Guid? SupplierId,
  string? Status,
  Guid? BranchId,
  int Page,
  int PageSize);
