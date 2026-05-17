using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface IFailSaleUseCase
{
  Task<Result> ExecuteAsync(
    SaleFailedEventV1 saleFailed,
    CancellationToken cancellationToken = default);
}
