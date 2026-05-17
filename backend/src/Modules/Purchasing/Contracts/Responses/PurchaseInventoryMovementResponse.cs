namespace SaasCommerce.Modules.Purchasing.Contracts.Responses;

public sealed record PurchaseInventoryMovementResponse(
  Guid Id,
  Guid ProductId,
  decimal PreviousStock,
  decimal NewStock,
  decimal Quantity,
  string Reason,
  DateTimeOffset CreatedAt);
