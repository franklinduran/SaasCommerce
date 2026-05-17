using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ISaleRepository
{
  Task<Sale?> GetAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
}
