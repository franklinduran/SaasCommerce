using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ICashRegisterReadRepository
{
  Task<CashRegister?> GetAsync(BusinessId businessId, Guid cashRegisterId, CancellationToken ct);

  Task<CashRegister?> GetOpenRegisterAsync(BusinessId businessId, Guid userId, CancellationToken ct);

  Task<IReadOnlyCollection<CashRegister>> ListAsync(
    BusinessId businessId,
    CashRegisterSearchCriteria criteria,
    CancellationToken ct);

  Task<int> CountAsync(BusinessId businessId, CashRegisterSearchCriteria criteria, CancellationToken ct);

  Task<IReadOnlyCollection<CashRegister>> GetDailySummaryAsync(
    BusinessId businessId,
    Guid? branchId,
    DateOnly summaryDate,
    CancellationToken ct);
}
