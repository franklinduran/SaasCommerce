using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ICashRegisterRepository
{
  Task<CashRegister?> GetAsync(BusinessId businessId, Guid cashRegisterId, CancellationToken ct);

  Task<CashRegister?> GetOpenRegisterAsync(BusinessId businessId, Guid userId, CancellationToken ct);

  Task<bool> HasOpenRegisterAsync(BusinessId businessId, Guid userId, CancellationToken ct);

  Task AddAsync(CashRegister cashRegister, CancellationToken ct);

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

public sealed record CashRegisterSearchCriteria(
  Guid? BranchId,
  string? Status,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);
