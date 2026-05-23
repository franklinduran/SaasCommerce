using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfOperatingExpenseRepository(AppDbContext dbContext) : IOperatingExpenseRepository
{
  public Task<OperatingExpense?> GetAsync(
    BusinessId businessId,
    Guid expenseId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<OperatingExpense>()
      .SingleOrDefaultAsync(
        e => e.BusinessId == businessId && e.Id == expenseId,
        cancellationToken);

  public async Task AddAsync(
    OperatingExpense expense,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(expense);

    await dbContext.Set<OperatingExpense>().AddAsync(expense, cancellationToken);
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    OperatingExpenseSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Expenses(businessId).AsNoTracking(), criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<OperatingExpense>> ListAsync(
    BusinessId businessId,
    OperatingExpenseSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplyFilters(Expenses(businessId).AsNoTracking(), criteria)
      .OrderByDescending(e => e.ExpenseDate)
      .ThenByDescending(e => e.CreatedAt)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  public async Task<IReadOnlyCollection<OperatingExpense>> ListForSummaryAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId,
    CancellationToken cancellationToken = default)
  {
    var query = Expenses(businessId)
      .AsNoTracking()
      .Where(e => e.ExpenseDate >= dateFrom && e.ExpenseDate <= dateTo);

    if (branchId.HasValue)
    {
      var bId = new BranchId(branchId.Value);
      query = query.Where(e => e.BranchId == bId);
    }

    return await query
      .OrderByDescending(e => e.ExpenseDate)
      .ToArrayAsync(cancellationToken);
  }

  private IQueryable<OperatingExpense> Expenses(BusinessId businessId)
    => dbContext.Set<OperatingExpense>()
      .Where(e => e.BusinessId == businessId);

  private static IQueryable<OperatingExpense> ApplyFilters(
    IQueryable<OperatingExpense> query,
    OperatingExpenseSearchCriteria criteria)
  {
    if (criteria.BranchId.HasValue)
    {
      var branchId = new BranchId(criteria.BranchId.Value);
      query = query.Where(e => e.BranchId == branchId);
    }

    if (criteria.CategoryId.HasValue)
    {
      query = query.Where(e => e.CategoryId == criteria.CategoryId.Value);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<OperatingExpenseStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(e => e.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(criteria.PaymentMethod) &&
        Enum.TryParse<ExpensePaymentMethod>(criteria.PaymentMethod, true, out var paymentMethod))
    {
      query = query.Where(e => e.PaymentMethod == paymentMethod);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(e => e.ExpenseDate >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(e => e.ExpenseDate <= criteria.DateTo.Value);
    }

    return query;
  }
}
