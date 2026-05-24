using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

/// <summary>
/// Gathers all data needed for a daily closing from EF Core.
/// Uses the "2-query" pattern for SystemBalance since it is not SQL-translatable.
/// </summary>
public sealed class EfDailyClosingDataGatherer(AppDbContext dbContext) : IDailyClosingDataGatherer
{
  public async Task<DailyClosingData> GatherAsync(
    BusinessId businessId,
    BranchId branchId,
    DateOnly date,
    CancellationToken cancellationToken = default)
  {
    var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    var dayEnd = dayStart.AddDays(1);
    var dayStartOffset = new DateTimeOffset(dayStart, TimeSpan.Zero);
    var dayEndOffset = new DateTimeOffset(dayEnd, TimeSpan.Zero);

    // ── 1. Sales by payment method ────────────────────────────────────────────
    var salesRaw = await dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId
                  && s.BranchId == branchId
                  && s.Status == SaleStatus.Completed
                  && s.CreatedAt >= dayStartOffset
                  && s.CreatedAt < dayEndOffset)
      .GroupBy(s => s.PaymentMethod)
      .Select(g => new { PaymentMethod = g.Key, Total = g.Sum(s => s.Total), Count = g.Count() })
      .ToArrayAsync(cancellationToken);

    var totalSales = salesRaw.Sum(r => r.Total);
    var salesCount = salesRaw.Sum(r => r.Count);
    var cashSales = salesRaw.Where(r => r.PaymentMethod == "Cash").Sum(r => r.Total);
    var transferSales = salesRaw.Where(r => r.PaymentMethod == "Transfer").Sum(r => r.Total);
    var cardSales = salesRaw.Where(r => r.PaymentMethod == "Card").Sum(r => r.Total);
    var creditSales = salesRaw.Where(r => r.PaymentMethod == "Credit").Sum(r => r.Total);

    // ── 2. Cost from sale items ───────────────────────────────────────────────
    var completedSales = dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId
                  && s.BranchId == branchId
                  && s.Status == SaleStatus.Completed
                  && s.CreatedAt >= dayStartOffset
                  && s.CreatedAt < dayEndOffset);

    var products = dbContext.Set<Product>()
      .AsNoTracking()
      .Where(p => p.BusinessId == businessId);

    var costResult = await (
      from sale in completedSales
      join item in dbContext.Set<SaleItem>().AsNoTracking() on sale.Id equals item.SaleId
      join product in products on item.ProductId equals product.Id into pg
      from product in pg.DefaultIfEmpty()
      group new
      {
        EffectiveCost = item.UnitCost != null
          ? item.Quantity * item.UnitCost.Value
          : item.Quantity * (product != null ? product.CostPrice : 0m),
        HasMissingCost = item.UnitCost == null && (product == null || product.CostPrice == 0m)
      } by 1
      into g
      select new
      {
        TotalCost = g.Sum(x => x.EffectiveCost),
        HasMissingCosts = g.Any(x => x.HasMissingCost)
      })
      .FirstOrDefaultAsync(cancellationToken);

    var totalCost = costResult?.TotalCost ?? 0m;
    var hasMissingCosts = costResult?.HasMissingCosts ?? false;

    // ── 3. Cash sessions (2-query strategy — SystemBalance is not EF-translatable) ──
    var sessionHeaders = await dbContext.Set<CashSession>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId
                  && s.BranchId == branchId
                  && s.OpenedAt >= dayStartOffset
                  && s.OpenedAt < dayEndOffset)
      .Select(s => new
      {
        s.Id,
        s.OpeningBalance,
        s.Status,
      })
      .ToArrayAsync(cancellationToken);

    var sessionIds = sessionHeaders.Select(s => s.Id).ToArray();

    var movementAggregates = sessionIds.Length > 0
      ? await dbContext.Set<CashMovement>()
          .AsNoTracking()
          .Where(m => sessionIds.Contains(m.CashSessionId))
          .GroupBy(m => new { m.CashSessionId, m.Type })
          .Select(g => new { g.Key.CashSessionId, g.Key.Type, Total = g.Sum(m => m.Amount) })
          .ToArrayAsync(cancellationToken)
      : [];

    // Compute opening balance of all sessions opened today
    var totalOpeningBalance = sessionHeaders.Sum(s => s.OpeningBalance);
    var hasOpenCashSessions = sessionHeaders.Any(s => s.Status == CashSessionStatus.Open);

    // ── 4. Operating expenses ─────────────────────────────────────────────────
    var totalExpenses = await dbContext.Set<OperatingExpense>()
      .AsNoTracking()
      .Where(e => e.BusinessId == businessId
                  && e.BranchId == branchId
                  && e.Status == OperatingExpenseStatus.Paid
                  && e.ExpenseDate >= dayStartOffset
                  && e.ExpenseDate < dayEndOffset)
      .SumAsync(e => e.Amount, cancellationToken);

    // ── 5. Credits (business-wide — no BranchId on CustomerCreditMovement) ───
    var creditMovements = await dbContext.Set<CustomerCreditMovement>()
      .AsNoTracking()
      .Where(m => m.BusinessId == businessId
                  && m.CreatedAt >= dayStartOffset
                  && m.CreatedAt < dayEndOffset)
      .Select(m => new { m.Type, m.Amount })
      .ToArrayAsync(cancellationToken);

    var newCreditsAmount = creditMovements
      .Where(m => m.Type == CustomerCreditMovementType.Debit)
      .Sum(m => m.Amount);
    var newCreditsCount = creditMovements
      .Count(m => m.Type == CustomerCreditMovementType.Debit);
    var creditPaymentsReceived = creditMovements
      .Where(m => m.Type == CustomerCreditMovementType.Payment)
      .Sum(m => m.Amount);

    return new DailyClosingData(
      TotalSales: totalSales,
      CashSales: cashSales,
      TransferSales: transferSales,
      CardSales: cardSales,
      CreditSales: creditSales,
      SalesCount: salesCount,
      CashSessionOpeningBalance: totalOpeningBalance,
      HasOpenCashSessions: hasOpenCashSessions,
      TotalExpenses: totalExpenses,
      TotalCost: totalCost,
      HasMissingCosts: hasMissingCosts,
      NewCreditsAmount: newCreditsAmount,
      NewCreditsCount: newCreditsCount,
      CreditPaymentsReceived: creditPaymentsReceived);
  }
}
