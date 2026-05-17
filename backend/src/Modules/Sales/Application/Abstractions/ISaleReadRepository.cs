using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ISaleReadRepository
{
  Task<SaleResponse?> GetAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<SaleResponse>> ListAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default);
}
