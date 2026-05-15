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
    Guid productId,
    DateTimeOffset createdAt)
  {
    Id = id;
    BusinessId = businessId;
    ProductId = productId;
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public InventoryMovement ApplyAdjustment(
    decimal quantity,
    InventoryMovementReason reason,
    Guid userId,
    bool allowNegativeStock,
    DateTimeOffset occurredAt)
  {
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
      BusinessId,
      ProductId,
      previousStock,
      newStock,
      quantity,
      reason,
      userId,
      occurredAt);
  }
}
