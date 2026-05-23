using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

public sealed record GetExpenseSummaryQuery(
  DateTimeOffset DateFrom,
  DateTimeOffset DateTo,
  Guid? BranchId);

public sealed class GetExpenseSummaryHandler(
  IOperatingExpenseRepository expenses,
  IExpenseCategoryRepository categories,
  ICurrentUserService currentUser)
{
  public async Task<Result<ExpenseSummaryResponse>> Handle(
    GetExpenseSummaryQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ExpenseSummaryResponse>(ExpenseErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);

    var list = await expenses.ListForSummaryAsync(
      bId, query.DateFrom, query.DateTo, query.BranchId, cancellationToken);

    // Only include paid expenses in monetary totals
    var paid = list.Where(e => e.Status == OperatingExpenseStatus.Paid).ToArray();
    var pending = list.Where(e => e.Status == OperatingExpenseStatus.Pending).ToArray();
    var cancelled = list.Where(e => e.Status == OperatingExpenseStatus.Cancelled).ToArray();

    var totalAmount = paid.Sum(e => e.Amount);
    var pendingAmount = pending.Sum(e => e.Amount);

    // Load categories for naming
    var allCategories = await categories.ListAsync(bId, cancellationToken);
    var categoryMap = allCategories.ToDictionary(c => c.Id, c => c.Name);

    // Group paid by category
    var byCategory = paid
      .GroupBy(e => e.CategoryId)
      .Select(g => new ExpenseByCategoryResponse(
        g.Key,
        categoryMap.GetValueOrDefault(g.Key, "Sin categoría"),
        g.Sum(e => e.Amount),
        g.Count()))
      .OrderByDescending(x => x.TotalAmount)
      .ToArray();

    // Group paid by payment method
    var byPaymentMethod = paid
      .GroupBy(e => e.PaymentMethod.ToString())
      .Select(g => new ExpenseByPaymentMethodResponse(
        g.Key,
        g.Sum(e => e.Amount),
        g.Count()))
      .OrderByDescending(x => x.TotalAmount)
      .ToArray();

    var summary = new ExpenseSummaryResponse(
      DateFrom: query.DateFrom,
      DateTo: query.DateTo,
      TotalAmount: totalAmount,
      PendingAmount: pendingAmount,
      PaidCount: paid.Length,
      PendingCount: pending.Length,
      CancelledCount: cancelled.Length,
      ByCategory: byCategory,
      ByPaymentMethod: byPaymentMethod);

    return Result.Success(summary);
  }
}
