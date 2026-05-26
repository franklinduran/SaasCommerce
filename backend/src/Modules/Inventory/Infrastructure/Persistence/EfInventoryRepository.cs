using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Persistence;

public sealed class EfInventoryRepository(AppDbContext dbContext) : IInventoryRepository
{
  public Task<StockItem?> GetStockItemAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid productId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<StockItem>()
      .SingleOrDefaultAsync(
        stockItem => stockItem.BusinessId == businessId &&
          stockItem.BranchId == branchId &&
          stockItem.ProductId == productId,
        cancellationToken);

  public Task AddStockItemAsync(
    StockItem stockItem,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(stockItem);

    return dbContext.Set<StockItem>().AddAsync(stockItem, cancellationToken).AsTask();
  }

  public Task AddMovementAsync(
    InventoryMovement movement,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(movement);

    return dbContext.Set<InventoryMovement>().AddAsync(movement, cancellationToken).AsTask();
  }

  public Task<bool> HasSaleMovementAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid saleId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<InventoryMovement>()
      .AsNoTracking()
      .AnyAsync(
        movement => movement.BusinessId == businessId &&
          movement.BranchId == branchId &&
          movement.SaleId == saleId,
        cancellationToken);

  public Task<bool> HasPurchaseMovementAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid purchaseId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<InventoryMovement>()
      .AsNoTracking()
      .AnyAsync(
        movement => movement.BusinessId == businessId &&
          movement.BranchId == branchId &&
          movement.PurchaseId == purchaseId,
        cancellationToken);

  public Task<bool> HasSaleReturnMovementAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<InventoryMovement>()
      .AsNoTracking()
      .AnyAsync(
        movement => movement.BusinessId == businessId &&
          movement.BranchId == branchId &&
          movement.ReturnId == saleReturnId,
        cancellationToken);

  public async Task<int> CountStockAsync(
    BusinessId businessId,
    BranchId? branchId,
    StockSearchCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(criteria);

    var query = ApplyStockBaseFilters(
      dbContext.Set<StockItem>().AsNoTracking(),
      businessId,
      branchId,
      criteria);

    if (!criteria.LowStockOnly)
    {
      return await query.CountAsync(cancellationToken);
    }

    var stockItems = await query.ToArrayAsync(cancellationToken);
    return FilterLowStock(stockItems, criteria).Length;
  }

  public async Task<IReadOnlyCollection<StockItem>> ListStockAsync(
    BusinessId businessId,
    BranchId? branchId,
    StockSearchCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(criteria);

    var query = ApplyStockBaseFilters(
      dbContext.Set<StockItem>().AsNoTracking(),
      businessId,
      branchId,
      criteria);

    if (criteria.LowStockOnly)
    {
      var stockItems = await query.ToArrayAsync(cancellationToken);
      return ApplyStockSorting(FilterLowStock(stockItems, criteria), criteria)
        .Skip((criteria.Page - 1) * criteria.PageSize)
        .Take(criteria.PageSize)
        .ToArray();
    }

    return await ApplyStockSorting(query, criteria)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);
  }

  public Task<int> CountMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    InventoryMovementSearchCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(criteria);

    return ApplyMovementFilters(
        dbContext.Set<InventoryMovement>().AsNoTracking(),
        businessId,
        branchId,
        criteria)
      .CountAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<InventoryMovement>> ListMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    InventoryMovementSearchCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(criteria);

    var query = ApplyMovementFilters(
      dbContext.Set<InventoryMovement>().AsNoTracking(),
      businessId,
      branchId,
      criteria);

    return await ApplyMovementSorting(query, criteria)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);
  }

  private static IQueryable<StockItem> ApplyStockBaseFilters(
    IQueryable<StockItem> query,
    BusinessId businessId,
    BranchId? branchId,
    StockSearchCriteria criteria)
  {
    query = query.Where(stockItem => stockItem.BusinessId == businessId);

    if (branchId.HasValue)
    {
      query = query.Where(stockItem => stockItem.BranchId == branchId.Value);
    }

    if (criteria.RestrictToProductIds)
    {
      query = query.Where(stockItem => criteria.ProductIds.Contains(stockItem.ProductId));
    }

    if (criteria.OutOfStockOnly)
    {
      query = query.Where(stockItem => stockItem.Quantity <= 0);
    }

    return query;
  }

  private static StockItem[] FilterLowStock(
    IReadOnlyCollection<StockItem> stockItems,
    StockSearchCriteria criteria)
    => stockItems
      .Where(stockItem =>
        criteria.MinimumStockByProduct.TryGetValue(stockItem.ProductId, out var minimumStock) &&
        minimumStock.HasValue &&
        stockItem.Quantity <= minimumStock.Value)
      .ToArray();

  private static IOrderedEnumerable<StockItem> ApplyStockSorting(
    IReadOnlyCollection<StockItem> stockItems,
    StockSearchCriteria criteria)
    => criteria.SortBy switch
    {
      StockSortOption.Quantity => criteria.SortDirection == InventorySortDirection.Desc
        ? stockItems.OrderByDescending(stockItem => stockItem.Quantity)
        : stockItems.OrderBy(stockItem => stockItem.Quantity),
      StockSortOption.CreatedAt => criteria.SortDirection == InventorySortDirection.Desc
        ? stockItems.OrderByDescending(stockItem => stockItem.CreatedAt)
        : stockItems.OrderBy(stockItem => stockItem.CreatedAt),
      _ => criteria.SortDirection == InventorySortDirection.Desc
        ? stockItems.OrderByDescending(stockItem => stockItem.ProductId)
        : stockItems.OrderBy(stockItem => stockItem.ProductId)
    };

  private static IQueryable<StockItem> ApplyStockSorting(
    IQueryable<StockItem> query,
    StockSearchCriteria criteria)
    => criteria.SortBy switch
    {
      StockSortOption.Quantity => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(stockItem => stockItem.Quantity)
        : query.OrderBy(stockItem => stockItem.Quantity),
      StockSortOption.CreatedAt => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(stockItem => stockItem.CreatedAt)
        : query.OrderBy(stockItem => stockItem.CreatedAt),
      _ => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(stockItem => stockItem.ProductId)
        : query.OrderBy(stockItem => stockItem.ProductId)
    };

  private static IQueryable<InventoryMovement> ApplyMovementFilters(
    IQueryable<InventoryMovement> query,
    BusinessId businessId,
    BranchId branchId,
    InventoryMovementSearchCriteria criteria)
  {
    query = query.Where(movement => movement.BusinessId == businessId && movement.BranchId == branchId);

    if (criteria.ProductId.HasValue)
    {
      query = query.Where(movement => movement.ProductId == criteria.ProductId.Value);
    }

    if (criteria.MovementType.HasValue)
    {
      query = query.Where(movement => movement.Reason == criteria.MovementType.Value);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(movement => movement.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(movement => movement.CreatedAt <= criteria.DateTo.Value);
    }

    return query;
  }

  private static IQueryable<InventoryMovement> ApplyMovementSorting(
    IQueryable<InventoryMovement> query,
    InventoryMovementSearchCriteria criteria)
    => criteria.SortBy switch
    {
      InventoryMovementSortOption.ProductId => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(movement => movement.ProductId)
        : query.OrderBy(movement => movement.ProductId),
      InventoryMovementSortOption.Quantity => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(movement => movement.Quantity)
        : query.OrderBy(movement => movement.Quantity),
      _ => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(movement => movement.CreatedAt)
        : query.OrderBy(movement => movement.CreatedAt)
    };
}
