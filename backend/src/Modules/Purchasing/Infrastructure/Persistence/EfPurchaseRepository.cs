using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Infrastructure.Persistence;

public sealed class EfPurchaseRepository(AppDbContext dbContext) : IPurchaseRepository
{
  public Task<Purchase?> GetAsync(
    BusinessId businessId,
    Guid purchaseId,
    CancellationToken cancellationToken = default)
    => Purchases(businessId)
      .Include(purchase => purchase.Items)
      .SingleOrDefaultAsync(purchase => purchase.Id == purchaseId, cancellationToken);

  public Task AddAsync(Purchase purchase, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(purchase);

    return dbContext.Set<Purchase>().AddAsync(purchase, cancellationToken).AsTask();
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    PurchaseSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Purchases(businessId).AsNoTracking(), criteria)
      .CountAsync(cancellationToken);

  public async Task<decimal> SumTotalAsync(
    BusinessId businessId,
    PurchaseSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplyFilters(Purchases(businessId).AsNoTracking(), criteria)
      .SumAsync(purchase => (decimal?)purchase.Total, cancellationToken) ?? 0;

  public async Task<IReadOnlyCollection<Purchase>> ListAsync(
    BusinessId businessId,
    PurchaseSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplySorting(ApplyFilters(Purchases(businessId).AsNoTracking(), criteria), criteria)
      .Include(purchase => purchase.Items)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  private IQueryable<Purchase> Purchases(BusinessId businessId)
    => dbContext.Set<Purchase>()
      .Where(purchase => purchase.BusinessId == businessId);

  private IQueryable<Purchase> ApplyFilters(
    IQueryable<Purchase> query,
    PurchaseSearchCriteria criteria)
  {
    if (criteria.SupplierId.HasValue)
    {
      query = query.Where(purchase => purchase.SupplierId == criteria.SupplierId.Value);
    }

    if (criteria.BranchId.HasValue)
    {
      var branchId = new BranchId(criteria.BranchId.Value);
      query = query.Where(purchase => purchase.BranchId == branchId);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<PurchaseStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(purchase => purchase.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Query))
    {
      query = ApplyQueryFilter(query, criteria.Query);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(purchase => purchase.PurchaseDate >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(purchase => purchase.PurchaseDate <= criteria.DateTo.Value);
    }

    return query;
  }

  private IQueryable<Purchase> ApplyQueryFilter(IQueryable<Purchase> query, string queryValue)
  {
    var term = queryValue.Trim();
    var normalizedTerm = term.ToUpperInvariant();
    var supplierIds = dbContext.Set<Supplier>()
      .AsNoTracking()
      .Where(supplier => supplier.SearchName.Contains(normalizedTerm))
      .Select(supplier => supplier.Id);

    return query.Where(purchase =>
      purchase.SupplierInvoiceNumber != null && purchase.SupplierInvoiceNumber.Contains(term) ||
      supplierIds.Contains(purchase.SupplierId));
  }

  private static IOrderedQueryable<Purchase> ApplySorting(
    IQueryable<Purchase> query,
    PurchaseSearchCriteria criteria)
    => (criteria.SortBy, criteria.SortDirection) switch
    {
      (PurchaseSortOption.Total, PurchaseSortDirection.Asc) => query.OrderBy(purchase => purchase.Total),
      (PurchaseSortOption.Total, _) => query.OrderByDescending(purchase => purchase.Total),
      (PurchaseSortOption.CreatedAt, PurchaseSortDirection.Asc) => query.OrderBy(purchase => purchase.CreatedAt),
      (PurchaseSortOption.CreatedAt, _) => query.OrderByDescending(purchase => purchase.CreatedAt),
      (PurchaseSortOption.PurchaseDate, PurchaseSortDirection.Asc) => query.OrderBy(purchase => purchase.PurchaseDate),
      _ => query.OrderByDescending(purchase => purchase.PurchaseDate)
    };
}
