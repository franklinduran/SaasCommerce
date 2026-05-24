namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record DailyClosingAlertResponse(
  Guid Id,
  string AlertType,
  string Message,
  decimal? EstimatedImpact);

public sealed record DailyClosingListItemResponse(
  Guid Id,
  Guid BranchId,
  string BranchName,
  string ClosingDate,
  string Status,
  decimal TotalSales,
  decimal EstimatedNetProfit,
  decimal NetMarginPercent,
  int AlertCount,
  DateTimeOffset CreatedAt,
  DateTimeOffset? ClosedAt);

public sealed record DailyClosingDetailResponse(
  Guid Id,
  Guid BusinessId,
  Guid BranchId,
  string BranchName,
  string ClosingDate,
  string Status,
  // Sales
  decimal TotalSales,
  decimal CashSales,
  decimal TransferSales,
  decimal CardSales,
  decimal CreditSales,
  int SalesCount,
  // Cash
  decimal CashExpected,
  decimal? CashCounted,
  decimal? CashDifference,
  // Expenses
  decimal TotalExpenses,
  // Profitability
  decimal TotalCost,
  decimal GrossProfit,
  decimal EstimatedNetProfit,
  decimal GrossMarginPercent,
  decimal NetMarginPercent,
  // Credits
  decimal NewCreditsAmount,
  int NewCreditsCount,
  decimal CreditPaymentsReceived,
  // Metadata
  string? Notes,
  DateTimeOffset CreatedAt,
  DateTimeOffset? ClosedAt,
  Guid? ClosedByUserId,
  IReadOnlyCollection<DailyClosingAlertResponse> Alerts);

public sealed record DailyClosingPreviewResponse(
  Guid BranchId,
  string BranchName,
  string ClosingDate,
  // Sales
  decimal TotalSales,
  decimal CashSales,
  decimal TransferSales,
  decimal CardSales,
  decimal CreditSales,
  int SalesCount,
  // Cash
  decimal CashExpected,
  // Expenses
  decimal TotalExpenses,
  // Profitability
  decimal TotalCost,
  decimal GrossProfit,
  decimal EstimatedNetProfit,
  decimal GrossMarginPercent,
  decimal NetMarginPercent,
  // Credits
  decimal NewCreditsAmount,
  int NewCreditsCount,
  decimal CreditPaymentsReceived,
  // Alerts
  IReadOnlyCollection<DailyClosingAlertResponse> Alerts);
