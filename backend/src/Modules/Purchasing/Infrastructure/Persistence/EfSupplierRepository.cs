using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Infrastructure.Persistence;

public sealed class EfSupplierRepository(AppDbContext dbContext) : ISupplierRepository
{
  public Task<Supplier?> GetAsync(
    BusinessId businessId,
    Guid supplierId,
    CancellationToken cancellationToken = default)
    => Suppliers(businessId)
      .SingleOrDefaultAsync(supplier => supplier.Id == supplierId, cancellationToken);

  public Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(supplier);

    return dbContext.Set<Supplier>().AddAsync(supplier, cancellationToken).AsTask();
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    SupplierSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Suppliers(businessId).AsNoTracking(), criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<Supplier>> ListAsync(
    BusinessId businessId,
    SupplierSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplySorting(ApplyFilters(Suppliers(businessId).AsNoTracking(), criteria), criteria)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  public async Task<IReadOnlyDictionary<Guid, Supplier>> ListByIdsAsync(
    BusinessId businessId,
    IReadOnlyCollection<Guid> supplierIds,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(supplierIds);

    if (supplierIds.Count == 0)
    {
      return new Dictionary<Guid, Supplier>();
    }

    return await Suppliers(businessId)
      .AsNoTracking()
      .Where(supplier => supplierIds.Contains(supplier.Id))
      .ToDictionaryAsync(supplier => supplier.Id, cancellationToken);
  }

  private IQueryable<Supplier> Suppliers(BusinessId businessId)
    => dbContext.Set<Supplier>()
      .Where(supplier => supplier.BusinessId == businessId);

  private static IQueryable<Supplier> ApplyFilters(
    IQueryable<Supplier> query,
    SupplierSearchCriteria criteria)
  {
    if (!string.IsNullOrWhiteSpace(criteria.Query))
    {
      var term = criteria.Query.Trim();
      var normalizedTerm = term.ToUpperInvariant();
      query = query.Where(supplier =>
        supplier.Name.Contains(term) ||
        supplier.SearchName.Contains(normalizedTerm) ||
        supplier.Rnc != null && supplier.Rnc.Contains(term) ||
        supplier.Phone != null && supplier.Phone.Contains(term) ||
        supplier.Email != null && supplier.Email.Contains(term));
    }

    if (criteria.IsActive.HasValue)
    {
      query = query.Where(supplier => supplier.IsActive == criteria.IsActive.Value);
    }

    return query;
  }

  private static IOrderedQueryable<Supplier> ApplySorting(
    IQueryable<Supplier> query,
    SupplierSearchCriteria criteria)
    => (criteria.SortBy, criteria.SortDirection) switch
    {
      (SupplierSortOption.CreatedAt, SupplierSortDirection.Desc) =>
        query.OrderByDescending(supplier => supplier.CreatedAt),
      (SupplierSortOption.CreatedAt, _) =>
        query.OrderBy(supplier => supplier.CreatedAt),
      (SupplierSortOption.Name, SupplierSortDirection.Desc) =>
        query.OrderByDescending(supplier => supplier.Name),
      _ => query.OrderBy(supplier => supplier.Name)
    };
}
