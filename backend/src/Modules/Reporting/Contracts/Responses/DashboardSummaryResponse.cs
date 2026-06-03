namespace SaasCommerce.Modules.Reporting.Contracts.Responses;

public sealed record DashboardSummaryResponse(
  DashboardSalesTodayDto SalesToday,
  DashboardInvoicesTodayDto InvoicesToday,
  DashboardReceivablesDto Receivables,
  DashboardLowStockDto LowStock,
  IReadOnlyCollection<DashboardRecentSaleDto> RecentSales,
  IReadOnlyCollection<DashboardRecentInvoiceDto> RecentInvoices,
  IReadOnlyCollection<DashboardRecentPurchaseDto> RecentPurchases,
  IReadOnlyCollection<DailySalesPointDto> DailySales,
  IReadOnlyCollection<DailyPurchasesPointDto> DailyPurchases,
  IReadOnlyCollection<PaymentMethodTotalDto> PaymentMethodTotals,
  SaleStatusBreakdownDto SaleStatusBreakdown,
  IReadOnlyCollection<TopProductDto> TopProducts);

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

public sealed record DailySalesPointDto(
  string Date,
  decimal TotalSales,
  int SaleCount);

public sealed record DailyPurchasesPointDto(
  string Date,
  decimal TotalPurchases,
  int PurchaseCount);

public sealed record PaymentMethodTotalDto(
  string Method,
  int Count,
  decimal Total);

public sealed record SaleStatusBreakdownDto(
  int Completed,
  int Cancelled,
  int Pending,
  int Failed,
  int Other);

public sealed record TopProductDto(
  string Name,
  int TotalQuantity,
  decimal TotalRevenue);
