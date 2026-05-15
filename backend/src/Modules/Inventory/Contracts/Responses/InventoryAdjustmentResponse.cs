namespace SaasCommerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryAdjustmentResponse(
  StockItemResponse StockItem,
  InventoryMovementResponse Movement);
