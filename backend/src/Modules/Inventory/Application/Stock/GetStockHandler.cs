using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class GetStockHandler(
  IInventoryRepository inventory,
  IInventoryProductLookupReader productLookupReader,
  ICurrentUserService currentUser)
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<StockListResponse>> Handle(
    GetStockQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<StockListResponse>> HandleCoreAsync(
    GetStockQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.BranchId is not Guid branchId)
    {
      return Result.Failure<StockListResponse>(InventoryErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var currentBranchId = new BranchId(branchId);
    var page = Math.Max(1, query.Page);
    var pageSize = NormalizePageSize(query.PageSize);
    var products = await productLookupReader.SearchAsync(
      new InventoryProductLookupQuery(
        businessId,
        query.Search,
        query.ProductType,
        query.CategoryId),
      cancellationToken);
    var productsById = products.ToDictionary(product => product.ProductId);
    var criteria = new StockSearchCriteria(
      productsById.Keys.ToArray(),
      RestrictToProductIds: true,
      query.LowStockOnly,
      productsById.ToDictionary(product => product.Key, product => product.Value.MinimumStock),
      page,
      pageSize,
      ParseStockSort(query.SortBy),
      ParseSortDirection(query.SortDirection, InventorySortDirection.Asc));
    var total = await inventory.CountStockAsync(tenantId, currentBranchId, criteria, cancellationToken);
    var items = await inventory.ListStockAsync(tenantId, currentBranchId, criteria, cancellationToken);
    var totalPages = CalculateTotalPages(total, pageSize);

    return Result.Success(new StockListResponse(
      items.Select(item => InventoryResponseMapper.ToResponse(
        item,
        productsById.GetValueOrDefault(item.ProductId))).ToArray(),
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

  private static int CalculateTotalPages(int totalItems, int pageSize)
    => totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

  private static StockSortOption ParseStockSort(string? sortBy)
  {
    if (string.Equals(sortBy, "quantity", StringComparison.OrdinalIgnoreCase))
    {
      return StockSortOption.Quantity;
    }

    if (string.Equals(sortBy, "createdAt", StringComparison.OrdinalIgnoreCase))
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
