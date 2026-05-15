using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Domain;

public sealed class InventoryMovement
{
  private InventoryMovement()
  {
  }

  public InventoryMovement(
    Guid id,
    BusinessId businessId,
    Guid productId,
    decimal previousStock,
    decimal newStock,
    decimal quantity,
    InventoryMovementReason reason,
    Guid userId,
    DateTimeOffset createdAt)
  {
    Id = id;
    BusinessId = businessId;
    ProductId = productId;
    PreviousStock = previousStock;
    NewStock = newStock;
    Quantity = quantity;
    Reason = reason;
    UserId = userId;
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal PreviousStock { get; private set; }

  public decimal NewStock { get; private set; }

  public decimal Quantity { get; private set; }

  public InventoryMovementReason Reason { get; private set; }

  public Guid UserId { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }
}
