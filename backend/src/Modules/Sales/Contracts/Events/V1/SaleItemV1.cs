namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record SaleItemV1(
  Guid ProductId,
  decimal Quantity,
  decimal UnitPrice);
