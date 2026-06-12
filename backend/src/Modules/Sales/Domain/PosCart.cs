using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class PosCart
{
  private readonly List<PosCartItem> _items = [];

  private PosCart() { }

  public PosCart(Guid id, BusinessId businessId, Guid userId, DateTimeOffset now)
  {
    Id = id;
    BusinessId = businessId;
    UserId = userId;
    UpdatedAt = now;
  }

  public Guid Id { get; private set; }
  public BusinessId BusinessId { get; private set; }
  public Guid UserId { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }
  public IReadOnlyCollection<PosCartItem> Items => _items.AsReadOnly();

  public void AddOrIncrement(
    Guid productId, string name, string sku,
    decimal unitPrice, int quantity, DateTimeOffset now)
  {
    var existing = _items.Find(i => i.ProductId == productId);

    if (existing is not null)
    {
      existing.Quantity += quantity;
    }
    else
    {
      _items.Add(new PosCartItem
      {
        Id = Guid.NewGuid(),
        CartId = Id,
        ProductId = productId,
        Name = name,
        Sku = sku,
        UnitPrice = unitPrice,
        Quantity = quantity,
      });
    }

    UpdatedAt = now;
  }

  public void SetQuantity(Guid productId, int quantity, DateTimeOffset now)
  {
    if (quantity <= 0)
    {
      RemoveItem(productId, now);
      return;
    }

    var existing = _items.Find(i => i.ProductId == productId);

    if (existing is not null)
    {
      existing.Quantity = quantity;
      UpdatedAt = now;
    }
  }

  public void RemoveItem(Guid productId, DateTimeOffset now)
  {
    var existing = _items.Find(i => i.ProductId == productId);

    if (existing is not null)
    {
      _items.Remove(existing);
      UpdatedAt = now;
    }
  }

  public void Clear(DateTimeOffset now)
  {
    _items.Clear();
    UpdatedAt = now;
  }
}

public sealed class PosCartItem
{
  public Guid Id { get; set; }
  public Guid CartId { get; set; }
  public Guid ProductId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Sku { get; set; } = string.Empty;
  public decimal UnitPrice { get; set; }
  public int Quantity { get; set; }
}
