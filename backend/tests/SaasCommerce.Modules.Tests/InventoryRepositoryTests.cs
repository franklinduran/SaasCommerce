#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class InventoryRepositoryTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);

  // ── HasSaleMovementAsync ─────────────────────────────────────────────────

  [Fact]
  public async Task HasSaleMovementAsync_ShouldReturnTrue_WhenSaleMovementExists()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var saleId = Guid.NewGuid();
    var movement = new InventoryMovement(
      Guid.NewGuid(),
      new InventoryMovementSnapshot(
        businessId, branchId, Guid.NewGuid(), 10, 7, -3,
        InventoryMovementReason.SaleDeduction, SaleId: saleId),
      Guid.NewGuid(),
      Now);
    dbContext.Add(movement);
    await dbContext.SaveChangesAsync();

    var repo = new EfInventoryRepository(dbContext);
    var result = await repo.HasSaleMovementAsync(businessId, branchId, saleId);

    result.Should().BeTrue();
  }

  [Fact]
  public async Task HasSaleMovementAsync_ShouldReturnFalse_WhenNoMatch()
  {
    await using var dbContext = CreateDbContext();
    var repo = new EfInventoryRepository(dbContext);

    var result = await repo.HasSaleMovementAsync(
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid());

    result.Should().BeFalse();
  }

  // ── HasPurchaseMovementAsync ─────────────────────────────────────────────

  [Fact]
  public async Task HasPurchaseMovementAsync_ShouldReturnTrue_WhenPurchaseMovementExists()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var purchaseId = Guid.NewGuid();
    var movement = new InventoryMovement(
      Guid.NewGuid(),
      new InventoryMovementSnapshot(
        businessId, branchId, Guid.NewGuid(), 0, 5, 5,
        InventoryMovementReason.PurchaseReceived, PurchaseId: purchaseId),
      Guid.NewGuid(),
      Now);
    dbContext.Add(movement);
    await dbContext.SaveChangesAsync();

    var repo = new EfInventoryRepository(dbContext);
    var result = await repo.HasPurchaseMovementAsync(businessId, branchId, purchaseId);

    result.Should().BeTrue();
  }

  [Fact]
  public async Task HasPurchaseMovementAsync_ShouldReturnFalse_WhenNoMatch()
  {
    await using var dbContext = CreateDbContext();
    var repo = new EfInventoryRepository(dbContext);

    var result = await repo.HasPurchaseMovementAsync(
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid());

    result.Should().BeFalse();
  }

  // ── CountStockAsync ──────────────────────────────────────────────────────

  [Fact]
  public async Task CountStockAsync_ShouldReturnCount_WhenNotLowStockOnly()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var productA = Guid.NewGuid();
    var productB = Guid.NewGuid();
    await SeedStockAsync(dbContext, businessId, branchId, productA, 10);
    await SeedStockAsync(dbContext, businessId, branchId, productB, 3);

    var repo = new EfInventoryRepository(dbContext);
    var criteria = new StockSearchCriteria(
      [productA, productB],
      RestrictToProductIds: true,
      LowStockOnly: false,
      OutOfStockOnly: false,
      new Dictionary<Guid, decimal?> { [productA] = 5, [productB] = 5 },
      1, 10,
      StockSortOption.ProductId,
      InventorySortDirection.Asc);

    var count = await repo.CountStockAsync(businessId, branchId, criteria);

    count.Should().Be(2);
  }

  [Fact]
  public async Task CountStockAsync_ShouldFilterLowStock_WhenLowStockOnly()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var productA = Guid.NewGuid();
    var productB = Guid.NewGuid();
    await SeedStockAsync(dbContext, businessId, branchId, productA, 10);
    await SeedStockAsync(dbContext, businessId, branchId, productB, 3);

    var repo = new EfInventoryRepository(dbContext);
    var criteria = new StockSearchCriteria(
      [productA, productB],
      RestrictToProductIds: true,
      LowStockOnly: true,
      OutOfStockOnly: false,
      new Dictionary<Guid, decimal?> { [productA] = 5, [productB] = 5 },
      1, 10,
      StockSortOption.ProductId,
      InventorySortDirection.Asc);

    var count = await repo.CountStockAsync(businessId, branchId, criteria);

    // Only productB (qty 3 <= min 5) is low stock
    count.Should().Be(1);
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static async Task SeedStockAsync(
    AppDbContext dbContext,
    BusinessId businessId,
    BranchId branchId,
    Guid productId,
    decimal quantity)
  {
    var stockItem = new StockItem(Guid.NewGuid(), businessId, branchId, productId, Now);
    stockItem.ApplyAdjustment(
      quantity, InventoryMovementReason.InitialStock, Guid.NewGuid(), false, Now);
    dbContext.Add(stockItem);
    await dbContext.SaveChangesAsync();
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }
}

#pragma warning restore CA1707
