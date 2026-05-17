using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public interface IGetCustomerByIdUseCase
{
  Task<Result<CustomerResponse>> ExecuteAsync(
    GetCustomerByIdQuery query,
    CancellationToken cancellationToken = default);
}

public sealed record GetCustomerByIdQuery(Guid CustomerId);
