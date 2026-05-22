namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record InventoryTransferStatusChangedNotificationV1(
  Guid TransferId,
  Guid BusinessId,
  Guid SourceBranchId,
  Guid TargetBranchId,
  string Status,
  string? Reason,
  DateTimeOffset OccurredAt);
