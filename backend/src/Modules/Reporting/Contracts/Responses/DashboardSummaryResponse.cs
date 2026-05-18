namespace SaasCommerce.Modules.Reporting.Contracts.Responses;

public sealed record DashboardSummaryResponse(
  DashboardSalesTodayDto SalesToday,
  DashboardInvoicesTodayDto InvoicesToday,
  DashboardReceivablesDto Receivables,
  DashboardLowStockDto LowStock,
  IReadOnlyCollection<DashboardRecentSaleDto> RecentSales,
  IReadOnlyCollection<DashboardRecentInvoiceDto> RecentInvoices,
  IReadOnlyCollection<DashboardRecentPurchaseDto> RecentPurchases);

public sealed record DashboardSalesTodayDto(
  int Count,
  decimal TotalAmount);

public sealed record DashboardInvoicesTodayDto(
  int Count,
  decimal TotalAmount);

public sealed record DashboardReceivablesDto(
  int CustomerCount,
  decimal TotalPending);

public sealed record DashboardLowStockDto(
  int ProductCount);

public sealed record DashboardRecentSaleDto(
  Guid SaleId,
  string Code,
  string Status,
  string PaymentMethod,
  decimal Total,
  DateTimeOffset CreatedAt);

public sealed record DashboardRecentInvoiceDto(
  Guid InvoiceId,
  string InvoiceNumber,
  string Status,
  decimal Total,
  DateTimeOffset CreatedAt);

public sealed record DashboardRecentPurchaseDto(
  Guid PurchaseId,
  string? SupplierName,
  string Status,
  decimal Total,
  DateTimeOffset CreatedAt);
