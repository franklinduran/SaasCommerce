using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

public sealed record GetOperatingExpensesQuery(
  Guid? BranchId,
  Guid? CategoryId,
  string? Status,
  string? PaymentMethod,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);

public sealed class GetOperatingExpensesHandler(
  IOperatingExpenseRepository expenses,
  IExpenseCategoryRepository categories,
  ICurrentUserService currentUser)
{
  public async Task<Result<OperatingExpensesListResult>> Handle(
    GetOperatingExpensesQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<OperatingExpensesListResult>(ExpenseErrors.UserContextRequired);
    }

    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);

    var bId = new BusinessId(businessId);

    var criteria = new OperatingExpenseSearchCriteria(
      query.BranchId,
      query.CategoryId,
      query.Status,
      query.PaymentMethod,
      query.DateFrom,
      query.DateTo,
      page,
      pageSize);

    var total = await expenses.CountAsync(bId, criteria, cancellationToken);
    var list = await expenses.ListAsync(bId, criteria, cancellationToken);

    // Bulk-load categories used by these expenses
    var categoryIds = list.Select(e => e.CategoryId).Distinct().ToHashSet();
    var allCategories = await categories.ListAsync(bId, cancellationToken);
    var categoryMap = allCategories
      .Where(c => categoryIds.Contains(c.Id))
      .ToDictionary(c => c.Id, c => c.Name);

    var items = list
      .Select(e => OperatingExpenseResponseMapper.ToListResponse(
        e, categoryMap.GetValueOrDefault(e.CategoryId, string.Empty)))
      .ToArray();

    var result = new OperatingExpensesListResult(items, total, page, pageSize);
    return Result.Success(result);
  }
}
