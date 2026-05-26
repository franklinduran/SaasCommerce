using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

/// <summary>
/// Data computed from DB for a specific branch/date. Used by both preview and create handlers.
/// </summary>
public sealed record DailyClosingData(
  // Sales by payment method
  decimal TotalSales,
  decimal CashSales,
  decimal TransferSales,
  decimal CardSales,
  decimal CreditSales,
  int SalesCount,
  // Cash sessions
  decimal CashSessionOpeningBalance,
  bool HasOpenCashSessions,
  // Expenses
  decimal TotalExpenses,
  // Cost
  decimal TotalCost,
  bool HasMissingCosts,
  // Credits (business-wide for date)
  decimal NewCreditsAmount,
  int NewCreditsCount,
  decimal CreditPaymentsReceived);

/// <summary>
/// Gathers operational data for a daily closing without persisting anything.
/// </summary>
public interface IDailyClosingDataGatherer
{
  Task<DailyClosingData> GatherAsync(
    BusinessId businessId,
    BranchId branchId,
    DateOnly closingDate,
    CancellationToken cancellationToken = default);
}
