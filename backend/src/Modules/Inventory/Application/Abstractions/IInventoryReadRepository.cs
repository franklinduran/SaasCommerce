using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Abstractions;

public interface IInventoryReadRepository
{
  Task<int> CountAsync(
    BusinessId businessId,
    InventoryReadCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<StockItemResponse>> ListAsync(
    BusinessId businessId,
    InventoryReadCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<InventoryProductDetailResponse?> GetProductDetailAsync(
    BusinessId businessId,
    Guid productId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<StockItemResponse>> ExportAllAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}

public sealed record InventoryReadCriteria(
  Guid? ProductId,
  Guid? BranchId,
  string? Search,
  bool LowStockOnly,
  bool OutOfStockOnly,
  int Page,
  int PageSize,
  StockSortOption SortBy,
  InventorySortDirection SortDirection);
