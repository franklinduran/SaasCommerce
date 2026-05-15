using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public static class InventoryErrors
{
  public static readonly DomainError UserContextRequired = new(
    "inventory.user_context_required",
    "Authenticated user context is required.");

  public static readonly DomainError InvalidAdjustment = new(
    "inventory.invalid_adjustment",
    "Product, quantity and reason are required for an inventory adjustment.");

  public static readonly DomainError NegativeStock = new(
    "inventory.negative_stock",
    "Inventory adjustment cannot leave stock below zero.");

  public static readonly DomainError ProductNotFound = new(
    "inventory.product_not_found",
    "Product was not found for this business.");

  public static readonly DomainError ProductDoesNotTrackInventory = new(
    "inventory.product_does_not_track_inventory",
    "Inventory adjustments cannot be applied to products that do not track stock.");
}
