#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class InventoryDomainTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);

  // ── Product.UpdateAverageCost ────────────────────────────────────────────

  [Fact]
  public void UpdateAverageCost_ShouldComputeWeightedAverage_WhenValid()
  {
    var product = CreateProduct(costPrice: 10);

    // currentStock=10 @ cost 10, purchased 10 @ cost 20 → weighted avg 15
    product.UpdateAverageCost(10, 10, 20, Now);

    product.CostPrice.Should().Be(15);
    product.UpdatedAt.Should().Be(Now);
  }

  [Fact]
  public void UpdateAverageCost_ShouldRoundToTwoDecimals()
  {
    var product = CreateProduct(costPrice: 10);

    // currentStock=1 @ 10, purchased 2 @ 15 → (10 + 30)/3 = 13.333... → 13.33
    product.UpdateAverageCost(1, 2, 15, Now);

    product.CostPrice.Should().Be(13.33m);
  }

  [Fact]
  public void UpdateAverageCost_ShouldUseUnitCost_WhenTotalQuantityIsZeroAfterNegativeOffset()
  {
    var product = CreateProduct(costPrice: 10);

    // currentStock=0, purchased small positive — totalQuantity > 0 normal path,
    // here verify the branch where totalQuantity is non-zero still works with zero current stock
    product.UpdateAverageCost(0, 5, 25, Now);

    product.CostPrice.Should().Be(25);
  }

  [Fact]
  public void UpdateAverageCost_ShouldThrow_WhenCurrentStockIsNegative()
  {
    var product = CreateProduct(costPrice: 10);

    var act = () => product.UpdateAverageCost(-1, 5, 20, Now);

    act.Should().Throw<ArgumentOutOfRangeException>()
      .WithParameterName("currentStock");
  }

  [Fact]
  public void UpdateAverageCost_ShouldThrow_WhenPurchasedQuantityIsZeroOrNegative()
  {
    var product = CreateProduct(costPrice: 10);

    var act = () => product.UpdateAverageCost(10, 0, 20, Now);

    act.Should().Throw<ArgumentOutOfRangeException>()
      .WithParameterName("purchasedQuantity");
  }

  [Fact]
  public void UpdateAverageCost_ShouldThrow_WhenUnitCostIsNegative()
  {
    var product = CreateProduct(costPrice: 10);

    var act = () => product.UpdateAverageCost(10, 5, -1, Now);

    act.Should().Throw<ArgumentOutOfRangeException>()
      .WithParameterName("unitCost");
  }

  // ── InventoryMovement (snapshot constructor) ─────────────────────────────

  [Fact]
  public void InventoryMovement_ShouldSetAllProperties_WhenValid()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var saleId = Guid.NewGuid();
    var snapshot = new InventoryMovementSnapshot(
      businessId, branchId, productId, 10, 7, -3,
      InventoryMovementReason.SaleDeduction, SaleId: saleId, Note: "  sale deduction  ");

    var movement = new InventoryMovement(Guid.NewGuid(), snapshot, userId, Now);

    movement.BusinessId.Should().Be(businessId);
    movement.BranchId.Should().Be(branchId);
    movement.ProductId.Should().Be(productId);
    movement.PreviousStock.Should().Be(10);
    movement.NewStock.Should().Be(7);
    movement.Quantity.Should().Be(-3);
    movement.Reason.Should().Be(InventoryMovementReason.SaleDeduction);
    movement.SaleId.Should().Be(saleId);
    movement.Note.Should().Be("sale deduction");
    movement.UserId.Should().Be(userId);
    movement.CreatedAt.Should().Be(Now);
  }

  [Fact]
  public void InventoryMovement_ShouldThrow_WhenQuantityIsZero()
  {
    var snapshot = new InventoryMovementSnapshot(
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(), 5, 5, 0,
      InventoryMovementReason.ManualAdjustment);

    var act = () => new InventoryMovement(Guid.NewGuid(), snapshot, Guid.NewGuid(), Now);

    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void InventoryMovement_ShouldNormalizeBlankNoteToNull()
  {
    var snapshot = new InventoryMovementSnapshot(
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(), 0, 5, 5,
      InventoryMovementReason.InitialStock, Note: "   ");

    var movement = new InventoryMovement(Guid.NewGuid(), snapshot, Guid.NewGuid(), Now);

    movement.Note.Should().BeNull();
    movement.SaleId.Should().BeNull();
    movement.PurchaseId.Should().BeNull();
  }

  // ── StockItem.ApplyAdjustment ────────────────────────────────────────────

  [Fact]
  public void ApplyAdjustment_ShouldThrow_WhenQuantityIsZero()
  {
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      Now);

    var act = () => stockItem.ApplyAdjustment(
      0, InventoryMovementReason.ManualAdjustment, Guid.NewGuid(), false, Now);

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void ApplyAdjustment_ShouldAllowNegativeStock_WhenAllowed()
  {
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      Now);

    var movement = stockItem.ApplyAdjustment(
      -5, InventoryMovementReason.SaleDeduction, Guid.NewGuid(), allowNegativeStock: true, Now);

    stockItem.Quantity.Should().Be(-5);
    movement.NewStock.Should().Be(-5);
    movement.PreviousStock.Should().Be(0);
  }

  [Fact]
  public void ApplyAdjustment_ShouldThrow_WhenStockGoesNegativeAndNotAllowed()
  {
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      Now);

    var act = () => stockItem.ApplyAdjustment(
      -3, InventoryMovementReason.SaleDeduction, Guid.NewGuid(), allowNegativeStock: false, Now);

    act.Should().Throw<InvalidOperationException>();
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static Product CreateProduct(decimal costPrice)
    => new(
      new ProductCreationContext(Guid.NewGuid(), new BusinessId(Guid.NewGuid()), Now),
      new ProductIdentity(ProductType.Simple, "Producto", null, null, null, UnitOfMeasure.Unit),
      new ProductCodes("SKU-001", null, null, null),
      new ProductPricing(100, costPrice, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, null, null, null, false),
      new ProductOptions(true, null, null, null));
}

#pragma warning restore CA1707
