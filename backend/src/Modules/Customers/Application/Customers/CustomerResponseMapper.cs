using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain;

namespace SaasCommerce.Modules.Customers.Application.Customers;

internal static class CustomerResponseMapper
{
  public static CustomerResponse ToResponse(Customer customer)
  {
    ArgumentNullException.ThrowIfNull(customer);

    return new CustomerResponse(
      customer.Id,
      customer.BusinessId.Value,
      customer.FullName,
      customer.Phone,
      customer.Email,
      customer.IsActive,
      customer.CreatedAt,
      customer.UpdatedAt,
      customer.DeactivatedAt);
  }
}
