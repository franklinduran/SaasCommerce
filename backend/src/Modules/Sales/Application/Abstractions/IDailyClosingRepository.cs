using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface IDailyClosingRepository
{
  Task<DailyClosing?> GetByIdAsync(Guid id, BusinessId businessId, CancellationToken cancellationToken = default);

  Task<DailyClosing?> GetByDateAndBranchAsync(
    BusinessId businessId,
    BranchId branchId,
    DateOnly date,
    CancellationToken cancellationToken = default);

  Task AddAsync(DailyClosing closing, CancellationToken cancellationToken = default);

  Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
