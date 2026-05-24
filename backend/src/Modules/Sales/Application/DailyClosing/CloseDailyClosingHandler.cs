using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

public sealed record CloseDailyClosingCommand(
  Guid ClosingId,
  decimal CashCounted,
  string? Notes);

public sealed class CloseDailyClosingHandler(
  IDailyClosingRepository repository,
  ICurrentUserService currentUser,
  AppDbContext dbContext,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<DailyClosingDetailResponse>> Handle(
    CloseDailyClosingCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.UserContextRequired);
    }

    if (command.CashCounted < 0)
    {
      return Result.Failure<DailyClosingDetailResponse>(
        new DomainError("daily_closing.invalid_cash_counted", "El efectivo contado no puede ser negativo."));
    }

    var bId = new BusinessId(businessId);
    var closing = await repository.GetByIdAsync(command.ClosingId, bId, cancellationToken);

    if (closing is null)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.NotFound);
    }

    if (closing.Status == DailyClosingStatus.Closed)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.AlreadyClosedStatus);
    }

    // Validate no open cash sessions remain for this branch
    var hasOpenSessions = await dbContext.Set<CashSession>()
      .AsNoTracking()
      .AnyAsync(
        s => s.BusinessId == bId
             && s.BranchId == closing.BranchId
             && s.Status == CashSessionStatus.Open,
        cancellationToken);

    if (hasOpenSessions)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.OpenCashSessionsExist);
    }

    try
    {
      closing.Close(userId, command.CashCounted, command.Notes, clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.AlreadyClosedStatus);
    }

    var ev = new DailyClosingClosedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      closing.Id,
      businessId,
      closing.BranchId.Value,
      userId,
      closing.ClosingDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
      closing.TotalSales,
      closing.EstimatedNetProfit,
      closing.CashDifference,
      clock.UtcNow);

    await outbox.AddAsync(ev, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    // Build response
    var branch = await dbContext.Set<Branch>()
      .AsNoTracking()
      .FirstOrDefaultAsync(b => b.BusinessId == bId && b.Id == closing.BranchId, cancellationToken);

    return Result.Success(new DailyClosingDetailResponse(
      Id: closing.Id,
      BusinessId: closing.BusinessId.Value,
      BranchId: closing.BranchId.Value,
      BranchName: branch?.Name ?? "Sucursal desconocida",
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
        .ToArray()));
  }
}
