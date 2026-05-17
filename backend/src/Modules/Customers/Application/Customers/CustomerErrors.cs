using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public static class CustomerErrors
{
  public static readonly DomainError UserContextRequired =
    new("customers.user_context_required", "The authenticated user does not have a valid tenant context.");

  public static readonly DomainError InvalidCustomer =
    new("customers.invalid_customer", "The customer request is invalid.");

  public static readonly DomainError CustomerNotFound =
    new("customers.customer_not_found", "The customer was not found.");
}
