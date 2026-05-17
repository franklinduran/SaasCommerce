using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface ISaleEventWriter
{
  Task AddAsync(
    SaleCreatedEventV1 saleCreated,
    CancellationToken cancellationToken = default);

  Task<SaleCreatedEventV1> AddSaleCreatedAsync(
    Sale sale,
    Guid businessId,
    Guid branchId,
    Guid userId,
    CancellationToken cancellationToken = default);

  Task NotifyStatusChangedAsync(
    SaleCreatedEventV1 saleCreated,
    SaleStatus status,
    string? reason,
    CancellationToken cancellationToken = default);
}
