using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Domain;

public sealed class StockItem
{
  private StockItem()
  {
  }

  public StockItem(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid productId,
    DateTimeOffset createdAt)
  {
    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    ProductId = productId;
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public InventoryMovement ApplyAdjustment( // NOSONAR S107 — movement requires quantity, reason, user, and optional sale/purchase context
    decimal quantity,
    InventoryMovementReason reason,
    Guid userId,
    bool allowNegativeStock,
    DateTimeOffset occurredAt,
    Guid? saleId = null,
    Guid? purchaseId = null,
    string? note = null)
  {
    if (quantity == 0)
    {
      throw new InvalidOperationException("Movement quantity cannot be zero.");
    }

    var previousStock = Quantity;
    var newStock = previousStock + quantity;

    if (newStock < 0 && !allowNegativeStock)
    {
      throw new InvalidOperationException("Stock cannot become negative.");
    }

    Quantity = newStock;
    UpdatedAt = occurredAt;

    return new InventoryMovement(
      Guid.NewGuid(),
      new InventoryMovementSnapshot(
        BusinessId,
        BranchId,
        ProductId,
        previousStock,
        newStock,
        quantity,
        reason,
        saleId,
        purchaseId,
        note),
      userId,
      occurredAt);
  }

  public bool IsLowStock(decimal? minimumStock)
    => minimumStock.HasValue && Quantity <= minimumStock.Value;
}
