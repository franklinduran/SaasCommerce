using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Inventory;

public sealed class EfProductInventoryPolicyReader(AppDbContext dbContext) : IProductInventoryPolicyReader
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
}
