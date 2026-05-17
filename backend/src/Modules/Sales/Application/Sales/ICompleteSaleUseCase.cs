using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface ICompleteSaleUseCase
{
  Task<Result> ExecuteAsync(
    SaleCompletedEventV1 saleCompleted,
    CancellationToken cancellationToken = default);
}
