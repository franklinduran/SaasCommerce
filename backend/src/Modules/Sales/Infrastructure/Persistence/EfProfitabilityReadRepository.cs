using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Profitability;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfProfitabilityReadRepository(AppDbContext dbContext) : IProfitabilityReadRepository
{
  public async Task<ProfitabilitySummaryResponse> GetSummaryAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId,
    CancellationToken cancellationToken = default)
  {
    var salesQuery = CompletedSales(businessId, dateFrom, dateTo, branchId);

    // Total sales amount and count (directly from Sales table – no join, avoids fan-out)
    var salesAggregate = await salesQuery
      .GroupBy(_ => 1)
      .Select(g => new { TotalSales = g.Sum(s => s.Total), SalesCount = g.Count() })
      .FirstOrDefaultAsync(cancellationToken);

    var totalSales = salesAggregate?.TotalSales ?? 0m;
    var salesCount = salesAggregate?.SalesCount ?? 0;

    // Total cost from items (join to products for fallback cost)
    var costResult = await CostQuery(salesQuery, businessId)
      .GroupBy(_ => 1)
      .Select(g => new
      {
        TotalCost = g.Sum(x => x.EffectiveCost),
        WarningCount = g.Count(x => x.HasMissingCost)
      })
      .FirstOrDefaultAsync(cancellationToken);

    var totalCost = costResult?.TotalCost ?? 0m;
    var warningCount = costResult?.WarningCount ?? 0;

    // Operating expenses (paid only, within date range)
    var totalExpenses = await PaidExpensesQuery(businessId, dateFrom, dateTo, branchId)
      .SumAsync(e => e.Amount, cancellationToken);

    var grossProfit = ProfitabilityCalculator.GrossProfit(totalSales, totalCost);
    var netProfit = ProfitabilityCalculator.EstimatedNetProfit(grossProfit, totalExpenses);

    return new ProfitabilitySummaryResponse(
      DateFrom: dateFrom,
      DateTo: dateTo,
      TotalSales: totalSales,
      TotalCost: totalCost,
      GrossProfit: grossProfit,
      OperatingExpenses: totalExpenses,
      EstimatedNetProfit: netProfit,
      GrossMarginPercent: ProfitabilityCalculator.GrossMarginPercent(totalSales, grossProfit),
      NetMarginPercent: ProfitabilityCalculator.NetMarginPercent(totalSales, netProfit),
      SalesCount: salesCount,
      WarningCount: warningCount);
  }

  public async Task<IReadOnlyCollection<ProductProfitabilityResponse>> GetProductProfitabilityAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId,
    Guid? categoryId,
    CancellationToken cancellationToken = default)
  {
    var salesQuery = CompletedSales(businessId, dateFrom, dateTo, branchId);

    var products = dbContext.Set<Product>().AsNoTracking()
      .Where(p => p.BusinessId == businessId);

    if (categoryId.HasValue)
    {
      products = products.Where(p => p.CategoryId == categoryId.Value);
    }

    var rows = await (
      from sale in salesQuery
      join item in dbContext.Set<SaleItem>().AsNoTracking() on sale.Id equals item.SaleId
      join product in products on item.ProductId equals product.Id
      let effectiveUnitCost = item.UnitCost ?? product.CostPrice
      select new
      {
        item.ProductId,
        ProductName = product.Name,
        product.Sku,
        product.CategoryId,
        item.Quantity,
        SaleAmount = item.Quantity * item.UnitPrice,
        EffectiveCost = item.Quantity * effectiveUnitCost,
        HasMissingCost = item.UnitCost == null && product.CostPrice == 0m
      })
      .GroupBy(x => new { x.ProductId, x.ProductName, x.Sku, x.CategoryId })
      .Select(g => new
      {
        g.Key.ProductId,
        g.Key.ProductName,
        g.Key.Sku,
        g.Key.CategoryId,
        TotalQuantity = g.Sum(x => x.Quantity),
        TotalSales = g.Sum(x => x.SaleAmount),
        TotalCost = g.Sum(x => x.EffectiveCost),
        HasMissingCost = g.Any(x => x.HasMissingCost)
      })
      .OrderByDescending(x => x.TotalSales - x.TotalCost)
      .ToArrayAsync(cancellationToken);

    // Load category names separately for items that have categories
    var categoryIds = rows
      .Where(r => r.CategoryId.HasValue)
      .Select(r => r.CategoryId!.Value)
      .Distinct()
      .ToArray();

    var categoryNames = categoryIds.Length > 0
      ? await dbContext.Set<Category>()
          .AsNoTracking()
          .Where(c => categoryIds.Contains(c.Id))
          .Select(c => new { c.Id, c.Name })
          .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
      : [];

    return rows
      .Select(r =>
      {
        var grossProfit = ProfitabilityCalculator.GrossProfit(r.TotalSales, r.TotalCost);
        return new ProductProfitabilityResponse(
          ProductId: r.ProductId,
          ProductName: r.ProductName,
          Sku: r.Sku,
          TotalQuantity: r.TotalQuantity,
          TotalSales: r.TotalSales,
          TotalCost: r.TotalCost,
          GrossProfit: grossProfit,
          MarginPercent: ProfitabilityCalculator.ProductMarginPercent(r.TotalSales, r.TotalCost),
          HasMissingCost: r.HasMissingCost,
          CategoryId: r.CategoryId,
          CategoryName: r.CategoryId.HasValue
            ? categoryNames.GetValueOrDefault(r.CategoryId.Value)
            : null);
      })
      .ToArray();
  }

  public async Task<IReadOnlyCollection<BranchProfitabilityResponse>> GetBranchProfitabilityAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    CancellationToken cancellationToken = default)
  {
    // Step 1: Sales + count by branch
    var salesByBranch = await CompletedSales(businessId, dateFrom, dateTo, null)
      .GroupBy(s => s.BranchId.Value)
      .Select(g => new
      {
        BranchId = g.Key,
        TotalSales = g.Sum(s => s.Total),
        SalesCount = g.Count()
      })
      .ToArrayAsync(cancellationToken);

    if (salesByBranch.Length == 0)
    {
      return [];
    }

    // Step 2: Cost by branch
    var salesQueryAll = CompletedSales(businessId, dateFrom, dateTo, null);
    var products = dbContext.Set<Product>().AsNoTracking()
      .Where(p => p.BusinessId == businessId);

    var costByBranch = await (
      from sale in salesQueryAll
      join item in dbContext.Set<SaleItem>().AsNoTracking() on sale.Id equals item.SaleId
      join product in products on item.ProductId equals product.Id into pg
      from product in pg.DefaultIfEmpty()
      let fallbackCost = product == null ? 0m : product.CostPrice
      let effectiveUnitCost = item.UnitCost ?? fallbackCost
      group item.Quantity * effectiveUnitCost by sale.BranchId.Value
      into g
      select new { BranchId = g.Key, TotalCost = g.Sum() })
      .ToArrayAsync(cancellationToken);

    // Step 3: Operating expenses by branch
    var expensesByBranch = await dbContext.Set<OperatingExpense>()
      .AsNoTracking()
      .Where(e => e.BusinessId == businessId
                  && e.Status == OperatingExpenseStatus.Paid
                  && e.ExpenseDate >= dateFrom && e.ExpenseDate <= dateTo)
      .GroupBy(e => e.BranchId.Value)
      .Select(g => new { BranchId = g.Key, TotalExpenses = g.Sum(e => e.Amount) })
      .ToArrayAsync(cancellationToken);

    // Step 4: Branch names
    var branchIds = salesByBranch.Select(s => s.BranchId).ToArray();
    var branchIdObjects = branchIds.Select(id => new BranchId(id)).ToArray();
    var branchNames = await dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(b => b.BusinessId == businessId && branchIdObjects.Contains(b.Id))
      .Select(b => new { b.Id, b.Name })
      .ToDictionaryAsync(b => b.Id.Value, b => b.Name, cancellationToken);

    // Merge
    var costMap = costByBranch.ToDictionary(x => x.BranchId, x => x.TotalCost);
    var expenseMap = expensesByBranch.ToDictionary(x => x.BranchId, x => x.TotalExpenses);

    return salesByBranch
      .Select(s =>
      {
        var totalCost = costMap.GetValueOrDefault(s.BranchId);
        var expenses = expenseMap.GetValueOrDefault(s.BranchId);
        var grossProfit = ProfitabilityCalculator.GrossProfit(s.TotalSales, totalCost);
        var netProfit = ProfitabilityCalculator.EstimatedNetProfit(grossProfit, expenses);

        return new BranchProfitabilityResponse(
          BranchId: s.BranchId,
          BranchName: branchNames.GetValueOrDefault(s.BranchId, "Sucursal desconocida"),
          TotalSales: s.TotalSales,
          TotalCost: totalCost,
          GrossProfit: grossProfit,
          OperatingExpenses: expenses,
          EstimatedNetProfit: netProfit,
          NetMarginPercent: ProfitabilityCalculator.NetMarginPercent(s.TotalSales, netProfit),
          SalesCount: s.SalesCount);
      })
      .OrderByDescending(b => b.EstimatedNetProfit)
      .ToArray();
  }

  // ── Private helpers ──────────────────────────────────────────────────────────

  private IQueryable<Sale> CompletedSales(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId)
  {
    var query = dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId
                  && s.Status == SaleStatus.Completed
                  && s.CreatedAt >= dateFrom
                  && s.CreatedAt <= dateTo);

    if (branchId.HasValue)
    {
      var bId = new BranchId(branchId.Value);
      query = query.Where(s => s.BranchId == bId);
    }

    return query;
  }

  private IQueryable<OperatingExpense> PaidExpensesQuery(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId)
  {
    var query = dbContext.Set<OperatingExpense>()
      .AsNoTracking()
      .Where(e => e.BusinessId == businessId
                  && e.Status == OperatingExpenseStatus.Paid
                  && e.ExpenseDate >= dateFrom && e.ExpenseDate <= dateTo);

    if (branchId.HasValue)
    {
      var bId = new BranchId(branchId.Value);
      query = query.Where(e => e.BranchId == bId);
    }

    return query;
  }

  private IQueryable<CostRow> CostQuery(IQueryable<Sale> salesQuery, BusinessId businessId)
  {
    var products = dbContext.Set<Product>().AsNoTracking()
      .Where(p => p.BusinessId == businessId);

    return from sale in salesQuery
           join item in dbContext.Set<SaleItem>().AsNoTracking() on sale.Id equals item.SaleId
           join product in products on item.ProductId equals product.Id into pg
           from product in pg.DefaultIfEmpty()
           let fallbackCost = product == null ? 0m : product.CostPrice
           let effectiveUnitCost = item.UnitCost ?? fallbackCost
           select new CostRow
           {
             EffectiveCost = item.Quantity * effectiveUnitCost,
             HasMissingCost = item.UnitCost == null && (product == null || product.CostPrice == 0m)
           };
  }

  private sealed class CostRow
  {
    public decimal EffectiveCost { get; init; }
    public bool HasMissingCost { get; init; }
  }
}
