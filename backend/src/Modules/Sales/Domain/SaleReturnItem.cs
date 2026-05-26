namespace SaasCommerce.Modules.Sales.Domain;

public sealed class SaleReturnItem
{
  private SaleReturnItem()
  {
  }

  internal SaleReturnItem(
    Guid id,
    Guid saleReturnId,
    Guid saleItemId,
    Guid productId,
    decimal quantity,
    decimal unitPrice,
    decimal? unitCost)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Return item id is required.", nameof(id));
    }

    if (saleReturnId == Guid.Empty)
    {
      throw new ArgumentException("Return id is required.", nameof(saleReturnId));
    }

    if (saleItemId == Guid.Empty)
    {
      throw new ArgumentException("Sale item id is required.", nameof(saleItemId));
    }

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
    SaleReturnId = saleReturnId;
    SaleItemId = saleItemId;
    ProductId = productId;
    Quantity = quantity;
    UnitPrice = unitPrice;
    UnitCost = unitCost;
  }

  public Guid Id { get; private set; }

  public Guid SaleReturnId { get; private set; }

  public Guid SaleItemId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }

  public decimal UnitPrice { get; private set; }

  public decimal? UnitCost { get; private set; }

  public decimal LineTotal => Quantity * UnitPrice;
}
