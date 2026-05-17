using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class GetInventoryHandler(
  IInventoryReadRepository inventory,
  ICurrentUserService currentUser)
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<InventoryListResponse>> Handle(
    GetInventoryQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<InventoryListResponse>> HandleCoreAsync(
    GetInventoryQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<InventoryListResponse>(InventoryErrors.UserContextRequired);
    }

    var page = Math.Max(1, query.Page);
    var pageSize = NormalizePageSize(query.PageSize);
    var criteria = new InventoryReadCriteria(
      query.ProductId,
      query.BranchId,
      query.Search,
      query.LowStockOnly,
      query.OutOfStockOnly,
      page,
      pageSize,
      ParseStockSort(query.SortBy),
      ParseSortDirection(query.SortDirection, InventorySortDirection.Asc));
    var tenantId = new BusinessId(businessId);
    var total = await inventory.CountAsync(tenantId, criteria, cancellationToken);
    var items = await inventory.ListAsync(tenantId, criteria, cancellationToken);
    var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

    return Result.Success(new InventoryListResponse(
      items,
      page,
      pageSize,
      total,
      total,
      totalPages,
      page > 1,
      page < totalPages));
  }

  private static int NormalizePageSize(int pageSize)
    => AllowedPageSizes.Contains(pageSize) ? pageSize : 10;

  private static StockSortOption ParseStockSort(string? sortBy)
  {
    if (string.Equals(sortBy, "quantity", StringComparison.OrdinalIgnoreCase))
    {
      return StockSortOption.Quantity;
    }

    if (string.Equals(sortBy, "createdAt", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(sortBy, "lastUpdatedAt", StringComparison.OrdinalIgnoreCase))
    {
      return StockSortOption.CreatedAt;
    }

    return StockSortOption.ProductId;
  }

  private static InventorySortDirection ParseSortDirection(
    string? sortDirection,
    InventorySortDirection defaultDirection)
  {
    if (string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
    {
      return InventorySortDirection.Desc;
    }

    if (string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase))
    {
      return InventorySortDirection.Asc;
    }

    return defaultDirection;
  }
}
