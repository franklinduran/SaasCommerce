using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;

namespace SaasCommerce.Modules.Customers.Application.Customers;

internal static class CustomerResponseMapper
{
  public static CustomerResponse ToResponse(
    Customer customer,
    CustomerCreditAccount? creditAccount = null)
  {
    ArgumentNullException.ThrowIfNull(customer);

    return new CustomerResponse(
      customer.Id,
      customer.BusinessId.Value,
      customer.FirstName,
      customer.LastName,
      customer.FullName,
      customer.Phone,
      customer.Email,
      customer.Cedula,
      customer.IsActive,
      customer.CreatedAt,
      customer.UpdatedAt,
      customer.DeactivatedAt,
      creditAccount?.CurrentBalance ?? 0,
      creditAccount?.CreditLimit ?? 0,
      creditAccount?.Status.ToString() ?? CustomerCreditStatus.Active.ToString());
  }
}
