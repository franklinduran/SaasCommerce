namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record SaleItemResponse(
  Guid ProductId,
  string ProductName,
  string? Sku,
  decimal Quantity,
  decimal UnitPrice,
  decimal LineTotal)
{
  public decimal Subtotal => LineTotal;
}
