namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record ProfitabilitySummaryResponse(
  DateTimeOffset DateFrom,
  DateTimeOffset DateTo,
  decimal TotalSales,
  decimal TotalCost,
  decimal GrossProfit,
  decimal OperatingExpenses,
  decimal EstimatedNetProfit,
  decimal GrossMarginPercent,
  decimal NetMarginPercent,
  int SalesCount,
  int WarningCount);

public sealed record ProductProfitabilityResponse(
  Guid ProductId,
  string ProductName,
  string Sku,
  decimal TotalQuantity,
  decimal TotalSales,
  decimal TotalCost,
  decimal GrossProfit,
  decimal MarginPercent,
  bool HasMissingCost,
  Guid? CategoryId,
  string? CategoryName);

public sealed record BranchProfitabilityResponse(
  Guid BranchId,
  string BranchName,
  decimal TotalSales,
  decimal TotalCost,
  decimal GrossProfit,
  decimal OperatingExpenses,
  decimal EstimatedNetProfit,
  decimal NetMarginPercent,
  int SalesCount);

public sealed record ProfitabilityAlertResponse(
  string AlertType,
  string Message,
  Guid? BranchId,
  string? BranchName,
  Guid? ProductId,
  string? ProductName,
  decimal? EstimatedImpact);
