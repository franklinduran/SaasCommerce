using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfDailyClosingRepository(AppDbContext dbContext) : IDailyClosingRepository
{
  public async Task<DailyClosing?> GetByIdAsync(
    Guid id,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    return await dbContext.Set<DailyClosing>()
      .Include(dc => dc.Alerts)
      .FirstOrDefaultAsync(
        dc => dc.Id == id && dc.BusinessId == businessId,
        cancellationToken);
  }

  public async Task<DailyClosing?> GetByDateAndBranchAsync(
    BusinessId businessId,
    BranchId branchId,
    DateOnly date,
    CancellationToken cancellationToken = default)
  {
    return await dbContext.Set<DailyClosing>()
      .Include(dc => dc.Alerts)
      .FirstOrDefaultAsync(
        dc => dc.BusinessId == businessId
              && dc.BranchId == branchId
              && dc.ClosingDate == date,
        cancellationToken);
  }

  public async Task AddAsync(DailyClosing closing, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(closing);
    await dbContext.Set<DailyClosing>().AddAsync(closing, cancellationToken);
  }

  public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    => dbContext.SaveChangesAsync(cancellationToken);
}
