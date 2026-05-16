using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Tenancy.Domain;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfAccountBusinessRepository(AppDbContext dbContext) : IAccountBusinessRepository
{
  public Task<bool> ExistsByIdentificationAsync(
    string identificationType,
    string identificationNumber,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Business>()
      .AnyAsync(
        business =>
          business.IdentificationType != null &&
          business.IdentificationType.ToString() == identificationType &&
          business.IdentificationNumber == identificationNumber,
        cancellationToken);

  public Task AddAsync(Business business, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(business);

    return dbContext.Set<Business>().AddAsync(business, cancellationToken).AsTask();
  }
}
