namespace SaasCommerce.Modules.Inventory.Domain;

public sealed class InventoryTransferItem
{
  private InventoryTransferItem()
  {
  }

  public InventoryTransferItem(Guid transferId, Guid productId, decimal quantity)
  {
    if (quantity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(quantity), "Transfer item quantity must be greater than zero.");
    }

    TransferId = transferId;
    ProductId = productId;
    Quantity = quantity;
  }

  public Guid TransferId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }
}
