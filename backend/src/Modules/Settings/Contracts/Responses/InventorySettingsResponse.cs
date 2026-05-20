namespace SaasCommerce.Modules.Settings.Contracts.Responses;

public sealed record InventorySettingsResponse(
  Guid BusinessId,
  bool EnableLowStockAlerts,
  decimal DefaultLowStockThreshold,
  bool RequireReasonForInventoryAdjustment,
  bool AllowInventoryTransferBetweenBranches,
  Guid UpdatedBy,
  DateTimeOffset UpdatedAt);
