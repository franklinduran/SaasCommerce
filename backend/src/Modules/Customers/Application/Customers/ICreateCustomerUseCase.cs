using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public interface ICreateCustomerUseCase
{
  Task<Result<CustomerResponse>> ExecuteAsync(
    CreateCustomerCommand command,
    CancellationToken cancellationToken = default);
}

public sealed record CreateCustomerCommand(
  string FullName,
  string? Phone,
  string? Email);
