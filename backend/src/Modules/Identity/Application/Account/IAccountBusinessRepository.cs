using SaasCommerce.Modules.Tenancy.Domain;

namespace SaasCommerce.Modules.Identity.Application.Account;

public interface IAccountBusinessRepository
{
  Task<bool> ExistsByIdentificationAsync(
    string identificationType,
    string identificationNumber,
    CancellationToken cancellationToken = default);

  Task AddAsync(Business business, CancellationToken cancellationToken = default);
}
