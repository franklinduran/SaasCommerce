using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Persistence;

public sealed class EfInventoryReadRepository(AppDbContext dbContext) : IInventoryReadRepository
{
  private const string AvailableStatus = "Available";
  private const string LowStockStatus = "LowStock";
  private const string OutOfStockStatus = "OutOfStock";

  public Task<int> CountAsync(
    BusinessId businessId,
    InventoryReadCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(BuildRowsQuery(businessId, criteria), criteria).CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<StockItemResponse>> ListAsync(
    BusinessId businessId,
    InventoryReadCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query = ApplySorting(ApplyFilters(BuildRowsQuery(businessId, criteria), criteria), criteria);

    var rows = await query
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

    return rows
      .Select(row => new StockItemResponse(
        row.StockId,
        row.BusinessId,
        row.BranchId,
        row.BranchName,
        row.ProductId,
        row.ProductName,
        row.Sku,
        row.Barcode,
        row.UnitOfMeasure,
        row.Quantity,
        row.MinimumStock,
        row.ReorderPoint,
        row.IsLowStock,
        row.IsOutOfStock,
        GetStockStatus(row.IsLowStock, row.IsOutOfStock),
        row.LastUpdatedAt))
      .ToArray();
  }

  public async Task<InventoryProductDetailResponse?> GetProductDetailAsync(
    BusinessId businessId,
    Guid productId,
    CancellationToken cancellationToken = default)
  {
    var product = await dbContext.Set<Product>()
      .AsNoTracking()
      .Where(candidate => candidate.BusinessId == businessId && candidate.Id == productId && candidate.TrackInventory)
      .Select(candidate => new ProductProjection
      {
        ProductId = candidate.Id,
        ProductName = candidate.Name,
        Sku = candidate.Sku,
        Barcode = candidate.Barcode,
        UnitOfMeasure = candidate.UnitOfMeasure.ToString(),
        MinimumStock = candidate.MinimumStock,
        ReorderPoint = candidate.ReorderPoint
      })
      .SingleOrDefaultAsync(cancellationToken);

    if (product is null)
    {
      return null;
    }

    var branches = await dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(branch => branch.BusinessId == businessId && branch.IsActive)
      .OrderBy(branch => branch.Name)
      .Select(branch => new BranchProjection
      {
        BranchId = branch.Id.Value,
        BranchName = branch.Name
      })
      .ToArrayAsync(cancellationToken);

    var stockItems = await dbContext.Set<StockItem>()
      .AsNoTracking()
      .Where(stockItem => stockItem.BusinessId == businessId && stockItem.ProductId == productId)
      .Select(stockItem => new
      {
        BranchId = stockItem.BranchId.Value,
        stockItem.Quantity,
        LastUpdatedAt = stockItem.UpdatedAt ?? stockItem.CreatedAt
      })
      .ToDictionaryAsync(stockItem => stockItem.BranchId, cancellationToken);

    var branchStocks = branches
      .Select(branch =>
      {
        var quantity = stockItems.TryGetValue(branch.BranchId, out var stockItem)
          ? stockItem.Quantity
          : 0;
        var updatedAt = stockItems.TryGetValue(branch.BranchId, out stockItem)
          ? stockItem.LastUpdatedAt
          : (DateTimeOffset?)null;
        var status = InventoryResponseMapper.GetStockStatus(quantity, product.MinimumStock);

        return new InventoryBranchStockResponse(
          branch.BranchId,
          branch.BranchName,
          quantity,
          product.MinimumStock,
          status == LowStockStatus || status == OutOfStockStatus,
          status == OutOfStockStatus,
          status,
          updatedAt);
      })
      .ToArray();

    var movements = await ListRecentMovementsAsync(businessId, productId, cancellationToken);
    var alerts = branchStocks
      .Where(stock => stock.MinimumStock is decimal minimumStock && stock.CurrentStock <= minimumStock)
      .Select(stock => new InventoryAlertResponse(
        stock.BranchId,
        stock.BranchName,
        product.ProductId,
        product.ProductName,
        stock.CurrentStock,
        stock.MinimumStock!.Value,
        stock.IsOutOfStock ? OutOfStockStatus : LowStockStatus,
        stock.LastUpdatedAt ?? DateTimeOffset.UtcNow))
      .ToArray();

    return new InventoryProductDetailResponse(
      product.ProductId,
      product.ProductName,
      product.Sku,
      product.Barcode,
      product.UnitOfMeasure,
      product.MinimumStock,
      product.ReorderPoint,
      branchStocks,
      movements,
      alerts);
  }

  public async Task<IReadOnlyCollection<StockItemResponse>> ExportAllAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var criteria = new InventoryReadCriteria(
      null, null, null,
      LowStockOnly: false,
      OutOfStockOnly: false,
      Page: 1, PageSize: int.MaxValue,
      SortBy: StockSortOption.ProductId,
      SortDirection: InventorySortDirection.Asc);

    var rows = await BuildRowsQuery(businessId, criteria)
      .OrderBy(r => r.ProductName)
      .ThenBy(r => r.BranchName)
      .Take(10_000)
      .ToArrayAsync(cancellationToken);

    return rows
      .Select(row => new StockItemResponse(
        row.StockId,
        row.BusinessId,
        row.BranchId,
        row.BranchName,
        row.ProductId,
        row.ProductName,
        row.Sku,
        row.Barcode,
        row.UnitOfMeasure,
        row.Quantity,
        row.MinimumStock,
        row.ReorderPoint,
        row.IsLowStock,
        row.IsOutOfStock,
        GetStockStatus(row.IsLowStock, row.IsOutOfStock),
        row.LastUpdatedAt))
      .ToArray();
  }

  private IQueryable<InventoryRowProjection> BuildRowsQuery(BusinessId businessId, InventoryReadCriteria criteria)
  {
    var stockItems = dbContext.Set<StockItem>()
      .AsNoTracking()
      .Where(stockItem => stockItem.BusinessId == businessId);

    if (criteria.ProductId.HasValue)
    {
      var productId = criteria.ProductId.Value;
      stockItems = stockItems.Where(stockItem => stockItem.ProductId == productId);
    }

    if (criteria.BranchId.HasValue)
    {
      var branchId = new BranchId(criteria.BranchId.Value);
      stockItems = stockItems.Where(stockItem => stockItem.BranchId == branchId);
    }

    var query =
      from stockItem in stockItems
      join product in dbContext.Set<Product>().AsNoTracking()
        on new { stockItem.ProductId, stockItem.BusinessId }
        equals new { ProductId = product.Id, product.BusinessId }
      join branch in dbContext.Set<Branch>().AsNoTracking()
        on new { stockItem.BranchId, stockItem.BusinessId }
        equals new { BranchId = branch.Id, branch.BusinessId }
      where stockItem.BusinessId == businessId && product.TrackInventory
      select new InventoryRowProjection
      {
        StockId = stockItem.Id,
        BusinessId = stockItem.BusinessId.Value,
        BranchId = stockItem.BranchId.Value,
        BranchName = branch.Name,
        ProductId = stockItem.ProductId,
        ProductName = product.Name,
        SearchName = product.SearchName,
        Sku = product.Sku,
        Barcode = product.Barcode,
        UnitOfMeasure = product.UnitOfMeasure.ToString(),
        Quantity = stockItem.Quantity,
        MinimumStock = product.MinimumStock,
        ReorderPoint = product.ReorderPoint,
        IsLowStock = product.MinimumStock != null && stockItem.Quantity <= product.MinimumStock,
        IsOutOfStock = stockItem.Quantity <= 0,
        LastUpdatedAt = stockItem.UpdatedAt ?? stockItem.CreatedAt
      };

    return query;
  }

  private static IQueryable<InventoryRowProjection> ApplyFilters(
    IQueryable<InventoryRowProjection> query,
    InventoryReadCriteria criteria)
  {
    if (!string.IsNullOrWhiteSpace(criteria.Search))
    {
      var term = criteria.Search.Trim();
      var normalizedTerm = term.ToUpperInvariant();

      query = query.Where(row =>
        row.ProductName.Contains(term) ||
        row.SearchName.Contains(normalizedTerm) ||
        row.Sku.Contains(term) ||
        row.Barcode != null && row.Barcode.Contains(term) ||
        row.ProductId.ToString().Contains(term));
    }

    if (criteria.LowStockOnly)
    {
      query = query.Where(row => row.IsLowStock);
    }

    if (criteria.OutOfStockOnly)
    {
      query = query.Where(row => row.IsOutOfStock);
    }

    return query;
  }

  private static IOrderedQueryable<InventoryRowProjection> ApplySorting(
    IQueryable<InventoryRowProjection> query,
    InventoryReadCriteria criteria)
    => criteria.SortBy switch
    {
      StockSortOption.Quantity => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(row => row.Quantity)
        : query.OrderBy(row => row.Quantity),
      StockSortOption.CreatedAt => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(row => row.LastUpdatedAt)
        : query.OrderBy(row => row.LastUpdatedAt),
      _ => criteria.SortDirection == InventorySortDirection.Desc
        ? query.OrderByDescending(row => row.ProductName)
        : query.OrderBy(row => row.ProductName)
    };

  private async Task<IReadOnlyCollection<InventoryMovementResponse>> ListRecentMovementsAsync(
    BusinessId businessId,
    Guid productId,
    CancellationToken cancellationToken)
  {
    var query =
      from movement in dbContext.Set<InventoryMovement>().AsNoTracking()
      join branch in dbContext.Set<Branch>().AsNoTracking()
        on new { movement.BranchId, movement.BusinessId }
        equals new { BranchId = branch.Id, branch.BusinessId }
      where movement.BusinessId == businessId && movement.ProductId == productId
      orderby movement.CreatedAt descending
      select new InventoryMovementResponse(
        movement.Id,
        movement.BusinessId.Value,
        movement.BranchId.Value,
        branch.Name,
        movement.ProductId,
        null,
        movement.PreviousStock,
        movement.NewStock,
        movement.Quantity,
        movement.Reason.ToString(),
        movement.SaleId,
        movement.PurchaseId,
        movement.Note,
        movement.UserId,
        movement.CreatedAt);

    return await query.Take(20).ToArrayAsync(cancellationToken);
  }

  private static string GetStockStatus(bool isLowStock, bool isOutOfStock)
  {
    if (isOutOfStock)
    {
      return OutOfStockStatus;
    }

    return isLowStock ? LowStockStatus : AvailableStatus;
  }

  private sealed class InventoryRowProjection
  {
    public Guid StockId { get; init; }
    public Guid BusinessId { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string SearchName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal? MinimumStock { get; init; }
    public decimal? ReorderPoint { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
  }

  private sealed class ProductProjection
  {
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? MinimumStock { get; init; }
    public decimal? ReorderPoint { get; init; }
  }

  private sealed class BranchProjection
  {
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
  }
}
