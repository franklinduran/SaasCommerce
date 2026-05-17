namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record InventoryStockChangedNotificationV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid BranchId,
  Guid? ProductId,
  Guid? SaleId,
  string Reason,
  DateTimeOffset CreatedAt);
