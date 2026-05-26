namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record SaleItemResponse(
  Guid SaleItemId,
  Guid ProductId,
  string ProductName,
  string? Sku,
  decimal Quantity,
  decimal UnitPrice,
  decimal LineTotal)
{
  public decimal Subtotal => LineTotal;
}
