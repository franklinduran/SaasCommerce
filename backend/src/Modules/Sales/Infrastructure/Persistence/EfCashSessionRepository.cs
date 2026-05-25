using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfCashSessionRepository(AppDbContext dbContext) : ICashSessionRepository
{
  public Task<CashSession?> GetAsync(
    BusinessId businessId,
    Guid cashSessionId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<CashSession>()
      .Include(cs => cs.Movements)
      .SingleOrDefaultAsync(
        cs => cs.BusinessId == businessId && cs.Id == cashSessionId,
        cancellationToken);

  public Task<CashSession?> GetOpenSessionAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<CashSession>()
      .Include(cs => cs.Movements)
      .SingleOrDefaultAsync(
        cs => cs.BusinessId == businessId &&
              cs.BranchId == branchId &&
              cs.Status == CashSessionStatus.Open,
        cancellationToken);

  public Task<bool> HasOpenSessionAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<CashSession>()
      .AnyAsync(
        cs => cs.BusinessId == businessId &&
              cs.BranchId == branchId &&
              cs.Status == CashSessionStatus.Open,
        cancellationToken);

  public async Task AddAsync(CashSession session, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(session);

    await dbContext.Set<CashSession>().AddAsync(session, cancellationToken);
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    CashSessionSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Sessions(businessId).AsNoTracking(), criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<CashSession>> ListAsync(
    BusinessId businessId,
    CashSessionSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplyFilters(Sessions(businessId).AsNoTracking(), criteria)
      .Include(cs => cs.Movements)
      .OrderByDescending(cs => cs.OpenedAt)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  public async Task<IReadOnlyCollection<CashSession>> ExportAllAsync(
    BusinessId businessId,
    DateTimeOffset? dateFrom,
    DateTimeOffset? dateTo,
    CancellationToken cancellationToken = default)
  {
    var baseQuery = Sessions(businessId).AsNoTracking();

    if (dateFrom.HasValue)
    {
      baseQuery = baseQuery.Where(cs => cs.OpenedAt >= dateFrom.Value);
    }

    if (dateTo.HasValue)
    {
      baseQuery = baseQuery.Where(cs => cs.OpenedAt <= dateTo.Value);
    }

    return await baseQuery
      .Include(cs => cs.Movements)
      .OrderBy(cs => cs.OpenedAt)
      .Take(10_000)
      .ToArrayAsync(cancellationToken);
  }

  private IQueryable<CashSession> Sessions(BusinessId businessId)
    => dbContext.Set<CashSession>()
      .Where(cs => cs.BusinessId == businessId);

  private static IQueryable<CashSession> ApplyFilters(
    IQueryable<CashSession> query,
    CashSessionSearchCriteria criteria)
  {
    if (criteria.BranchId.HasValue)
    {
      var branchId = new BranchId(criteria.BranchId.Value);
      query = query.Where(cs => cs.BranchId == branchId);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<CashSessionStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(cs => cs.Status == status);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(cs => cs.OpenedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(cs => cs.OpenedAt <= criteria.DateTo.Value);
    }

    return query;
  }
}
