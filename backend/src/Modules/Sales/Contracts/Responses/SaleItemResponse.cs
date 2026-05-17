namespace SaasCommerce.Modules.Sales.Contracts.Responses;

public sealed record SaleItemResponse(
  Guid ProductId,
  decimal Quantity,
  decimal UnitPrice,
  decimal LineTotal);
