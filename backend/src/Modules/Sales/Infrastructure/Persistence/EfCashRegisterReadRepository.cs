using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfCashRegisterReadRepository(AppDbContext dbContext) : ICashRegisterReadRepository
{
  public Task<CashRegister?> GetAsync(BusinessId businessId, Guid cashRegisterId, CancellationToken ct)
    => dbContext.Set<CashRegister>()
      .AsNoTracking()
      .Include(r => r.Movements)
      .SingleOrDefaultAsync(r => r.BusinessId == businessId && r.Id == cashRegisterId, ct);

  public Task<CashRegister?> GetOpenRegisterAsync(BusinessId businessId, Guid userId, CancellationToken ct)
    => dbContext.Set<CashRegister>()
      .AsNoTracking()
      .Include(r => r.Movements)
      .SingleOrDefaultAsync(
        r => r.BusinessId == businessId &&
             r.UserId == userId &&
             r.Status == CashRegisterStatus.Open,
        ct);

  public async Task<IReadOnlyCollection<CashRegister>> ListAsync(
    BusinessId businessId,
    CashRegisterSearchCriteria criteria,
    CancellationToken ct)
    => await ApplyFilters(Registers(businessId), criteria)
      .Include(r => r.Movements)
      .OrderByDescending(r => r.OpenedAt)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(ct);

  public Task<int> CountAsync(BusinessId businessId, CashRegisterSearchCriteria criteria, CancellationToken ct)
    => ApplyFilters(Registers(businessId), criteria).CountAsync(ct);

  public async Task<IReadOnlyCollection<CashRegister>> GetDailySummaryAsync(
    BusinessId businessId,
    Guid? branchId,
    DateOnly summaryDate,
    CancellationToken ct)
  {
    var from = new DateTimeOffset(summaryDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    var to = from.AddDays(1);

    var query = Registers(businessId).Where(r => r.OpenedAt >= from && r.OpenedAt < to);

    if (branchId.HasValue)
    {
      query = query.Where(r => r.BranchId == new BranchId(branchId.Value));
    }

    return await query
      .Include(r => r.Movements)
      .OrderBy(r => r.OpenedAt)
      .ToArrayAsync(ct);
  }

  private IQueryable<CashRegister> Registers(BusinessId businessId)
    => dbContext.Set<CashRegister>()
      .AsNoTracking()
      .Where(r => r.BusinessId == businessId);

  private static IQueryable<CashRegister> ApplyFilters(
    IQueryable<CashRegister> query,
    CashRegisterSearchCriteria criteria)
  {
    if (criteria.BranchId.HasValue)
    {
      query = query.Where(r => r.BranchId == new BranchId(criteria.BranchId.Value));
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<CashRegisterStatus>(criteria.Status, ignoreCase: true, out var status))
    {
      query = query.Where(r => r.Status == status);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(r => r.OpenedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(r => r.OpenedAt <= criteria.DateTo.Value);
    }

    return query;
  }
}
