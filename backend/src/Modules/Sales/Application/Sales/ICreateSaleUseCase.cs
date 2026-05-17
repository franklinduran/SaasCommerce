using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface ICreateSaleUseCase
{
  Task<Result> ExecuteAsync(
    SaleCreatedEventV1 saleCreated,
    CancellationToken cancellationToken = default);
}
