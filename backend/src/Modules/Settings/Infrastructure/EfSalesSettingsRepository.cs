using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Settings.Application;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure;

public sealed class EfSalesSettingsRepository(AppDbContext dbContext) : ISalesSettingsRepository
{
  public Task<SalesSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
    => dbContext.Set<SalesSettings>()
      .FirstOrDefaultAsync(s => s.BusinessId == businessId, cancellationToken);

  public async Task UpsertAsync(SalesSettings settings, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(settings);

    var tracked = dbContext.Set<SalesSettings>().Local
      .FirstOrDefault(s => s.BusinessId == settings.BusinessId);

    if (tracked is null)
    {
      var exists = await dbContext.Set<SalesSettings>()
        .AnyAsync(s => s.BusinessId == settings.BusinessId, cancellationToken);

      if (exists)
      {
        dbContext.Set<SalesSettings>().Update(settings);
      }
      else
      {
        await dbContext.Set<SalesSettings>().AddAsync(settings, cancellationToken);
      }
    }
  }
}
