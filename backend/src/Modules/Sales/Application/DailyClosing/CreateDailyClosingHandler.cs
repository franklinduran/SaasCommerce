using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Profitability;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

public sealed record CreateDailyClosingCommand(
  DateOnly Date,
  Guid BranchId,
  string? Notes);

public sealed class CreateDailyClosingHandler(
  IDailyClosingRepository repository,
  IDailyClosingDataGatherer gatherer,
  ICurrentUserService currentUser,
  AppDbContext dbContext,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<DailyClosingDetailResponse>> Handle(
    CreateDailyClosingCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);
    var branchId = new BranchId(command.BranchId);

    // Idempotency: check if closing already exists for this date+branch
    var existing = await repository.GetByDateAndBranchAsync(bId, branchId, command.Date, cancellationToken);
    if (existing is not null)
    {
      if (existing.Status == DailyClosingStatus.Closed)
      {
        return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.AlreadyClosed);
      }

      // Return existing draft (idempotent)
      var existingDetail = await BuildDetailResponse(existing, bId, cancellationToken);
      return Result.Success(existingDetail);
    }

    // Gather data
    var data = await gatherer.GatherAsync(bId, branchId, command.Date, cancellationToken);

    // Compute profitability
    var cashExpected = DailyClosingCalculator.ComputeCashExpected(data.CashSessionOpeningBalance, data.CashSales);
    var grossProfit = ProfitabilityCalculator.GrossProfit(data.TotalSales, data.TotalCost);
    var netProfit = ProfitabilityCalculator.EstimatedNetProfit(grossProfit, data.TotalExpenses);
    var grossMargin = ProfitabilityCalculator.GrossMarginPercent(data.TotalSales, grossProfit);
    var netMargin = ProfitabilityCalculator.NetMarginPercent(data.TotalSales, netProfit);

    // Create domain entity
    var closing = DailyClosing.Create(
      Guid.NewGuid(),
      bId,
      branchId,
      userId,
      command.Date,
      data.TotalSales,
      data.CashSales,
      data.TransferSales,
      data.CardSales,
      data.CreditSales,
      data.SalesCount,
      cashExpected,
      data.TotalExpenses,
      data.TotalCost,
      grossProfit,
      netProfit,
      grossMargin,
      netMargin,
      data.NewCreditsAmount,
      data.NewCreditsCount,
      data.CreditPaymentsReceived,
      command.Notes,
      clock.UtcNow);

    // Add alerts
    BuildAndAddAlerts(closing, data, netProfit);

    await repository.AddAsync(closing, cancellationToken);

    // Publish event
    var ev = new DailyClosingCreatedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      closing.Id,
      businessId,
      command.BranchId,
      userId,
      command.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
      data.TotalSales,
      closing.Alerts.Count,
      clock.UtcNow);

    await outbox.AddAsync(ev, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    var detail = await BuildDetailResponse(closing, bId, cancellationToken);
    return Result.Success(detail);
  }

  private async Task<DailyClosingDetailResponse> BuildDetailResponse(
    DailyClosing closing,
    BusinessId bId,
    CancellationToken cancellationToken)
  {
    var branchIdObj = closing.BranchId;
    var branch = await dbContext.Set<Branch>()
      .AsNoTracking()
      .FirstOrDefaultAsync(b => b.BusinessId == bId && b.Id == branchIdObj, cancellationToken);

    var branchName = branch?.Name ?? "Sucursal desconocida";

    return new DailyClosingDetailResponse(
      Id: closing.Id,
      BusinessId: closing.BusinessId.Value,
      BranchId: closing.BranchId.Value,
      BranchName: branchName,
      ClosingDate: closing.ClosingDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
      Status: closing.Status.ToString(),
      TotalSales: closing.TotalSales,
      CashSales: closing.CashSales,
      TransferSales: closing.TransferSales,
      CardSales: closing.CardSales,
      CreditSales: closing.CreditSales,
      SalesCount: closing.SalesCount,
      CashExpected: closing.CashExpected,
      CashCounted: closing.CashCounted,
      CashDifference: closing.CashDifference,
      TotalExpenses: closing.TotalExpenses,
      TotalCost: closing.TotalCost,
      GrossProfit: closing.GrossProfit,
      EstimatedNetProfit: closing.EstimatedNetProfit,
      GrossMarginPercent: closing.GrossMarginPercent,
      NetMarginPercent: closing.NetMarginPercent,
      NewCreditsAmount: closing.NewCreditsAmount,
      NewCreditsCount: closing.NewCreditsCount,
      CreditPaymentsReceived: closing.CreditPaymentsReceived,
      Notes: closing.Notes,
      CreatedAt: closing.CreatedAt,
      ClosedAt: closing.ClosedAt,
      ClosedByUserId: closing.ClosedByUserId,
      Alerts: closing.Alerts
        .Select(a => new DailyClosingAlertResponse(a.Id, a.AlertType.ToString(), a.Message, a.EstimatedImpact))
        .ToArray());
  }

  private static void BuildAndAddAlerts(DailyClosing closing, DailyClosingData data, decimal netProfit)
  {
    if (data.HasOpenCashSessions)
    {
      closing.AddAlert(DailyClosingAlert.Create(
        Guid.NewGuid(),
        closing.Id,
        DailyClosingAlertType.OpenCashSession,
        "Hay sesiones de caja abiertas en esta sucursal."));
    }

    if (data.HasMissingCosts)
    {
      closing.AddAlert(DailyClosingAlert.Create(
        Guid.NewGuid(),
        closing.Id,
        DailyClosingAlertType.MissingProductCost,
        "Algunos productos no tienen costo registrado. La rentabilidad puede estar subestimada."));
    }

    if (netProfit < 0)
    {
      closing.AddAlert(DailyClosingAlert.Create(
        Guid.NewGuid(),
        closing.Id,
        DailyClosingAlertType.NegativeMargin,
        $"El margen neto estimado es negativo ({netProfit:N2}). Revisa gastos y costos.",
        Math.Abs(netProfit)));
    }

    var expenseRatio = DailyClosingCalculator.ExpenseRatio(data.TotalSales, data.TotalExpenses);
    if (DailyClosingCalculator.IsHighExpenseRatio(expenseRatio))
    {
      closing.AddAlert(DailyClosingAlert.Create(
        Guid.NewGuid(),
        closing.Id,
        DailyClosingAlertType.HighExpenseRatio,
        $"Los gastos operativos representan el {expenseRatio:N1}% de las ventas (umbral: 30%).",
        data.TotalExpenses));
    }

    var creditRatio = DailyClosingCalculator.CreditSalesRatio(data.TotalSales, data.CreditSales);
    if (DailyClosingCalculator.IsCreditSalesHigh(creditRatio))
    {
      closing.AddAlert(DailyClosingAlert.Create(
        Guid.NewGuid(),
        closing.Id,
        DailyClosingAlertType.CreditSalesHigh,
        $"Las ventas a crédito representan el {creditRatio:N1}% de las ventas totales (umbral: 40%).",
        data.CreditSales));
    }
  }
}
