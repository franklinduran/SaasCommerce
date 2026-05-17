namespace SaasCommerce.Modules.Inventory.Contracts.Requests;

public sealed record CreateInventoryAdjustmentRequest(
  Guid ProductId,
  decimal Quantity,
  string Reason,
  Guid? BranchId = null,
  string? Note = null);
