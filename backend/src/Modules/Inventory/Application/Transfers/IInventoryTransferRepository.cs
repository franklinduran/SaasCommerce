using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public interface IInventoryTransferRepository
{
  Task<InventoryTransfer?> GetByIdAsync(
    BusinessId businessId,
    Guid transferId,
    CancellationToken cancellationToken = default);

  Task<(IReadOnlyCollection<InventoryTransfer> Items, int Total)> ListAsync(
    BusinessId businessId,
    Guid? sourceBranchId,
    Guid? targetBranchId,
    InventoryTransferStatus? status,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

  Task AddAsync(InventoryTransfer transfer, CancellationToken cancellationToken = default);
}
