using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ISaleReturnReadRepository
{
  Task<SaleReturnResponse?> GetAsync(
    BusinessId businessId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<SaleReturnResponse>> ListBySaleAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);
}
