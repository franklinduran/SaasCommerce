using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

internal static class InventoryResponseMapper
{
  public static StockItemResponse ToResponse(StockItem stockItem, InventoryProductLookup? product)
  {
    ArgumentNullException.ThrowIfNull(stockItem);

    return new StockItemResponse(
      stockItem.Id,
      stockItem.BusinessId.Value,
      stockItem.BranchId.Value,
      stockItem.ProductId,
      product?.Name ?? "Producto no encontrado",
      product?.Sku ?? string.Empty,
      product?.Barcode,
      product?.UnitOfMeasure ?? string.Empty,
      stockItem.Quantity,
      product?.MinimumStock,
      product?.ReorderPoint,
      product?.MinimumStock is decimal minimumStock && stockItem.Quantity <= minimumStock);
  }

  public static InventoryMovementResponse ToResponse(InventoryMovement movement)
  {
    ArgumentNullException.ThrowIfNull(movement);

    return new InventoryMovementResponse(
      movement.Id,
      movement.BusinessId.Value,
      movement.BranchId.Value,
      movement.ProductId,
      movement.PreviousStock,
      movement.NewStock,
      movement.Quantity,
      movement.Reason.ToString(),
      movement.UserId,
      movement.CreatedAt);
  }
}
