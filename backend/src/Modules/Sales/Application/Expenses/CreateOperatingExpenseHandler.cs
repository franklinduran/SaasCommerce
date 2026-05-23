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

public sealed record CreateOperatingExpenseCommand(
  Guid BranchId,
  Guid CategoryId,
  string Description,
  decimal Amount,
  string PaymentMethod,
  string Status,
  DateTimeOffset ExpenseDate,
  string? Notes);

public sealed class CreateOperatingExpenseHandler(
  IOperatingExpenseRepository expenses,
  IExpenseCategoryRepository categories,
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<OperatingExpenseResponse>> Handle(
    CreateOperatingExpenseCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.UserContextRequired);
    }

    // Validate payment method
    if (!Enum.TryParse<ExpensePaymentMethod>(command.PaymentMethod, ignoreCase: true, out var paymentMethod))
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.InvalidPaymentMethod);
    }

    // Validate status (only Pending or Paid accepted at creation)
    if (!Enum.TryParse<OperatingExpenseStatus>(command.Status, ignoreCase: true, out var status) ||
        status == OperatingExpenseStatus.Cancelled)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.InvalidStatus);
    }

    if (command.Amount <= 0)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.InvalidAmount);
    }

    if (string.IsNullOrWhiteSpace(command.Description))
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.InvalidExpense);
    }

    var bId = new BusinessId(businessId);
    var branchId = new BranchId(command.BranchId);

    // Validate category belongs to this business
    var category = await categories.GetAsync(bId, command.CategoryId, cancellationToken);
    if (category is null)
    {
      return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.CategoryNotFound);
    }

    var now = clock.UtcNow;
    OperatingExpense expense;
    Guid? cashSessionId = null;
    Guid? cashMovementId = null;

    // If paid with cash, require an open cash session and register CashOut
    if (status == OperatingExpenseStatus.Paid && paymentMethod == ExpensePaymentMethod.Cash)
    {
      var session = await cashSessions.GetOpenSessionAsync(bId, branchId, cancellationToken);
      if (session is null)
      {
        return Result.Failure<OperatingExpenseResponse>(ExpenseErrors.CashSessionRequired);
      }

      var movementDescription = $"Gasto: {command.Description.Trim()}";
      var movement = session.AddMovement(
        Guid.NewGuid(), userId, CashMovementType.CashOut,
        command.Amount, movementDescription, now);

      cashSessionId = session.Id;
      cashMovementId = movement.Id;

      expense = OperatingExpense.CreatePaid(
        Guid.NewGuid(), bId, branchId, userId,
        command.CategoryId, command.Description, command.Amount,
        paymentMethod, command.ExpenseDate, command.Notes,
        cashSessionId, cashMovementId, now);
    }
    else if (status == OperatingExpenseStatus.Paid)
    {
      // Non-cash immediate payment (transfer / card)
      expense = OperatingExpense.CreatePaid(
        Guid.NewGuid(), bId, branchId, userId,
        command.CategoryId, command.Description, command.Amount,
        paymentMethod, command.ExpenseDate, command.Notes,
        null, null, now);
    }
    else
    {
      // Pending
      expense = OperatingExpense.CreatePending(
        Guid.NewGuid(), bId, branchId, userId,
        command.CategoryId, command.Description, command.Amount,
        paymentMethod, command.ExpenseDate, command.Notes, now);
    }

    await expenses.AddAsync(expense, cancellationToken);

    await outbox.AddAsync(new OperatingExpenseCreatedEventV1(
      Guid.NewGuid(), Guid.NewGuid(),
      expense.Id, businessId, command.BranchId, userId,
      command.CategoryId, command.Description, command.Amount,
      expense.PaymentMethod.ToString(), expense.Status.ToString(),
      command.ExpenseDate, now), cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(OperatingExpenseResponseMapper.ToResponse(expense, category.Name));
  }
}
