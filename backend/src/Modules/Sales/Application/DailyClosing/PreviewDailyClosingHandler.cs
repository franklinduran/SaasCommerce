using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Profitability;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

public sealed record PreviewDailyClosingQuery(DateOnly Date, Guid BranchId);

public sealed class PreviewDailyClosingHandler(
  IDailyClosingDataGatherer gatherer,
  ICurrentUserService currentUser,
  AppDbContext dbContext)
{
  public async Task<Result<DailyClosingPreviewResponse>> Handle(
    PreviewDailyClosingQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<DailyClosingPreviewResponse>(DailyClosingErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);
    var branchId = new BranchId(query.BranchId);

    // Get branch name
    var branch = await dbContext.Set<Branch>()
      .AsNoTracking()
      .FirstOrDefaultAsync(b => b.BusinessId == bId && b.Id == branchId, cancellationToken);

    var branchName = branch?.Name ?? "Sucursal desconocida";

    // Gather data
    var data = await gatherer.GatherAsync(bId, branchId, query.Date, cancellationToken);

    // Compute profitability
    var cashExpected = DailyClosingCalculator.ComputeCashExpected(data.CashSessionOpeningBalance, data.CashSales);
    var grossProfit = ProfitabilityCalculator.GrossProfit(data.TotalSales, data.TotalCost);
    var netProfit = ProfitabilityCalculator.EstimatedNetProfit(grossProfit, data.TotalExpenses);
    var grossMargin = ProfitabilityCalculator.GrossMarginPercent(data.TotalSales, grossProfit);
    var netMargin = ProfitabilityCalculator.NetMarginPercent(data.TotalSales, netProfit);

    // Build alerts
    var alerts = BuildAlerts(data, netProfit);

    return Result.Success(new DailyClosingPreviewResponse(
      BranchId: query.BranchId,
      BranchName: branchName,
      ClosingDate: query.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
      TotalSales: data.TotalSales,
      CashSales: data.CashSales,
      TransferSales: data.TransferSales,
      CardSales: data.CardSales,
      CreditSales: data.CreditSales,
      SalesCount: data.SalesCount,
      CashExpected: cashExpected,
      TotalExpenses: data.TotalExpenses,
      TotalCost: data.TotalCost,
      GrossProfit: grossProfit,
      EstimatedNetProfit: netProfit,
      GrossMarginPercent: grossMargin,
      NetMarginPercent: netMargin,
      NewCreditsAmount: data.NewCreditsAmount,
      NewCreditsCount: data.NewCreditsCount,
      CreditPaymentsReceived: data.CreditPaymentsReceived,
      Alerts: alerts));
  }

  private static List<DailyClosingAlertResponse> BuildAlerts(
    DailyClosingData data,
    decimal netProfit)
  {
    var list = new List<DailyClosingAlertResponse>();

    if (data.HasOpenCashSessions)
    {
      list.Add(new DailyClosingAlertResponse(
        Guid.NewGuid(),
        "OpenCashSession",
        "Hay sesiones de caja abiertas en esta sucursal.",
        null));
    }

    if (data.HasMissingCosts)
    {
      list.Add(new DailyClosingAlertResponse(
        Guid.NewGuid(),
        "MissingProductCost",
        "Algunos productos no tienen costo registrado. La rentabilidad puede estar subestimada.",
        null));
    }

    if (netProfit < 0)
    {
      list.Add(new DailyClosingAlertResponse(
        Guid.NewGuid(),
        "NegativeMargin",
        $"El margen neto estimado es negativo ({netProfit:N2}). Revisa gastos y costos.",
        Math.Abs(netProfit)));
    }

    var expenseRatio = DailyClosingCalculator.ExpenseRatio(data.TotalSales, data.TotalExpenses);
    if (DailyClosingCalculator.IsHighExpenseRatio(expenseRatio))
    {
      list.Add(new DailyClosingAlertResponse(
        Guid.NewGuid(),
        "HighExpenseRatio",
        $"Los gastos operativos representan el {expenseRatio:N1}% de las ventas (umbral: 30%).",
        data.TotalExpenses));
    }

    var creditRatio = DailyClosingCalculator.CreditSalesRatio(data.TotalSales, data.CreditSales);
    if (DailyClosingCalculator.IsCreditSalesHigh(creditRatio))
    {
      list.Add(new DailyClosingAlertResponse(
        Guid.NewGuid(),
        "CreditSalesHigh",
        $"Las ventas a crédito representan el {creditRatio:N1}% de las ventas totales (umbral: 40%).",
        data.CreditSales));
    }

    return list;
  }
}
