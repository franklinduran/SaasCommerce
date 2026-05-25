using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ICashSessionRepository
{
  Task<CashSession?> GetAsync(
    BusinessId businessId,
    Guid cashSessionId,
    CancellationToken cancellationToken = default);

  /// <summary>Returns the currently open session for a branch, or null if none exists.</summary>
  Task<CashSession?> GetOpenSessionAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default);

  Task<bool> HasOpenSessionAsync(
    BusinessId businessId,
    BranchId branchId,
    CancellationToken cancellationToken = default);

  Task AddAsync(CashSession session, CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    CashSessionSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<CashSession>> ListAsync(
    BusinessId businessId,
    CashSessionSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<CashSession>> ExportAllAsync(
    BusinessId businessId,
    DateTimeOffset? dateFrom,
    DateTimeOffset? dateTo,
    CancellationToken cancellationToken = default);
}

public sealed record CashSessionSearchCriteria(
  Guid? BranchId,
  string? Status,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);
