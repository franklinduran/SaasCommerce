using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public interface IBranchRepository
{
  Task<Branch?> GetByIdAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Branch>> ListAsync(
    BusinessId businessId,
    bool? isActive,
    CancellationToken cancellationToken = default);

  Task<bool> ExistsByCodeAsync(
    BusinessId businessId,
    string code,
    BranchId? excludeId,
    CancellationToken cancellationToken = default);

  Task<bool> ExistsByNameAsync(
    BusinessId businessId,
    string name,
    BranchId? excludeId,
    CancellationToken cancellationToken = default);

  Task<int> CountActiveAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<bool> HasMainBranchAsync(
    BusinessId businessId,
    BranchId? excludeId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
}
