using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface IOperatingExpenseRepository
{
  Task<OperatingExpense?> GetAsync(
    BusinessId businessId,
    Guid expenseId,
    CancellationToken cancellationToken = default);

  Task AddAsync(OperatingExpense expense, CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    OperatingExpenseSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<OperatingExpense>> ListAsync(
    BusinessId businessId,
    OperatingExpenseSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<OperatingExpense>> ListForSummaryAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId,
    CancellationToken cancellationToken = default);
}

public sealed record OperatingExpenseSearchCriteria(
  Guid? BranchId,
  Guid? CategoryId,
  string? Status,
  string? PaymentMethod,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);
