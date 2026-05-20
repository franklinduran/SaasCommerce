using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public interface IInventorySettingsRepository
{
  Task<InventorySettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default);

  Task UpsertAsync(InventorySettings settings, CancellationToken cancellationToken = default);
}
