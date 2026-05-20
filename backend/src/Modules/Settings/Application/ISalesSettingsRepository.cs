using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public interface ISalesSettingsRepository
{
  Task<SalesSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default);

  Task UpsertAsync(SalesSettings settings, CancellationToken cancellationToken = default);
}
