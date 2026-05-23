using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfExpenseCategoryRepository(AppDbContext dbContext) : IExpenseCategoryRepository
{
  public Task<ExpenseCategory?> GetAsync(
    BusinessId businessId,
    Guid categoryId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<ExpenseCategory>()
      .SingleOrDefaultAsync(
        c => c.BusinessId == businessId && c.Id == categoryId,
        cancellationToken);

  public Task<bool> ExistsByNameAsync(
    BusinessId businessId,
    string name,
    CancellationToken cancellationToken = default)
    => dbContext.Set<ExpenseCategory>()
      .AnyAsync(
        c => c.BusinessId == businessId && c.Name == name.Trim(),
        cancellationToken);

  public async Task AddAsync(
    ExpenseCategory category,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(category);

    await dbContext.Set<ExpenseCategory>().AddAsync(category, cancellationToken);
  }

  public async Task<IReadOnlyCollection<ExpenseCategory>> ListAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => await dbContext.Set<ExpenseCategory>()
      .AsNoTracking()
      .Where(c => c.BusinessId == businessId && c.IsActive)
      .OrderBy(c => c.Name)
      .ToArrayAsync(cancellationToken);
}
