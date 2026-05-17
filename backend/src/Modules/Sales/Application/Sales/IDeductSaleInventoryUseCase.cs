using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface IDeductSaleInventoryUseCase
{
  Task<Result> ExecuteAsync(
    InventoryDeductionRequestedEventV1 inventoryDeductionRequested,
    CancellationToken cancellationToken = default);
}
