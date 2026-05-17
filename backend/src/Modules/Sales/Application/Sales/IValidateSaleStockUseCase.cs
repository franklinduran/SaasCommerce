using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface IValidateSaleStockUseCase
{
  Task<Result> ExecuteAsync(
    StockValidationRequestedEventV1 stockValidationRequested,
    CancellationToken cancellationToken = default);
}
