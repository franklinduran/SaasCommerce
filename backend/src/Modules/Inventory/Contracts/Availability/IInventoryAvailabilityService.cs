namespace SaasCommerce.Modules.Inventory.Contracts.Availability;

public interface IInventoryAvailabilityService
{
  Task<InventoryAvailabilityResult> ValidateStockAsync(
    InventoryAvailabilityRequest request,
    CancellationToken cancellationToken = default);
}
