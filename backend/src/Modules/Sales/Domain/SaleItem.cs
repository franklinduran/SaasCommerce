namespace SaasCommerce.Modules.Sales.Domain;

public sealed class SaleItem
{
  private SaleItem()
  {
  }

  internal SaleItem(
    Guid id,
    Guid saleId,
    Guid productId,
    decimal quantity,
    decimal unitPrice,
    decimal? unitCost = null)
  {
    if (productId == Guid.Empty)
    {
      throw new ArgumentException("Product id is required.", nameof(productId));
    }

    if (quantity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
    }

    if (unitPrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
    }

    if (unitCost.HasValue && unitCost.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(unitCost), "Unit cost cannot be negative.");
    }

    Id = id;
    SaleId = saleId;
    ProductId = productId;
    Quantity = quantity;
    UnitPrice = unitPrice;
    UnitCost = unitCost;
  }

  public Guid Id { get; private set; }

  public Guid SaleId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }

  public decimal UnitPrice { get; private set; }

  /// <summary>
  /// Cost per unit at the time of sale. Null for historical sales migrated before this field was added.
  /// </summary>
  public decimal? UnitCost { get; private set; }

  public decimal LineTotal => Quantity * UnitPrice;

  public decimal LineCost => Quantity * (UnitCost ?? 0m);
}
