namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public static class InventoryTransferRealtimeEvents
{
  public const string Created = "inventoryTransfer.created";
  public const string Completed = "inventoryTransfer.completed";
  public const string Failed = "inventoryTransfer.failed";
  public const string Cancelled = "inventoryTransfer.cancelled";
}
