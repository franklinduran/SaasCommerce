using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

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

  public Task AddAsync(Product product, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(product);

    return dbContext.Set<Product>().AddAsync(product, cancellationToken).AsTask();
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    ProductSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Products(businessId), criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<Product>> ListAsync(
    BusinessId businessId,
    ProductSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplyFilters(Products(businessId), criteria)
      .OrderBy(product => product.Name)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  private IQueryable<Product> Products(BusinessId businessId)
    => dbContext.Set<Product>()
      .Where(product => product.BusinessId == businessId);

  private static IQueryable<Product> ApplyFilters(
    IQueryable<Product> query,
    ProductSearchCriteria criteria)
  {
    if (!string.IsNullOrWhiteSpace(criteria.Query))
    {
      var term = criteria.Query.Trim();
      var normalizedTerm = term.ToUpperInvariant();
      query = query.Where(product =>
        product.Name.Contains(term) ||
        product.SearchName.Contains(normalizedTerm) ||
        product.Sku.Contains(term) ||
        product.Barcode != null && product.Barcode.Contains(term));
    }

    if (criteria.ProductType.HasValue)
    {
      query = query.Where(product => product.ProductType == criteria.ProductType.Value);
    }

    if (criteria.CategoryId.HasValue)
    {
      query = query.Where(product => product.CategoryId == criteria.CategoryId.Value);
    }

    if (criteria.IsActive.HasValue)
    {
      query = query.Where(product => product.IsActive == criteria.IsActive.Value);
    }

    return query;
  }
}
