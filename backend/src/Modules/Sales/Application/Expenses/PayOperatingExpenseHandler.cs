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

public sealed record PayOperatingExpenseCommand(
  Guid ExpenseId,
  string PaymentMethod);

public sealed class PayOperatingExpenseHandler(
  IOperatingExpenseRepository expenses,
  IExpenseCategoryRepository categories,
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<OperatingExpenseResponse>> Handle(
    PayOperatingExpenseCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId ||
        currentUser.BranchId is not Guid branchIdValue)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.UserContextRequired);
    }

    if (!Enum.TryParse<ExpensePaymentMethod>(command.PaymentMethod, ignoreCase: true, out var paymentMethod))
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.InvalidPaymentMethod);
    }

    var bId = new BusinessId(businessId);
    var branchId = new BranchId(branchIdValue);

    var expense = await expenses.GetAsync(bId, command.ExpenseId, cancellationToken);
    if (expense is null)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.ExpenseNotFound);
    }

    if (expense.Status == OperatingExpenseStatus.Cancelled)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.ExpenseAlreadyCancelled);
    }

    // Idempotent: already paid — return current state
    if (expense.Status == OperatingExpenseStatus.Paid)
    {
      var cat = await categories.GetAsync(bId, expense.CategoryId, cancellationToken);
      return Result.Success(OperatingExpenseResponseMapper.ToResponse(expense, cat?.Name ?? string.Empty));
    }

    var now = clock.UtcNow;
    Guid? cashSessionId = null;
    Guid? cashMovementId = null;

    if (paymentMethod == ExpensePaymentMethod.Cash)
    {
      var session = await cashSessions.GetOpenSessionAsync(bId, branchId, cancellationToken);
      if (session is null)
      {
        return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.CashSessionRequired);
      }

      var movementDescription = $"Pago gasto: {expense.Description}";
      var movement = session.AddMovement(
        Guid.NewGuid(), userId, CashMovementType.CashOut,
        expense.Amount, movementDescription, now);

      cashSessionId = session.Id;
      cashMovementId = movement.Id;
    }

    expense.Pay(userId, now, cashSessionId, cashMovementId);

    await outbox.AddAsync(new OperatingExpensePaidEventV1(
      Guid.NewGuid(), Guid.NewGuid(),
      expense.Id, businessId, branchIdValue, userId,
      expense.Amount, paymentMethod.ToString(),
      cashSessionId, cashMovementId, now), cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var category = await categories.GetAsync(bId, expense.CategoryId, cancellationToken);
    return Result.Success(OperatingExpenseResponseMapper.ToResponse(expense, category?.Name ?? string.Empty));
  }
}
