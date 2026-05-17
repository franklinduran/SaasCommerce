using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Inventory;

public sealed class EfProductPurchaseReader(AppDbContext dbContext) : IProductPurchaseReader
{
  public Task<ProductPurchaseInfo?> GetAsync(
    Guid businessId,
    Guid productId,
    CancellationToken cancellationToken = default)
    => Products(businessId)
      .Where(product => product.Id == productId)
      .Select(product => new ProductPurchaseInfo(
        product.Id,
        product.BusinessId.Value,
        product.Name,
        product.Sku,
        product.TrackInventory,
        product.AllowNegativeStock,
        product.CostPrice,
        product.IsActive))
      .SingleOrDefaultAsync(cancellationToken);

  public async Task<IReadOnlyDictionary<Guid, ProductPurchaseInfo>> ListAsync(
    Guid businessId,
    IReadOnlyCollection<Guid> productIds,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(productIds);

    if (productIds.Count == 0)
    {
      return new Dictionary<Guid, ProductPurchaseInfo>();
    }

    return await Products(businessId)
      .AsNoTracking()
      .Where(product => productIds.Contains(product.Id))
      .Select(product => new ProductPurchaseInfo(
        product.Id,
        product.BusinessId.Value,
        product.Name,
        product.Sku,
        product.TrackInventory,
        product.AllowNegativeStock,
        product.CostPrice,
        product.IsActive))
      .ToDictionaryAsync(product => product.ProductId, cancellationToken);
  }

  public async Task<ProductCostUpdate?> UpdateAverageCostAsync(
    Guid businessId,
    Guid productId,
    decimal currentStock,
    decimal purchasedQuantity,
    decimal unitCost,
    DateTimeOffset updatedAt,
    CancellationToken cancellationToken = default)
  {
    var product = await Products(businessId)
      .SingleOrDefaultAsync(candidate => candidate.Id == productId, cancellationToken);

    if (product is null)
    {
      return null;
    }

    var previousCost = product.CostPrice;
    product.UpdateAverageCost(currentStock, purchasedQuantity, unitCost, updatedAt);

    return new ProductCostUpdate(product.Id, previousCost, product.CostPrice);
  }

  private IQueryable<Product> Products(Guid businessId)
  {
    var tenantId = new BusinessId(businessId);

    return dbContext.Set<Product>()
      .Where(product => product.BusinessId == tenantId);
  }
}
