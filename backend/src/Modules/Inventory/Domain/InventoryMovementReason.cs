namespace SaasCommerce.Modules.Inventory.Domain;

public enum InventoryMovementReason
{
  InitialLoad = 1,
  Purchase = 2,
  Sale = 3,
  Adjustment = 4,
  Return = 5,
  ManualCorrection = 6
}
