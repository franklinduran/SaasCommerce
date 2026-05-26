namespace SaasCommerce.Modules.Sales.Domain;

public sealed class CreditNoteItem
{
  private CreditNoteItem()
  {
  }

  internal CreditNoteItem(
    Guid id,
    Guid creditNoteId,
    Guid saleReturnItemId,
    Guid productId,
    decimal quantity,
    decimal unitPrice)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Credit note item id is required.", nameof(id));
    }

    if (creditNoteId == Guid.Empty)
    {
      throw new ArgumentException("Credit note id is required.", nameof(creditNoteId));
    }

    if (saleReturnItemId == Guid.Empty)
    {
      throw new ArgumentException("Return item id is required.", nameof(saleReturnItemId));
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

    Id = id;
    CreditNoteId = creditNoteId;
    SaleReturnItemId = saleReturnItemId;
    ProductId = productId;
    Quantity = quantity;
    UnitPrice = unitPrice;
  }

  public Guid Id { get; private set; }

  public Guid CreditNoteId { get; private set; }

  public Guid SaleReturnItemId { get; private set; }

  public Guid ProductId { get; private set; }

  public decimal Quantity { get; private set; }

  public decimal UnitPrice { get; private set; }

  public decimal LineTotal => Quantity * UnitPrice;
}
