namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryMovementResponse(
  Guid Id,
  Guid BusinessId,
  Guid ProductId,
  decimal PreviousStock,
  decimal NewStock,
  decimal Quantity,
  string Reason,
  Guid UserId,
  DateTimeOffset CreatedAt);
