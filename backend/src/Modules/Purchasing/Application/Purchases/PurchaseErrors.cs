using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public static class PurchaseErrors
{
  public static readonly DomainError InvalidPurchase =
    new("purchases.invalid_purchase", "The purchase request is invalid.");

  public static readonly DomainError PurchaseNotFound =
    new("purchases.purchase_not_found", "The purchase was not found.");

  public static readonly DomainError InvalidPurchaseState =
    new("purchases.invalid_state", "The purchase cannot move to the requested state.");

  public static readonly DomainError UserContextRequired =
    new("purchases.user_context_required", "The authenticated user does not have a valid tenant context.");

  public static readonly DomainError SupplierNotFound =
    new("purchases.supplier_not_found", "The purchase supplier was not found.");

  public static readonly DomainError ProductNotFound =
    new("purchases.product_not_found", "One or more purchase products were not found.");

  public static readonly DomainError ProductDoesNotTrackInventory =
    new("purchases.product_does_not_track_inventory", "One or more purchase products do not track inventory.");
}
