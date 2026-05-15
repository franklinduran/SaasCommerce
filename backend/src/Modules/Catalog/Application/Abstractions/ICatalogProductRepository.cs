using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Abstractions;

public interface ICatalogProductRepository
{
  Task<Product?> GetByIdAsync(
    Guid productId,
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<Product?> GetBySkuAsync(
    string sku,
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<Product?> GetByBarcodeAsync(
    string barcode,
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Product product, CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    string? query,
    ProductType? productType,
    Guid? categoryId,
    bool? isActive,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Product>> ListAsync(
    BusinessId businessId,
    string? query,
    ProductType? productType,
    Guid? categoryId,
    bool? isActive,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);
}
