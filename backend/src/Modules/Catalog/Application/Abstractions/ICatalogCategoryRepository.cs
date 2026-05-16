using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Abstractions;

public interface ICatalogCategoryRepository
{
  Task<Category?> GetByIdAsync(
    BusinessId businessId,
    Guid categoryId,
    CancellationToken cancellationToken = default);

  Task<Category?> GetByNameAsync(
    BusinessId businessId,
    string name,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Category>> ListAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Category category, CancellationToken cancellationToken = default);
}
