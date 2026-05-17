using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Abstractions;

public interface IPurchaseMovementReader
{
  Task<IReadOnlyCollection<PurchaseInventoryMovementResponse>> ListByPurchaseAsync(
    BusinessId businessId,
    Guid purchaseId,
    CancellationToken cancellationToken = default);
}
