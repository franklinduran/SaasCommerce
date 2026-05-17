namespace SaasCommerce.Modules.Purchasing.Contracts.Events.V1;

public sealed record PurchaseItemV1(
  Guid ProductId,
  decimal Quantity,
  decimal UnitCost,
  decimal Subtotal);
