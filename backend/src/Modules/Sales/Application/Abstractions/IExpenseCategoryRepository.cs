using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface IExpenseCategoryRepository
{
  Task<ExpenseCategory?> GetAsync(
    BusinessId businessId,
    Guid categoryId,
    CancellationToken cancellationToken = default);

  Task<bool> ExistsByNameAsync(
    BusinessId businessId,
    string name,
    CancellationToken cancellationToken = default);

  Task AddAsync(ExpenseCategory category, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<ExpenseCategory>> ListAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}
