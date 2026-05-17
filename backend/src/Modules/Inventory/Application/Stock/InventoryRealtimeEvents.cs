namespace SaasCommerce.Modules.Inventory.Application.Stock;

public static class InventoryRealtimeEvents
{
  public const string Adjusted = "inventory.adjusted";
  public const string LowStockDetected = "inventory.lowStockDetected";
  public const string StockChanged = "inventory.stockChanged";
}
