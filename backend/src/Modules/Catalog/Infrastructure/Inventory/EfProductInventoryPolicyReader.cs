using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Inventory;

public sealed class EfProductInventoryPolicyReader(AppDbContext dbContext) :
  IProductInventoryPolicyReader,
  IInventoryProductLookupReader
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
        candidate.UnitOfMeasure
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
        product.UnitOfMeasure.ToString());
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
}
