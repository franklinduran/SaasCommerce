using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Settings.Application;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure;

public sealed class EfBillingSettingsRepository(AppDbContext dbContext) : IBillingSettingsRepository
{
  public Task<BillingSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
    => dbContext.Set<BillingSettings>()
      .FirstOrDefaultAsync(s => s.BusinessId == businessId, cancellationToken);

  public async Task UpsertAsync(BillingSettings settings, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(settings);

    var tracked = dbContext.Set<BillingSettings>().Local
      .FirstOrDefault(s => s.BusinessId == settings.BusinessId);

    if (tracked is null)
    {
      var exists = await dbContext.Set<BillingSettings>()
        .AnyAsync(s => s.BusinessId == settings.BusinessId, cancellationToken);

      if (exists)
      {
        dbContext.Set<BillingSettings>().Update(settings);
      }
      else
      {
        await dbContext.Set<BillingSettings>().AddAsync(settings, cancellationToken);
      }
    }
  }
}
