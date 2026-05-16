namespace SaasCommerce.Modules.Inventory.Contracts.Availability;

public sealed record InventoryAvailabilityResult(
  Guid BusinessId,
  Guid BranchId,
  Guid ProductId,
  decimal RequestedQuantity,
  decimal AvailableQuantity,
  bool IsAvailable,
  string? ReasonIfUnavailable);
