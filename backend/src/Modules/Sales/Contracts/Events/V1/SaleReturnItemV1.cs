namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record SaleReturnItemV1(
  Guid SaleItemId,
  Guid ProductId,
  decimal Quantity,
  decimal UnitPrice,
  decimal LineTotal);
