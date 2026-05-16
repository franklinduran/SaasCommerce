using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Availability;

public sealed class EfInventoryAvailabilityService(IInventoryRepository inventory) : IInventoryAvailabilityService
{
  public async Task<InventoryAvailabilityResult> ValidateStockAsync(
    InventoryAvailabilityRequest request,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (!request.TrackInventory)
    {
      return new InventoryAvailabilityResult(
        request.BusinessId,
        request.BranchId,
        request.ProductId,
        request.Quantity,
        0,
        true,
        null);
    }

    var stockItem = await inventory.GetStockItemAsync(
      new BusinessId(request.BusinessId),
      new BranchId(request.BranchId),
      request.ProductId,
      cancellationToken);
    var availableQuantity = stockItem?.Quantity ?? 0;
    var isAvailable = request.AllowNegativeStock || availableQuantity >= request.Quantity;

    return new InventoryAvailabilityResult(
      request.BusinessId,
      request.BranchId,
      request.ProductId,
      request.Quantity,
      availableQuantity,
      isAvailable,
      isAvailable ? null : "Insufficient stock.");
  }
}
