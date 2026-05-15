using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

internal static class InventoryResponseMapper
{
  public static StockItemResponse ToResponse(StockItem stockItem)
  {
    ArgumentNullException.ThrowIfNull(stockItem);

    return new StockItemResponse(
      stockItem.Id,
      stockItem.BusinessId.Value,
      stockItem.ProductId,
      stockItem.Quantity);
  }

  public static InventoryMovementResponse ToResponse(InventoryMovement movement)
  {
    ArgumentNullException.ThrowIfNull(movement);

    return new InventoryMovementResponse(
      movement.Id,
      movement.BusinessId.Value,
      movement.ProductId,
      movement.PreviousStock,
      movement.NewStock,
      movement.Quantity,
      movement.Reason.ToString(),
      movement.UserId,
      movement.CreatedAt);
  }
}
