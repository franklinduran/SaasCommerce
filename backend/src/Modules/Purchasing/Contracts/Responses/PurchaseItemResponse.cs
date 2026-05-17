namespace SaasCommerce.Modules.Purchasing.Contracts.Responses;

public sealed record PurchaseItemResponse(
  Guid Id,
  Guid ProductId,
  string ProductName,
  string? Sku,
  decimal Quantity,
  decimal UnitCost,
  decimal Subtotal);
