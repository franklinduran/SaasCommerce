namespace SaasCommerce.Modules.Purchasing.Domain;

public enum PurchaseStatus
{
  Draft = 1,
  Received = 2,
  Processing = 3,
  InventoryUpdated = 4,
  Completed = 5,
  Failed = 6,
  Cancelled = 7
}
