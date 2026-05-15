using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Catalog.Application.Products;

public static class CatalogErrors
{
  public static readonly DomainError UserContextRequired = new(
    "catalog.user_context_required",
    "Authenticated user context is required.");

  public static readonly DomainError ProductNotFound = new(
    "catalog.product_not_found",
    "Product was not found.");

  public static readonly DomainError DuplicateSku = new(
    "catalog.duplicate_sku",
    "A product with the same SKU already exists for this business.");

  public static readonly DomainError DuplicateBarcode = new(
    "catalog.duplicate_barcode",
    "A product with the same barcode already exists for this business.");

  public static readonly DomainError InvalidProduct = new(
    "catalog.invalid_product",
    "Product data is invalid for its type, pricing, unit or inventory policy.");
}
