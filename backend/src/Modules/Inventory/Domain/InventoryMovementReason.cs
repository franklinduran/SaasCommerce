namespace SaasCommerce.Modules.Inventory.Domain;

public enum InventoryMovementReason
{
  InitialStock = 1,
  PurchaseEntry = 2,
  PurchaseReceived = 6,
  SaleDeduction = 3,
  ManualAdjustment = 4,
  Return = 5
}
