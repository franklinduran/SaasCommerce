using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Domain;

namespace SaasCommerce.Modules.Catalog.Application.Products;

internal static class ProductResponseMapper
{
  public static ProductResponse ToResponse(Product product)
  {
    ArgumentNullException.ThrowIfNull(product);

    return new ProductResponse(
      product.Id,
      product.BusinessId.Value,
      product.ProductType.ToString(),
      product.CategoryId,
      product.BrandId,
      product.ParentProductId,
      product.Name,
      product.Description,
      product.Sku,
      product.Barcode,
      product.InternalCode,
      product.SupplierCode,
      product.UnitOfMeasure.ToString(),
      product.SalePrice,
      product.CostPrice,
      product.WholesalePrice,
      product.MinSalePrice,
      product.TaxCategory.ToString(),
      product.TaxRate,
      product.ProfitMargin,
      product.IsTaxIncluded,
      product.AllowsDiscount,
      product.TrackInventory,
      product.MinimumStock,
      product.MaximumStock,
      product.ReorderPoint,
      product.AllowNegativeStock,
      product.VariantName,
      product.AttributesJson,
      product.ImageUrl,
      product.IsActive);
  }
}
