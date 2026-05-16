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
    ProductSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Product>> ListAsync(
    BusinessId businessId,
    ProductSearchCriteria criteria,
    CancellationToken cancellationToken = default);
}

public sealed record ProductSearchCriteria(
  string? Query,
  ProductType? ProductType,
  Guid? CategoryId,
  bool? IsActive,
  int Page,
  int PageSize);
