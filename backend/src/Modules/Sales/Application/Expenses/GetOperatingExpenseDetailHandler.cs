using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

public sealed record GetOperatingExpenseDetailQuery(Guid ExpenseId);

public sealed class GetOperatingExpenseDetailHandler(
  IOperatingExpenseRepository expenses,
  IExpenseCategoryRepository categories,
  ICurrentUserService currentUser)
{
  public async Task<Result<OperatingExpenseResponse>> Handle(
    GetOperatingExpenseDetailQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);

    var expense = await expenses.GetAsync(bId, query.ExpenseId, cancellationToken);
    if (expense is null)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.ExpenseNotFound);
    }

    var category = await categories.GetAsync(bId, expense.CategoryId, cancellationToken);
    return Result.Success(OperatingExpenseResponseMapper.ToResponse(expense, category?.Name ?? string.Empty));
  }
}
