using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfDailyClosingReadRepository(AppDbContext dbContext) : IDailyClosingReadRepository
{
  public async Task<(IReadOnlyCollection<DailyClosingListItemResponse> Items, int TotalCount)> GetListAsync(
    BusinessId businessId,
    Guid? branchId,
    DateOnly? dateFrom,
    DateOnly? dateTo,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<DailyClosing>()
      .AsNoTracking()
      .Where(dc => dc.BusinessId == businessId);

    if (branchId.HasValue)
    {
      var bId = new BranchId(branchId.Value);
      query = query.Where(dc => dc.BranchId == bId);
    }

    if (dateFrom.HasValue)
    {
      query = query.Where(dc => dc.ClosingDate >= dateFrom.Value);
    }

    if (dateTo.HasValue)
    {
      query = query.Where(dc => dc.ClosingDate <= dateTo.Value);
    }

    var totalCount = await query.CountAsync(cancellationToken);

    var rows = await query
      .OrderByDescending(dc => dc.ClosingDate)
      .ThenByDescending(dc => dc.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(dc => new
      {
        dc.Id,
        dc.BranchId,
        dc.ClosingDate,
        dc.Status,
        dc.TotalSales,
        dc.EstimatedNetProfit,
        dc.NetMarginPercent,
        AlertCount = dc.Alerts.Count(),
        dc.CreatedAt,
        dc.ClosedAt,
      })
      .ToArrayAsync(cancellationToken);

    if (rows.Length == 0)
    {
      return ([], totalCount);
    }

    // Load branch names
    var branchIdObjects = rows.Select(r => r.BranchId).Distinct().ToArray();
    var branchNames = await dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(b => b.BusinessId == businessId && branchIdObjects.Contains(b.Id))
      .Select(b => new { b.Id, b.Name })
      .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

    var items = rows
      .Select(r => new DailyClosingListItemResponse(
        Id: r.Id,
        BranchId: r.BranchId.Value,
        BranchName: branchNames.GetValueOrDefault(r.BranchId, "Sucursal desconocida"),
        ClosingDate: r.ClosingDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        Status: r.Status.ToString(),
        TotalSales: r.TotalSales,
        EstimatedNetProfit: r.EstimatedNetProfit,
        NetMarginPercent: r.NetMarginPercent,
        AlertCount: r.AlertCount,
        CreatedAt: r.CreatedAt,
        ClosedAt: r.ClosedAt))
      .ToArray();

    return (items, totalCount);
  }

  public async Task<IReadOnlyCollection<DailyClosingListItemResponse>> ExportAllAsync(
    BusinessId businessId,
    DateOnly? dateFrom,
    DateOnly? dateTo,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<DailyClosing>()
      .AsNoTracking()
      .Where(dc => dc.BusinessId == businessId);

    if (dateFrom.HasValue)
    {
      query = query.Where(dc => dc.ClosingDate >= dateFrom.Value);
    }

    if (dateTo.HasValue)
    {
      query = query.Where(dc => dc.ClosingDate <= dateTo.Value);
    }

    var rows = await query
      .OrderBy(dc => dc.ClosingDate)
      .Take(10_000)
      .Select(dc => new
      {
        dc.Id,
        dc.BranchId,
        dc.ClosingDate,
        dc.Status,
        dc.TotalSales,
        dc.EstimatedNetProfit,
        dc.NetMarginPercent,
        AlertCount = dc.Alerts.Count(),
        dc.CreatedAt,
        dc.ClosedAt,
      })
      .ToArrayAsync(cancellationToken);

    if (rows.Length == 0)
    {
      return [];
    }

    var branchIdObjects = rows.Select(r => r.BranchId).Distinct().ToArray();
    var branchNames = await dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(b => b.BusinessId == businessId && branchIdObjects.Contains(b.Id))
      .Select(b => new { b.Id, b.Name })
      .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

    return rows
      .Select(r => new DailyClosingListItemResponse(
        Id: r.Id,
        BranchId: r.BranchId.Value,
        BranchName: branchNames.GetValueOrDefault(r.BranchId, "Sucursal desconocida"),
        ClosingDate: r.ClosingDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        Status: r.Status.ToString(),
        TotalSales: r.TotalSales,
        EstimatedNetProfit: r.EstimatedNetProfit,
        NetMarginPercent: r.NetMarginPercent,
        AlertCount: r.AlertCount,
        CreatedAt: r.CreatedAt,
        ClosedAt: r.ClosedAt))
      .ToArray();
  }

  public async Task<DailyClosingDetailResponse?> GetDetailAsync(
    Guid id,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var closing = await dbContext.Set<DailyClosing>()
      .AsNoTracking()
      .Include(dc => dc.Alerts)
      .FirstOrDefaultAsync(
        dc => dc.Id == id && dc.BusinessId == businessId,
        cancellationToken);

    if (closing is null)
    {
      return null;
    }

    var branch = await dbContext.Set<Branch>()
      .AsNoTracking()
      .FirstOrDefaultAsync(b => b.BusinessId == businessId && b.Id == closing.BranchId, cancellationToken);

    return new DailyClosingDetailResponse(
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
        .ToArray());
  }
}
