using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.Modules.Reporting.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Reporting.Infrastructure.Persistence;

public sealed class EfReportsReadRepository(AppDbContext dbContext) : IReportsReadRepository
{
  private const int DashboardRecentItemCount = 10;

  public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
    BusinessId businessId,
    DateTimeOffset today,
    CancellationToken cancellationToken = default)
  {
    var todayEnd = today.AddDays(1).AddTicks(-1);

    var salesToday = await dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId &&
                  s.Status == SaleStatus.Completed &&
                  s.CompletedAt >= today &&
                  s.CompletedAt <= todayEnd)
      .GroupBy(_ => 1)
      .Select(g => new { Count = g.Count(), Total = g.Sum(s => s.Total) })
      .FirstOrDefaultAsync(cancellationToken);

    var invoicesToday = await dbContext.Set<Invoice>()
      .AsNoTracking()
      .Where(i => i.BusinessId == businessId &&
                  i.Status == InvoiceStatus.Issued &&
                  i.CreatedAt >= today &&
                  i.CreatedAt <= todayEnd)
      .GroupBy(_ => 1)
      .Select(g => new { Count = g.Count(), Total = g.Sum(i => i.Total) })
      .FirstOrDefaultAsync(cancellationToken);

    var receivables = await dbContext.Set<CustomerCreditAccount>()
      .AsNoTracking()
      .Where(c => c.BusinessId == businessId && c.CurrentBalance > 0)
      .GroupBy(_ => 1)
      .Select(g => new { CustomerCount = g.Count(), TotalPending = g.Sum(c => c.CurrentBalance) })
      .FirstOrDefaultAsync(cancellationToken);

    var lowStockCount = await (
      from stock in dbContext.Set<StockItem>().AsNoTracking()
      join product in dbContext.Set<Product>().AsNoTracking()
        on stock.ProductId equals product.Id
      where stock.BusinessId == businessId &&
            product.BusinessId == businessId &&
            product.TrackInventory &&
            product.MinimumStock.HasValue &&
            stock.Quantity <= product.MinimumStock!.Value
      select stock.Id)
      .CountAsync(cancellationToken);

    var recentSales = await dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId)
      .OrderByDescending(s => s.CreatedAt)
      .Take(DashboardRecentItemCount)
      .Select(s => new DashboardRecentSaleDto(
        s.Id,
        s.Id.ToString().Substring(0, 8).ToUpperInvariant(),
        s.Status.ToString(),
        s.PaymentMethod,
        s.Total,
        s.CreatedAt))
      .ToArrayAsync(cancellationToken);

    var recentInvoices = await dbContext.Set<Invoice>()
      .AsNoTracking()
      .Where(i => i.BusinessId == businessId)
      .OrderByDescending(i => i.CreatedAt)
      .Take(DashboardRecentItemCount)
      .Select(i => new DashboardRecentInvoiceDto(
        i.Id,
        i.InvoiceNumber,
        i.Status.ToString(),
        i.Total,
        i.CreatedAt))
      .ToArrayAsync(cancellationToken);

    var recentPurchases = await (
      from purchase in dbContext.Set<Purchase>().AsNoTracking()
      join supplier in dbContext.Set<Supplier>().AsNoTracking()
        on purchase.SupplierId equals supplier.Id into supplierJoin
      from supplier in supplierJoin.DefaultIfEmpty()
      where purchase.BusinessId == businessId
      orderby purchase.CreatedAt descending
      select new DashboardRecentPurchaseDto(
        purchase.Id,
        supplier != null ? supplier.Name : null,
        purchase.Status.ToString(),
        purchase.Total,
        purchase.CreatedAt))
      .Take(DashboardRecentItemCount)
      .ToArrayAsync(cancellationToken);

    return new DashboardSummaryResponse(
      new DashboardSalesTodayDto(salesToday?.Count ?? 0, salesToday?.Total ?? 0),
      new DashboardInvoicesTodayDto(invoicesToday?.Count ?? 0, invoicesToday?.Total ?? 0),
      new DashboardReceivablesDto(receivables?.CustomerCount ?? 0, receivables?.TotalPending ?? 0),
      new DashboardLowStockDto(lowStockCount),
      recentSales,
      recentInvoices,
      recentPurchases);
  }

  public async Task<SalesReportResponse> GetSalesReportAsync(
    BusinessId businessId,
    SalesReportCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId);

    query = ApplySalesFilters(query, criteria);

    var totalItems = await query.CountAsync(cancellationToken);

    var items = await (
      from sale in query
      join customer in dbContext.Set<Customer>().AsNoTracking()
        on sale.CustomerId equals customer.Id into customerJoin
      from customer in customerJoin.DefaultIfEmpty()
      join branch in dbContext.Set<Branch>().AsNoTracking()
        on sale.BranchId equals branch.Id into branchJoin
      from branch in branchJoin.DefaultIfEmpty()
      orderby sale.CreatedAt descending
      select new SalesReportItemDto(
        sale.Id,
        sale.Id.ToString().Substring(0, 8).ToUpperInvariant(),
        customer != null ? customer.FullName : null,
        branch != null ? branch.Name : null,
        sale.Status.ToString(),
        sale.PaymentMethod,
        sale.Total,
        sale.CreatedAt,
        sale.CompletedAt))
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

    var summaryQuery = dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(s => s.BusinessId == businessId);

    summaryQuery = ApplySalesFilters(summaryQuery, criteria);

    var summary = await summaryQuery
      .GroupBy(_ => 1)
      .Select(g => new { Count = g.Count(), Total = g.Sum(s => s.Total) })
      .FirstOrDefaultAsync(cancellationToken);

    var totalAmount = summary?.Total ?? 0;
    var count = summary?.Count ?? 0;
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)criteria.PageSize);

    return new SalesReportResponse(
      items,
      new SalesReportSummaryDto(count, totalAmount, count > 0 ? totalAmount / count : 0),
      criteria.Page,
      criteria.PageSize,
      totalItems,
      totalPages,
      criteria.Page > 1,
      totalPages > criteria.Page);
  }

  public async Task<InvoiceReportResponse> GetInvoiceReportAsync(
    BusinessId businessId,
    InvoiceReportCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<Invoice>()
      .AsNoTracking()
      .Where(i => i.BusinessId == businessId);

    query = ApplyInvoiceFilters(query, criteria);

    var totalItems = await query.CountAsync(cancellationToken);

    var items = await (
      from invoice in query
      join customer in dbContext.Set<Customer>().AsNoTracking()
        on invoice.CustomerId equals customer.Id into customerJoin
      from customer in customerJoin.DefaultIfEmpty()
      orderby invoice.CreatedAt descending
      select new InvoiceReportItemDto(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.SaleId,
        customer != null ? customer.FullName : null,
        invoice.Status.ToString(),
        invoice.Subtotal,
        invoice.TaxTotal,
        invoice.Total,
        invoice.CreatedAt))
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

    var summaryQuery = dbContext.Set<Invoice>()
      .AsNoTracking()
      .Where(i => i.BusinessId == businessId);

    summaryQuery = ApplyInvoiceFilters(summaryQuery, criteria);

    var summary = await summaryQuery
      .GroupBy(_ => 1)
      .Select(g => new { Count = g.Count(), Total = g.Sum(i => i.Total) })
      .FirstOrDefaultAsync(cancellationToken);

    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)criteria.PageSize);

    return new InvoiceReportResponse(
      items,
      new InvoiceReportSummaryDto(summary?.Count ?? 0, summary?.Total ?? 0),
      criteria.Page,
      criteria.PageSize,
      totalItems,
      totalPages,
      criteria.Page > 1,
      totalPages > criteria.Page);
  }

  public async Task<AccountsReceivableReportResponse> GetAccountsReceivableReportAsync(
    BusinessId businessId,
    AccountsReceivableCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query =
      from credit in dbContext.Set<CustomerCreditAccount>().AsNoTracking()
      join customer in dbContext.Set<Customer>().AsNoTracking()
        on credit.CustomerId equals customer.Id
      where credit.BusinessId == businessId && credit.CurrentBalance > 0
      select new { credit, customer };

    if (criteria.CustomerId.HasValue)
    {
      query = query.Where(x => x.credit.CustomerId == criteria.CustomerId.Value);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<CustomerCreditStatus>(criteria.Status, true, out var creditStatus))
    {
      query = query.Where(x => x.credit.Status == creditStatus);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(x => x.credit.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(x => x.credit.CreatedAt <= criteria.DateTo.Value);
    }

    var totalItems = await query.CountAsync(cancellationToken);

    var items = await query
      .OrderByDescending(x => x.credit.CurrentBalance)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .Select(x => new AccountsReceivableItemDto(
        x.customer.Id,
        x.customer.FullName,
        x.customer.Phone,
        x.credit.CreditLimit,
        x.credit.CurrentBalance,
        x.credit.Status.ToString(),
        x.credit.CreatedAt))
      .ToArrayAsync(cancellationToken);

    var summaryData = await dbContext.Set<CustomerCreditAccount>()
      .AsNoTracking()
      .Where(c => c.BusinessId == businessId && c.CurrentBalance > 0)
      .GroupBy(_ => 1)
      .Select(g => new
      {
        Count = g.Count(),
        TotalPending = g.Sum(c => c.CurrentBalance),
        MaxDebt = g.Max(c => c.CurrentBalance),
      })
      .FirstOrDefaultAsync(cancellationToken);

    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)criteria.PageSize);

    return new AccountsReceivableReportResponse(
      items,
      new AccountsReceivableSummaryDto(
        summaryData?.Count ?? 0,
        summaryData?.TotalPending ?? 0,
        summaryData?.MaxDebt ?? 0),
      criteria.Page,
      criteria.PageSize,
      totalItems,
      totalPages,
      criteria.Page > 1,
      totalPages > criteria.Page);
  }

  public async Task<LowStockReportResponse> GetLowStockReportAsync(
    BusinessId businessId,
    LowStockCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query =
      from stock in dbContext.Set<StockItem>().AsNoTracking()
      join product in dbContext.Set<Product>().AsNoTracking()
        on stock.ProductId equals product.Id
      join branch in dbContext.Set<Branch>().AsNoTracking()
        on stock.BranchId equals branch.Id into branchJoin
      from branch in branchJoin.DefaultIfEmpty()
      join category in dbContext.Set<Category>().AsNoTracking()
        on product.CategoryId equals category.Id into categoryJoin
      from category in categoryJoin.DefaultIfEmpty()
      join lastMovement in (
        dbContext.Set<InventoryMovement>().AsNoTracking()
          .GroupBy(m => m.ProductId)
          .Select(g => new { ProductId = g.Key, LastAt = g.Max(m => m.CreatedAt) }))
        on stock.ProductId equals lastMovement.ProductId into lastMovementJoin
      from lastMovement in lastMovementJoin.DefaultIfEmpty()
      where stock.BusinessId == businessId &&
            product.BusinessId == businessId &&
            product.TrackInventory &&
            product.MinimumStock.HasValue &&
            stock.Quantity <= product.MinimumStock!.Value
      select new
      {
        stock,
        product,
        branch,
        category,
        lastMovementAt = lastMovement != null ? lastMovement.LastAt : (DateTimeOffset?)null,
      };

    if (criteria.BranchId.HasValue)
    {
      var targetBranch = new BranchId(criteria.BranchId.Value);
      query = query.Where(x => x.stock.BranchId == targetBranch);
    }

    if (criteria.CategoryId.HasValue)
    {
      query = query.Where(x => x.product.CategoryId == criteria.CategoryId.Value);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Search))
    {
      var s = criteria.Search.Trim();
      query = query.Where(x =>
        x.product.Name.Contains(s) ||
        (x.product.Sku != null && x.product.Sku.Contains(s)));
    }

    var totalItems = await query.CountAsync(cancellationToken);

    var items = await query
      .OrderBy(x => x.stock.Quantity)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .Select(x => new LowStockReportItemDto(
        x.product.Id,
        x.product.Name,
        x.product.Sku,
        x.category != null ? x.category.Name : null,
        x.branch != null ? x.branch.Name : null,
        x.stock.Quantity,
        x.product.MinimumStock ?? 0,
        (x.product.MinimumStock ?? 0) - x.stock.Quantity,
        x.product.SalePrice,
        x.lastMovementAt))
      .ToArrayAsync(cancellationToken);

    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)criteria.PageSize);

    return new LowStockReportResponse(
      items,
      criteria.Page,
      criteria.PageSize,
      totalItems,
      totalPages,
      criteria.Page > 1,
      totalPages > criteria.Page);
  }

  public async Task<PurchaseReportResponse> GetPurchaseReportAsync(
    BusinessId businessId,
    PurchaseReportCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query =
      from purchase in dbContext.Set<Purchase>().AsNoTracking()
      join supplier in dbContext.Set<Supplier>().AsNoTracking()
        on purchase.SupplierId equals supplier.Id into supplierJoin
      from supplier in supplierJoin.DefaultIfEmpty()
      where purchase.BusinessId == businessId
      select new { purchase, supplier };

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(x => x.purchase.PurchaseDate >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(x => x.purchase.PurchaseDate <= criteria.DateTo.Value);
    }

    if (criteria.SupplierId.HasValue)
    {
      query = query.Where(x => x.purchase.SupplierId == criteria.SupplierId.Value);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<PurchaseStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(x => x.purchase.Status == status);
    }

    if (criteria.BranchId.HasValue)
    {
      var targetBranch = new BranchId(criteria.BranchId.Value);
      query = query.Where(x => x.purchase.BranchId == targetBranch);
    }

    var totalItems = await query.CountAsync(cancellationToken);

    var purchaseIds = await query
      .OrderByDescending(x => x.purchase.PurchaseDate)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .Select(x => x.purchase.Id)
      .ToArrayAsync(cancellationToken);

    var itemCounts = await dbContext.Set<PurchaseItem>()
      .AsNoTracking()
      .Where(pi => purchaseIds.Contains(pi.PurchaseId))
      .GroupBy(pi => pi.PurchaseId)
      .Select(g => new { PurchaseId = g.Key, Count = g.Count() })
      .ToDictionaryAsync(x => x.PurchaseId, x => x.Count, cancellationToken);

    var pageItems = await query
      .Where(x => purchaseIds.Contains(x.purchase.Id))
      .OrderByDescending(x => x.purchase.PurchaseDate)
      .Select(x => new PurchaseReportItemDto(
        x.purchase.Id,
        x.supplier != null ? x.supplier.Name : null,
        x.purchase.SupplierInvoiceNumber,
        x.purchase.Status.ToString(),
        0,
        x.purchase.Total,
        x.purchase.PurchaseDate,
        x.purchase.CreatedAt))
      .ToArrayAsync(cancellationToken);

    var itemsWithCounts = pageItems
      .Select(item => item with { ItemCount = itemCounts.GetValueOrDefault(item.PurchaseId, 0) })
      .ToArray();

    var summaryQuery =
      from purchase in dbContext.Set<Purchase>().AsNoTracking()
      where purchase.BusinessId == businessId
      select purchase;

    if (criteria.DateFrom.HasValue)
    {
      summaryQuery = summaryQuery.Where(p => p.PurchaseDate >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      summaryQuery = summaryQuery.Where(p => p.PurchaseDate <= criteria.DateTo.Value);
    }

    if (criteria.SupplierId.HasValue)
    {
      summaryQuery = summaryQuery.Where(p => p.SupplierId == criteria.SupplierId.Value);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<PurchaseStatus>(criteria.Status, true, out var purchaseStatus))
    {
      summaryQuery = summaryQuery.Where(p => p.Status == purchaseStatus);
    }

    if (criteria.BranchId.HasValue)
    {
      var targetBranch = new BranchId(criteria.BranchId.Value);
      summaryQuery = summaryQuery.Where(p => p.BranchId == targetBranch);
    }

    var summary = await summaryQuery
      .GroupBy(_ => 1)
      .Select(g => new { Count = g.Count(), Total = g.Sum(p => p.Total) })
      .FirstOrDefaultAsync(cancellationToken);

    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)criteria.PageSize);

    return new PurchaseReportResponse(
      itemsWithCounts,
      new PurchaseReportSummaryDto(summary?.Count ?? 0, summary?.Total ?? 0),
      criteria.Page,
      criteria.PageSize,
      totalItems,
      totalPages,
      criteria.Page > 1,
      totalPages > criteria.Page);
  }

  private static IQueryable<Sale> ApplySalesFilters(
    IQueryable<Sale> query,
    SalesReportCriteria criteria)
  {
    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(s => s.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(s => s.CreatedAt <= criteria.DateTo.Value);
    }

    if (criteria.BranchId.HasValue)
    {
      var targetBranch = new BranchId(criteria.BranchId.Value);
      query = query.Where(s => s.BranchId == targetBranch);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<SaleStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(s => s.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(criteria.PaymentMethod))
    {
      query = query.Where(s => s.PaymentMethod == criteria.PaymentMethod);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Search))
    {
      var term = criteria.Search.Trim();
      query = query.Where(s => s.Id.ToString().Contains(term));
    }

    return query;
  }

  private static IQueryable<Invoice> ApplyInvoiceFilters(
    IQueryable<Invoice> query,
    InvoiceReportCriteria criteria)
  {
    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(i => i.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(i => i.CreatedAt <= criteria.DateTo.Value);
    }

    if (criteria.CustomerId.HasValue)
    {
      query = query.Where(i => i.CustomerId == criteria.CustomerId.Value);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<InvoiceStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(i => i.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Search))
    {
      var term = criteria.Search.Trim();
      query = query.Where(i => i.InvoiceNumber.Contains(term));
    }

    return query;
  }
}
