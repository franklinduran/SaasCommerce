using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Expenses;

public static class ExpenseErrors
{
  public static readonly DomainError UserContextRequired =
    new("expenses.user_context_required", "Se requiere contexto de usuario autenticado.");

  public static readonly DomainError ExpenseNotFound =
    new("expenses.expense_not_found", "El gasto no fue encontrado.");

  public static readonly DomainError CategoryNotFound =
    new("expenses.category_not_found", "La categoria de gasto no fue encontrada.");

  public static readonly DomainError DuplicateCategoryName =
    new("expenses.duplicate_category_name", "Ya existe una categoria con ese nombre en este negocio.");

  public static readonly DomainError InvalidAmount =
    new("expenses.invalid_amount", "El monto debe ser mayor a cero.");

  public static readonly DomainError InvalidPaymentMethod =
    new("expenses.invalid_payment_method", "El metodo de pago no es valido. Use: Cash, Transfer, Card.");

  public static readonly DomainError InvalidStatus =
    new("expenses.invalid_status", "El estado no es valido. Use: Pending, Paid.");

  public static readonly DomainError CashSessionRequired =
    new("expenses.cash_session_required", "Se requiere una sesion de caja abierta para registrar un gasto en efectivo.");

  public static readonly DomainError ExpenseAlreadyPaid =
    new("expenses.already_paid", "El gasto ya fue pagado.");

  public static readonly DomainError ExpenseAlreadyCancelled =
    new("expenses.already_cancelled", "El gasto ya fue cancelado.");

  public static readonly DomainError CannotCancelPaidExpense =
    new("expenses.cannot_cancel_paid", "No se puede cancelar un gasto que ya fue pagado.");

  public static readonly DomainError InvalidExpense =
    new("expenses.invalid_expense", "El gasto contiene datos invalidos.");
}
