using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Persistence;

public sealed class EfCatalogCategoryRepository(AppDbContext dbContext) : ICatalogCategoryRepository
{
  public Task<Category?> GetByIdAsync(
    BusinessId businessId,
    Guid categoryId,
    CancellationToken cancellationToken = default)
    => Categories(businessId)
      .SingleOrDefaultAsync(category => category.Id == categoryId, cancellationToken);

  public Task<Category?> GetByNameAsync(
    BusinessId businessId,
    string name,
    CancellationToken cancellationToken = default)
  {
    var normalizedName = name.Trim();

    return Categories(businessId)
      .SingleOrDefaultAsync(category => category.Name == normalizedName, cancellationToken);
  }

  public async Task<IReadOnlyCollection<Category>> ListAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => await Categories(businessId)
      .OrderBy(category => category.Name)
      .ToArrayAsync(cancellationToken);

  public Task AddAsync(Category category, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(category);

    return dbContext.Set<Category>().AddAsync(category, cancellationToken).AsTask();
  }

  private IQueryable<Category> Categories(BusinessId businessId)
    => dbContext.Set<Category>()
      .Where(category => category.BusinessId == businessId);
}
