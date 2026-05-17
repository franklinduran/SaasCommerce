using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public static class CustomerCreditErrors
{
  public static readonly DomainError UserContextRequired =
    new("credits.user_context_required", "The authenticated user does not have a valid tenant context.");

  public static readonly DomainError CustomerNotFound =
    new("credits.customer_not_found", "The customer was not found.");

  public static readonly DomainError InvalidCreditOperation =
    new("credits.invalid_operation", "The credit operation is invalid.");

  public static readonly DomainError PaymentExceedsBalance =
    new("credits.payment_exceeds_balance", "The payment amount exceeds the pending balance.");

  public static readonly DomainError CreditAccountBlocked =
    new("credits.credit_blocked", "The customer's credit account is blocked.");

  public static readonly DomainError CreditLimitExceeded =
    new("credits.credit_limit_exceeded", "The credit sale exceeds the customer's credit limit.");

  public static readonly DomainError SaleNotFound =
    new("credits.sale_not_found", "The sale was not found.");
}
