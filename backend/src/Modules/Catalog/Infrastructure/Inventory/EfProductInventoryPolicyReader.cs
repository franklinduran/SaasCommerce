using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Inventory;

public sealed class EfProductInventoryPolicyReader(AppDbContext dbContext) :
  IProductInventoryPolicyReader,
  IInventoryProductLookupReader,
  IProductSalesPolicyReader
{
  public async Task<ProductInventoryPolicy?> GetAsync(
    Guid businessId,
    Guid productId,
    CancellationToken cancellationToken = default)
  {
    var tenantId = new BusinessId(businessId);
    var product = await dbContext.Set<Product>()
      .Where(candidate => candidate.BusinessId == tenantId && candidate.Id == productId)
      .Select(candidate => new
      {
        candidate.Id,
        BusinessId = candidate.BusinessId.Value,
        candidate.ProductType,
        candidate.TrackInventory,
        candidate.AllowNegativeStock,
        candidate.UnitOfMeasure,
        candidate.MinimumStock
      })
      .SingleOrDefaultAsync(cancellationToken);

    return product is null
      ? null
      : new ProductInventoryPolicy(
        product.Id,
        product.BusinessId,
        product.ProductType.ToString(),
        product.TrackInventory,
        product.AllowNegativeStock,
        product.UnitOfMeasure.ToString(),
        product.MinimumStock);
  }

  public async Task<IReadOnlyCollection<InventoryProductLookup>> SearchAsync(
    InventoryProductLookupQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    var tenantId = new BusinessId(query.BusinessId);
    var products = dbContext.Set<Product>()
      .AsNoTracking()
      .Where(product => product.BusinessId == tenantId && product.TrackInventory);

    if (!string.IsNullOrWhiteSpace(query.Search))
    {
      var term = query.Search.Trim();
      var normalizedTerm = term.ToUpperInvariant();
      products = products.Where(product =>
        product.Name.Contains(term) ||
        product.SearchName.Contains(normalizedTerm) ||
        product.Sku.Contains(term) ||
        product.Barcode != null && product.Barcode.Contains(term));
    }

    if (!string.IsNullOrWhiteSpace(query.ProductType) &&
        Enum.TryParse<ProductType>(query.ProductType, true, out var productType))
    {
      products = products.Where(product => product.ProductType == productType);
    }

    if (query.CategoryId.HasValue)
    {
      products = products.Where(product => product.CategoryId == query.CategoryId.Value);
    }

    return await products
      .OrderBy(product => product.Name)
      .Select(product => new InventoryProductLookup(
        product.Id,
        product.BusinessId.Value,
        product.Name,
        product.Sku,
        product.Barcode,
        product.ProductType.ToString(),
        product.CategoryId,
        product.UnitOfMeasure.ToString(),
        product.MinimumStock,
        product.ReorderPoint))
      .ToArrayAsync(cancellationToken);
  }

  public async Task<ProductSalesPolicy?> GetSalesPolicyAsync(
    Guid businessId,
    Guid productId,
    CancellationToken cancellationToken = default)
  {
    var tenantId = new BusinessId(businessId);
    var product = await dbContext.Set<Product>()
      .AsNoTracking()
      .Where(candidate => candidate.BusinessId == tenantId && candidate.Id == productId)
      .Select(candidate => new
      {
        candidate.Id,
        BusinessId = candidate.BusinessId.Value,
        candidate.Name,
        candidate.Sku,
        candidate.Barcode,
        candidate.ProductType,
        candidate.UnitOfMeasure,
        candidate.SalePrice,
        candidate.CostPrice,
        candidate.TaxCategory,
        candidate.TaxRate,
        candidate.IsTaxIncluded,
        candidate.AllowsDiscount,
        candidate.TrackInventory,
        candidate.AllowNegativeStock,
        candidate.MinimumStock,
        candidate.IsActive
      })
      .SingleOrDefaultAsync(cancellationToken);

    if (product is null)
    {
      return null;
    }

    var blockedReason = GetBlockedReason(product.IsActive, product.SalePrice);

    return new ProductSalesPolicy(
      product.Id,
      product.BusinessId,
      product.Name,
      product.Sku,
      product.Barcode,
      product.ProductType.ToString(),
      product.UnitOfMeasure.ToString(),
      product.SalePrice,
      product.TaxCategory.ToString(),
      product.TaxRate,
      product.IsTaxIncluded,
      product.AllowsDiscount,
      product.TrackInventory,
      product.AllowNegativeStock,
      product.IsActive,
      blockedReason is null,
      blockedReason,
      product.MinimumStock,
      product.CostPrice);
  }

  private static string? GetBlockedReason(bool isActive, decimal salePrice)
  {
    if (!isActive)
    {
      return "Product is inactive.";
    }

    return salePrice < 0
      ? "Product sale price is invalid."
      : null;
  }
}
