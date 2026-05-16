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

  public Task<int> CountStockAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<StockItem>()
      .CountAsync(
        stockItem => stockItem.BusinessId == businessId && stockItem.BranchId == branchId,
        cancellationToken);

  public async Task<IReadOnlyCollection<StockItem>> ListStockAsync(
    BusinessId businessId,
    BranchId branchId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    => await dbContext.Set<StockItem>()
      .AsNoTracking()
      .Where(stockItem => stockItem.BusinessId == businessId && stockItem.BranchId == branchId)
      .OrderBy(stockItem => stockItem.ProductId)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToArrayAsync(cancellationToken);

  public Task<int> CountMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid? productId,
    CancellationToken cancellationToken = default)
    => ApplyMovementFilters(dbContext.Set<InventoryMovement>().AsNoTracking(), businessId, branchId, productId)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<InventoryMovement>> ListMovementsAsync(
    BusinessId businessId,
    BranchId branchId,
    Guid? productId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    => await ApplyMovementFilters(dbContext.Set<InventoryMovement>().AsNoTracking(), businessId, branchId, productId)
      .OrderByDescending(movement => movement.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToArrayAsync(cancellationToken);

  private static IQueryable<InventoryMovement> ApplyMovementFilters(
    IQueryable<InventoryMovement> query,
    BusinessId businessId,
    BranchId branchId,
    Guid? productId)
  {
    query = query.Where(movement => movement.BusinessId == businessId && movement.BranchId == branchId);

    if (productId.HasValue)
    {
      query = query.Where(movement => movement.ProductId == productId.Value);
    }

    return query;
  }
}
