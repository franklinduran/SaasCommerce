using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public interface IBillingSettingsRepository
{
  Task<BillingSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default);

  Task UpsertAsync(BillingSettings settings, CancellationToken cancellationToken = default);
}
