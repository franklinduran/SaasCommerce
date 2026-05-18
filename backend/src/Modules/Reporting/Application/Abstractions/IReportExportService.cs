namespace SaasCommerce.Modules.Reporting.Application.Abstractions;

public interface IReportExportService
{
  Task<byte[]> ExportSalesAsync(
    Guid businessId,
    SalesReportCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<byte[]> ExportInvoicesAsync(
    Guid businessId,
    InvoiceReportCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<byte[]> ExportAccountsReceivableAsync(
    Guid businessId,
    AccountsReceivableCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<byte[]> ExportLowStockAsync(
    Guid businessId,
    LowStockCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<byte[]> ExportPurchasesAsync(
    Guid businessId,
    PurchaseReportCriteria criteria,
    CancellationToken cancellationToken = default);
}
