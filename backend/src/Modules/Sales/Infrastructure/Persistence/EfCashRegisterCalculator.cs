using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfCashRegisterCalculator(AppDbContext dbContext) : ICashRegisterCalculator
{
  public async Task<CashRegisterTotals> CalculateAsync(
    BusinessId businessId,
    BranchId branchId,
    DateTimeOffset openedAt,
    DateTimeOffset closedAt,
    CancellationToken ct)
  {
    // Sum completed sales by payment method in the register time window
    var salesQuery = dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s =>
        s.BusinessId == businessId &&
        s.BranchId == branchId &&
        s.Status == SaleStatus.Completed &&
        s.CompletedAt >= openedAt &&
        s.CompletedAt < closedAt);

    var cashSales = await salesQuery
      .Where(s => s.PaymentMethod == "Cash")
      .SumAsync(s => s.Total, ct);

    var cardSales = await salesQuery
      .Where(s => s.PaymentMethod == "Card")
      .SumAsync(s => s.Total, ct);

    var transferSales = await salesQuery
      .Where(s => s.PaymentMethod == "Transfer")
      .SumAsync(s => s.Total, ct);

    var creditSales = await salesQuery
      .Where(s => s.PaymentMethod == "Credit")
      .SumAsync(s => s.Total, ct);

    // Sum approved cash returns (returns on cash sales) in the time window
    var cashReturns = await dbContext.Set<SaleReturn>()
      .AsNoTracking()
      .Join(
        dbContext.Set<Sale>().AsNoTracking(),
        sr => sr.SaleId,
        s => s.Id,
        (sr, s) => new { Return = sr, Sale = s })
      .Where(x =>
        x.Return.BusinessId == businessId &&
        x.Return.BranchId == branchId &&
        x.Return.Status == SaleReturnStatus.Approved &&
        x.Sale.PaymentMethod == "Cash" &&
        x.Return.ApprovedAt >= openedAt &&
        x.Return.ApprovedAt < closedAt)
      .SumAsync(x => (decimal?)x.Return.Total, ct) ?? 0m;

    return new CashRegisterTotals(cashSales, cardSales, transferSales, creditSales, cashReturns);
  }
}
