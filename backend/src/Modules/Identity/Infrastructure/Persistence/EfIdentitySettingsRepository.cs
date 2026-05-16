using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Settings;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfIdentitySettingsRepository(AppDbContext dbContext) : IIdentitySettingsRepository
{
  public Task<User?> GetUserAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<User>()
      .Include(user => user.Roles)
      .Include(user => user.RefreshTokens)
      .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

  public Task<Business?> GetBusinessAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Business>()
      .Include(business => business.Phones)
      .SingleOrDefaultAsync(business => business.Id == businessId, cancellationToken);

  public Task<Branch?> GetBranchAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Branch>()
      .SingleOrDefaultAsync(
        branch => branch.Id == branchId && branch.BusinessId == businessId,
        cancellationToken);

  public Task<bool> ExistsBusinessIdentificationAsync(
    BusinessIdentificationType identificationType,
    string identificationNumber,
    BusinessId exceptBusinessId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Business>()
      .AnyAsync(
        business =>
          business.Id != exceptBusinessId &&
          business.IdentificationType == identificationType &&
          business.IdentificationNumber == identificationNumber,
        cancellationToken);
}
