using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Persistence;

public sealed class EfCatalogProductRepository(AppDbContext dbContext) : ICatalogProductRepository
{
  public Task<Product?> GetByIdAsync(
    Guid productId,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => Products(businessId)
      .SingleOrDefaultAsync(product => product.Id == productId, cancellationToken);

  public Task<Product?> GetBySkuAsync(
    string sku,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var normalizedSku = sku.Trim().ToUpperInvariant();

    return Products(businessId)
      .SingleOrDefaultAsync(product => product.Sku == normalizedSku, cancellationToken);
  }

  public Task<Product?> GetByBarcodeAsync(
    string barcode,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var normalizedBarcode = barcode.Trim();

    return Products(businessId)
      .SingleOrDefaultAsync(product => product.Barcode == normalizedBarcode, cancellationToken);
  }

  public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(product);

    await dbContext.Set<Product>().AddAsync(product, cancellationToken);
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    string? query,
    ProductType? productType,
    Guid? categoryId,
    bool? isActive,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Products(businessId), query, productType, categoryId, isActive)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<Product>> ListAsync(
    BusinessId businessId,
    string? query,
    ProductType? productType,
    Guid? categoryId,
    bool? isActive,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    => await ApplyFilters(Products(businessId), query, productType, categoryId, isActive)
      .OrderBy(product => product.Name)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToArrayAsync(cancellationToken);

  private IQueryable<Product> Products(BusinessId businessId)
    => dbContext.Set<Product>()
      .Where(product => product.BusinessId == businessId);

  private static IQueryable<Product> ApplyFilters(
    IQueryable<Product> query,
    string? searchText,
    ProductType? productType,
    Guid? categoryId,
    bool? isActive)
  {
    if (!string.IsNullOrWhiteSpace(searchText))
    {
      var term = searchText.Trim();
      var normalizedTerm = term.ToUpperInvariant();
      query = query.Where(product =>
        product.Name.Contains(term) ||
        product.SearchName.Contains(normalizedTerm) ||
        product.Sku.Contains(term) ||
        product.Barcode != null && product.Barcode.Contains(term));
    }

    if (productType.HasValue)
    {
      query = query.Where(product => product.ProductType == productType.Value);
    }

    if (categoryId.HasValue)
    {
      query = query.Where(product => product.CategoryId == categoryId.Value);
    }

    if (isActive.HasValue)
    {
      query = query.Where(product => product.IsActive == isActive.Value);
    }

    return query;
  }
}
