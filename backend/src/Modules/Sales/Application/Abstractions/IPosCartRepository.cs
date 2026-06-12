using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface IPosCartRepository
{
  Task<PosCart?> GetAsync(BusinessId businessId, Guid userId, CancellationToken ct = default);
  Task AddAsync(PosCart cart, CancellationToken ct = default);
  Task RemoveItemAsync(PosCartItem item, CancellationToken ct = default);
}
