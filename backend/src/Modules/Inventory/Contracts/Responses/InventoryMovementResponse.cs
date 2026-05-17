namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryMovementResponse(
  Guid Id,
  Guid BusinessId,
  Guid BranchId,
  string? BranchName,
  Guid ProductId,
  string? ProductName,
  decimal PreviousStock,
  decimal NewStock,
  decimal Quantity,
  string Reason,
  Guid? SaleId,
  string? Note,
  Guid UserId,
  DateTimeOffset CreatedAt);
