using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

public sealed record CancelOperatingExpenseCommand(Guid ExpenseId);

public sealed class CancelOperatingExpenseHandler(
  IOperatingExpenseRepository expenses,
  IExpenseCategoryRepository categories,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<OperatingExpenseResponse>> Handle(
    CancelOperatingExpenseCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId ||
        currentUser.BranchId is not Guid branchIdValue)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);

    var expense = await expenses.GetAsync(bId, command.ExpenseId, cancellationToken);
    if (expense is null)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.ExpenseNotFound);
    }

    if (expense.Status == OperatingExpenseStatus.Paid)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.CannotCancelPaidExpense);
    }

    var now = clock.UtcNow;

    // Idempotent: already cancelled — return current state
    if (expense.Status == OperatingExpenseStatus.Cancelled)
    {
      var cat = await categories.GetAsync(bId, expense.CategoryId, cancellationToken);
      return Result.Success(OperatingExpenseResponseMapper.ToResponse(expense, cat?.Name ?? string.Empty));
    }

    expense.Cancel(now);

    await outbox.AddAsync(new OperatingExpenseCancelledEventV1(
      Guid.NewGuid(), Guid.NewGuid(),
      expense.Id, businessId, branchIdValue, userId,
      expense.Amount, now), cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var category = await categories.GetAsync(bId, expense.CategoryId, cancellationToken);
    return Result.Success(OperatingExpenseResponseMapper.ToResponse(expense, category?.Name ?? string.Empty));
  }
}
