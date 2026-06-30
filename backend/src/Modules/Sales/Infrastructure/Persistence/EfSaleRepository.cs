using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfSaleRepository(AppDbContext dbContext) : ISaleRepository
{
  public Task<Sale?> GetAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Sale>()
      .Include(sale => sale.Items)
      .SingleOrDefaultAsync(
        sale => sale.BusinessId == businessId && sale.Id == saleId,
        cancellationToken);

  public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(sale);

    await dbContext.Set<Sale>().AddAsync(sale, cancellationToken);
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Sales(businessId).AsNoTracking(), criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<Sale>> ListAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplySorting(ApplyFilters(Sales(businessId).AsNoTracking(), criteria), criteria)
      .Include(sale => sale.Items)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  private IQueryable<Sale> Sales(BusinessId businessId)
    => dbContext.Set<Sale>()
      .Where(sale => sale.BusinessId == businessId);

  private static IQueryable<Sale> ApplyFilters(
    IQueryable<Sale> query,
    SaleSearchCriteria criteria)
  {
    if (criteria.BranchId.HasValue)
    {
      var branchId = new BranchId(criteria.BranchId.Value);
      query = query.Where(sale => sale.BranchId == branchId);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<SaleStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(sale => sale.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(criteria.PaymentMethod))
    {
      query = query.Where(sale => sale.PaymentMethod == criteria.PaymentMethod);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(sale => sale.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(sale => sale.CreatedAt <= criteria.DateTo.Value);
    }

    return query;
  }

  private static IOrderedQueryable<Sale> ApplySorting(
    IQueryable<Sale> query,
    SaleSearchCriteria criteria)
    => (criteria.SortBy, criteria.SortDirection) switch
    {
      (SaleSortOption.Total, SaleSortDirection.Asc) => query.OrderBy(sale => sale.Total),
      (SaleSortOption.Total, _) => query.OrderByDescending(sale => sale.Total),
      (SaleSortOption.CreatedAt, SaleSortDirection.Asc) => query.OrderBy(sale => sale.CreatedAt),
      _ => query.OrderByDescending(sale => sale.CreatedAt)
    };
}
