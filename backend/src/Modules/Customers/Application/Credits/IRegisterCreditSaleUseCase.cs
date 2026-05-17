using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public interface IRegisterCreditSaleUseCase
{
  Task<Result> ExecuteAsync(
    SaleCompletedEventV1 saleCompleted,
    CancellationToken cancellationToken = default);
}
