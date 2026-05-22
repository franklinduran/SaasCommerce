using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Application.Branches;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Infrastructure.Persistence;

public sealed class EfBranchRepository(AppDbContext dbContext) : IBranchRepository
{
  public Task<Branch?> GetByIdAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Branch>()
      .SingleOrDefaultAsync(
        branch => branch.BusinessId == businessId && branch.Id == branchId,
        cancellationToken);

  public async Task<IReadOnlyCollection<Branch>> ListAsync(
    BusinessId businessId,
    bool? isActive,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(branch => branch.BusinessId == businessId);

    if (isActive.HasValue)
    {
      query = query.Where(branch => branch.IsActive == isActive.Value);
    }

    return await query
      .OrderBy(branch => branch.IsMain ? 0 : 1)
      .ThenBy(branch => branch.Name)
      .ToArrayAsync(cancellationToken);
  }

  public Task<bool> ExistsByCodeAsync(
    BusinessId businessId,
    string code,
    BranchId? excludeId,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(branch => branch.BusinessId == businessId && branch.Code == code);

    if (excludeId.HasValue)
    {
      query = query.Where(branch => branch.Id != excludeId.Value);
    }

    return query.AnyAsync(cancellationToken);
  }

  public Task<bool> ExistsByNameAsync(
    BusinessId businessId,
    string name,
    BranchId? excludeId,
    CancellationToken cancellationToken = default)
  {
    var normalizedName = name.Trim();

    var query = dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(branch => branch.BusinessId == businessId && branch.Name == normalizedName);

    if (excludeId.HasValue)
    {
      query = query.Where(branch => branch.Id != excludeId.Value);
    }

    return query.AnyAsync(cancellationToken);
  }

  public Task<int> CountActiveAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Branch>()
      .AsNoTracking()
      .CountAsync(branch => branch.BusinessId == businessId && branch.IsActive, cancellationToken);

  public Task<bool> HasMainBranchAsync(
    BusinessId businessId,
    BranchId? excludeId,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(branch => branch.BusinessId == businessId && branch.IsMain);

    if (excludeId.HasValue)
    {
      query = query.Where(branch => branch.Id != excludeId.Value);
    }

    return query.AnyAsync(cancellationToken);
  }

  public Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(branch);

    return dbContext.Set<Branch>().AddAsync(branch, cancellationToken).AsTask();
  }
}
