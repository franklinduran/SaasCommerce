using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Cash;

public static class CashErrors
{
  public static readonly DomainError UserContextRequired =
    new("cash.user_context_required", "The authenticated user does not have a valid tenant context.");

  public static readonly DomainError SessionNotFound =
    new("cash.session_not_found", "The cash session was not found.");

  public static readonly DomainError SessionAlreadyOpen =
    new("cash.session_already_open", "A cash session is already open for this branch.");

  public static readonly DomainError NoOpenSession =
    new("cash.no_open_session", "There is no open cash session for this branch.");

  public static readonly DomainError SessionNotOpen =
    new("cash.session_not_open", "The cash session is not open.");

  public static readonly DomainError InvalidAmount =
    new("cash.invalid_amount", "The amount must be greater than zero.");

  public static readonly DomainError InvalidMovementType =
    new("cash.invalid_movement_type", "The movement type is invalid. Use 'CashIn' or 'CashOut'.");

  public static readonly DomainError InvalidClosingBalance =
    new("cash.invalid_closing_balance", "The closing balance cannot be negative.");
}
