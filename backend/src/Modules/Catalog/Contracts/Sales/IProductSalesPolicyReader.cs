namespace SaasCommerce.Modules.Catalog.Contracts.Sales;

public interface IProductSalesPolicyReader
{
  Task<ProductSalesPolicy?> GetSalesPolicyAsync(
    Guid businessId,
    Guid productId,
    CancellationToken cancellationToken = default);
}
