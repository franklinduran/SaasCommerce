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
  string FullName,
  string? Phone,
  string? Email,
  bool IsActive);
