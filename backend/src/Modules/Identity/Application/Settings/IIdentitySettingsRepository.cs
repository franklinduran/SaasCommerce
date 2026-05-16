using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public interface IIdentitySettingsRepository
{
  Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

  Task<Business?> GetBusinessAsync(BusinessId businessId, CancellationToken cancellationToken = default);

  Task<Branch?> GetBranchAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default);

  Task<bool> ExistsBusinessIdentificationAsync(
    BusinessIdentificationType identificationType,
    string identificationNumber,
    BusinessId exceptBusinessId,
    CancellationToken cancellationToken = default);
}
