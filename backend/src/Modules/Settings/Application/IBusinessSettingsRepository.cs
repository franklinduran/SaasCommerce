using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public interface IBusinessSettingsRepository
{
  Task<BusinessSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default);

  Task UpsertAsync(BusinessSettings settings, CancellationToken cancellationToken = default);
}
