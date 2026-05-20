using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Settings.Application;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure;

public sealed class EfInventorySettingsRepository(AppDbContext dbContext) : IInventorySettingsRepository
{
  public Task<InventorySettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
    => dbContext.Set<InventorySettings>()
      .FirstOrDefaultAsync(s => s.BusinessId == businessId, cancellationToken);

  public async Task UpsertAsync(InventorySettings settings, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(settings);

    var tracked = dbContext.Set<InventorySettings>().Local
      .FirstOrDefault(s => s.BusinessId == settings.BusinessId);

    if (tracked is null)
    {
      var exists = await dbContext.Set<InventorySettings>()
        .AnyAsync(s => s.BusinessId == settings.BusinessId, cancellationToken);

      if (exists)
      {
        dbContext.Set<InventorySettings>().Update(settings);
      }
      else
      {
        await dbContext.Set<InventorySettings>().AddAsync(settings, cancellationToken);
      }
    }
  }
}
