namespace SaasCommerce.Modules.Inventory.Contracts.Availability;

public sealed record InventoryAvailabilityRequest(
  Guid BusinessId,
  Guid BranchId,
  Guid ProductId,
  decimal Quantity,
  bool TrackInventory,
  bool AllowNegativeStock);
