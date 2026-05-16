using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Domain;

public sealed class InventoryMovement
{
  private InventoryMovement()
  {
  }

  public InventoryMovement(
    Guid id,
    InventoryMovementSnapshot snapshot,
    Guid userId,
    DateTimeOffset createdAt)
  {
    Id = id;
    BusinessId = snapshot.BusinessId;
    BranchId = snapshot.BranchId;
    ProductId = snapshot.ProductId;
    PreviousStock = snapshot.PreviousStock;
    NewStock = snapshot.NewStock;
    Quantity = snapshot.Quantity;
    Reason = snapshot.Reason;
    UserId = userId;
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal PreviousStock { get; private set; }

  public decimal NewStock { get; private set; }

  public decimal Quantity { get; private set; }

  public InventoryMovementReason Reason { get; private set; }

  public Guid UserId { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }
}

public sealed record InventoryMovementSnapshot(
  BusinessId BusinessId,
  BranchId BranchId,
  Guid ProductId,
  decimal PreviousStock,
  decimal NewStock,
  decimal Quantity,
  InventoryMovementReason Reason);
