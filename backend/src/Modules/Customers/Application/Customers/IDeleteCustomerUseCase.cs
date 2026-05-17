using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public interface IDeleteCustomerUseCase
{
  Task<Result<CustomerResponse>> ExecuteAsync(
    DeleteCustomerCommand command,
    CancellationToken cancellationToken = default);
}

public sealed record DeleteCustomerCommand(Guid CustomerId);
