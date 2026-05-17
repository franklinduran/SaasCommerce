namespace SaasCommerce.Modules.Purchasing.Domain;

public sealed class PurchaseItem
{
  private PurchaseItem()
  {
  }

  internal PurchaseItem(
    Guid id,
    Guid purchaseId,
    Guid productId,
    decimal quantity,
    decimal unitCost)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Purchase item id is required.", nameof(id));
    }

    if (purchaseId == Guid.Empty)
    {
      throw new ArgumentException("Purchase id is required.", nameof(purchaseId));
    }

    if (productId == Guid.Empty)
    {
      throw new ArgumentException("Product id is required.", nameof(productId));
    }

    if (quantity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
    }

    if (unitCost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(unitCost), "Unit cost cannot be negative.");
    }

    Id = id;
    PurchaseId = purchaseId;
    ProductId = productId;
    Quantity = quantity;
    UnitCost = unitCost;
  }

  public Guid Id { get; private set; }

  public Guid PurchaseId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }

  public decimal UnitCost { get; private set; }

  public decimal Subtotal => Quantity * UnitCost;
}
