using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

public sealed record CreateExpenseCategoryCommand(string Name);

public sealed class CreateExpenseCategoryHandler(
  IExpenseCategoryRepository categories,
  ICurrentUserService currentUser,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<ExpenseCategoryResponse>> Handle(
    CreateExpenseCategoryCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ExpenseCategoryResponse>(ExpenseErrors.UserContextRequired);
    }

    if (string.IsNullOrWhiteSpace(command.Name))
    {
      return Result.Failure<ExpenseCategoryResponse>(ExpenseErrors.InvalidExpense);
    }

    var bId = new BusinessId(businessId);

    var exists = await categories.ExistsByNameAsync(bId, command.Name, cancellationToken);
    if (exists)
    {
      return Result.Failure<ExpenseCategoryResponse>(ExpenseErrors.DuplicateCategoryName);
    }

    var category = ExpenseCategory.Create(Guid.NewGuid(), bId, command.Name, clock.UtcNow);
    await categories.AddAsync(category, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(OperatingExpenseResponseMapper.ToCategoryResponse(category));
  }
}
