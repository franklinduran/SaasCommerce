using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public interface IUpdateCustomerUseCase
{
  Task<Result<CustomerResponse>> ExecuteAsync(
    UpdateCustomerCommand command,
    CancellationToken cancellationToken = default);
}

public sealed record UpdateCustomerCommand(
  Guid CustomerId,
  string FirstName,
  string LastName,
  string? Phone,
  string? Email,
  bool IsActive,
  string? Cedula = null);
