namespace SaasCommerce.Modules.Inventory.Contracts.Events.V1;

public sealed record LowStockDetectedNotificationV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid BranchId,
  Guid ProductId,
  string ProductName,
  decimal CurrentStock,
  decimal MinimumStock,
  DateTimeOffset CreatedAt);
