namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public static class PurchaseRealtimeEvents
{
  public const string Received = "purchase.received";
  public const string Completed = "purchase.completed";
  public const string Failed = "purchase.failed";
  public const string PurchaseInventoryUpdated = "purchase.inventoryUpdated";
  public const string InventoryUpdated = "inventory.updated";
  public const string ProductCostUpdated = "product.costUpdated";
}
