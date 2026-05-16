using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Abstractions;

public interface IInventoryRepository
{
  Task<StockItem?> GetStockItemAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid productId,
    CancellationToken cancellationToken = default);

  Task AddStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default);

  Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default);

  Task<int> CountStockAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<StockItem>> ListStockAsync(
    BusinessId businessId,
    BranchId branchId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

  Task<int> CountMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid? productId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<InventoryMovement>> ListMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid? productId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);
}
