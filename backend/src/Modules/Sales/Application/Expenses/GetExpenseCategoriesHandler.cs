using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

public sealed class GetExpenseCategoriesHandler(
  IExpenseCategoryRepository categories,
  ICurrentUserService currentUser)
{
  public async Task<Result<IReadOnlyCollection<ExpenseCategoryResponse>>> Handle(
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<IReadOnlyCollection<ExpenseCategoryResponse>>(ExpenseErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);
    var list = await categories.ListAsync(bId, cancellationToken);

    return Result.Success<IReadOnlyCollection<ExpenseCategoryResponse>>(
      list.Select(OperatingExpenseResponseMapper.ToCategoryResponse).ToArray());
  }
}
