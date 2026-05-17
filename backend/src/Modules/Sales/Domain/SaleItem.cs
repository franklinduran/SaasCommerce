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
    decimal unitPrice)
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

    Id = id;
    SaleId = saleId;
    ProductId = productId;
    Quantity = quantity;
    UnitPrice = unitPrice;
  }

  public Guid Id { get; private set; }

  public Guid SaleId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }

  public decimal UnitPrice { get; private set; }

  public decimal LineTotal => Quantity * UnitPrice;
}
