using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Settings.Application;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure;

public sealed class EfBusinessSettingsRepository(AppDbContext dbContext) : IBusinessSettingsRepository
{
  public Task<BusinessSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
    => dbContext.Set<BusinessSettings>()
      .FirstOrDefaultAsync(s => s.BusinessId == businessId, cancellationToken);

  public async Task UpsertAsync(BusinessSettings settings, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(settings);

    var tracked = dbContext.Set<BusinessSettings>().Local
      .FirstOrDefault(s => s.BusinessId == settings.BusinessId);

    if (tracked is null)
    {
      var exists = await dbContext.Set<BusinessSettings>()
        .AnyAsync(s => s.BusinessId == settings.BusinessId, cancellationToken);

      if (exists)
      {
        dbContext.Set<BusinessSettings>().Update(settings);
      }
      else
      {
        await dbContext.Set<BusinessSettings>().AddAsync(settings, cancellationToken);
      }
    }
  }
}
