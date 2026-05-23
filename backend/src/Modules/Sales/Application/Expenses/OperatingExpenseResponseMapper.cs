using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

internal static class OperatingExpenseResponseMapper
{
  internal static OperatingExpenseResponse ToResponse(
    OperatingExpense expense,
    string categoryName)
    => new(
      expense.Id,
      expense.BusinessId.Value,
      expense.BranchId.Value,
      expense.UserId,
      expense.CategoryId,
      categoryName,
      expense.Description,
      expense.Amount,
      expense.PaymentMethod.ToString(),
      expense.Status.ToString(),
      expense.ExpenseDate,
      expense.Notes,
      expense.CashSessionId,
      expense.CashMovementId,
      expense.PaidAt,
      expense.PaidByUserId,
      expense.CancelledAt,
      expense.CreatedAt,
      expense.UpdatedAt);

  internal static OperatingExpenseListResponse ToListResponse(
    OperatingExpense expense,
    string categoryName)
    => new(
      expense.Id,
      expense.BranchId.Value,
      expense.CategoryId,
      categoryName,
      expense.Description,
      expense.Amount,
      expense.PaymentMethod.ToString(),
      expense.Status.ToString(),
      expense.ExpenseDate,
      expense.CreatedAt);

  internal static ExpenseCategoryResponse ToCategoryResponse(ExpenseCategory category)
    => new(
      category.Id,
      category.BusinessId.Value,
      category.Name,
      category.IsActive,
      category.CreatedAt);
}
