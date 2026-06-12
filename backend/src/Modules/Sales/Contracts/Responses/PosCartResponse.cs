using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record PosCartResponse(
  Guid CartId,
  IReadOnlyCollection<PosCartItemResponse> Items,
  decimal Subtotal)
{
  public static PosCartResponse From(PosCart cart)
  {
    var items = cart.Items
      .Select(i => new PosCartItemResponse(i.ProductId, i.Name, i.Sku, i.UnitPrice, i.Quantity))
      .ToArray();

    var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
    return new PosCartResponse(cart.Id, items, subtotal);
  }
}

public sealed record PosCartItemResponse(
  Guid ProductId,
  string Name,
  string Sku,
  decimal UnitPrice,
  int Quantity);
