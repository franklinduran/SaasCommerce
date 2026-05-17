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

  Task<bool> HasSaleMovementAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid saleId,
    CancellationToken cancellationToken = default);

  Task<int> CountStockAsync(
    BusinessId businessId,
    BranchId? branchId,
    StockSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<StockItem>> ListStockAsync(
    BusinessId businessId,
    BranchId? branchId,
    StockSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<int> CountMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    InventoryMovementSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<InventoryMovement>> ListMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    InventoryMovementSearchCriteria criteria,
    CancellationToken cancellationToken = default);
}

public sealed record StockSearchCriteria(
  IReadOnlyCollection<Guid> ProductIds,
  bool RestrictToProductIds,
  bool LowStockOnly,
  bool OutOfStockOnly,
  IReadOnlyDictionary<Guid, decimal?> MinimumStockByProduct,
  int Page,
  int PageSize,
  StockSortOption SortBy,
  InventorySortDirection SortDirection);

public sealed record InventoryMovementSearchCriteria(
  Guid? ProductId,
  InventoryMovementReason? MovementType,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize,
  InventoryMovementSortOption SortBy,
  InventorySortDirection SortDirection);

public enum StockSortOption
{
  ProductId = 1,
  Quantity = 2,
  CreatedAt = 3
}

public enum InventoryMovementSortOption
{
  CreatedAt = 1,
  ProductId = 2,
  Quantity = 3
}

public enum InventorySortDirection
{
  Asc = 1,
  Desc = 2
}
