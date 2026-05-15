namespace SaasCommerce.Modules.Catalog.Contracts.Inventory;

public interface IProductInventoryPolicyReader
{
  Task<ProductInventoryPolicy?> GetAsync(
    Guid businessId,
    Guid productId,
    CancellationToken cancellationToken = default);
}
