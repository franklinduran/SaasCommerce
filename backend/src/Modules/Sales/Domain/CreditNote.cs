using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class CreditNote
{
  private readonly List<CreditNoteItem> items = [];

  private CreditNote()
  {
  }

  private static void ValidateIdentity(Guid id, Guid saleId, Guid saleReturnId, string code)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Credit note id is required.", nameof(id));
    }

    if (saleId == Guid.Empty)
    {
      throw new ArgumentException("Sale id is required.", nameof(saleId));
    }

    if (saleReturnId == Guid.Empty)
    {
      throw new ArgumentException("Return id is required.", nameof(saleReturnId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(code);
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid SaleId { get; private set; }

  public Guid SaleReturnId { get; private set; }

  public Guid? CustomerId { get; private set; }

  public string Code { get; private set; } = string.Empty;

  public decimal Total { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public IReadOnlyCollection<CreditNoteItem> Items => items.AsReadOnly();

  public static CreditNote Generate(
    Guid id,
    Sale sale,
    SaleReturn saleReturn,
    string code,
    DateTimeOffset createdAt)
  {
    ArgumentNullException.ThrowIfNull(sale);
    ArgumentNullException.ThrowIfNull(saleReturn);

    if (saleReturn.Status != SaleReturnStatus.Approved)
    {
      throw new InvalidOperationException("Only approved returns can generate credit notes.");
    }

    ValidateIdentity(id, sale.Id, saleReturn.Id, code);

    var note = new CreditNote
    {
      Id = id,
      BusinessId = saleReturn.BusinessId,
      BranchId = saleReturn.BranchId,
      SaleId = sale.Id,
      SaleReturnId = saleReturn.Id,
      CustomerId = sale.CustomerId,
      Code = code.Trim(),
      CreatedAt = createdAt
    };

    foreach (var returnItem in saleReturn.Items)
    {
      var item = new CreditNoteItem(
        Guid.NewGuid(),
        note.Id,
        returnItem.Id,
        returnItem.ProductId,
        returnItem.Quantity,
        returnItem.UnitPrice);

      note.items.Add(item);
      note.Total += item.LineTotal;
    }

    return note;
  }
}
