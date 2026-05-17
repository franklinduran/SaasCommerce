using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public interface IGetCustomerCreditMovementsUseCase
{
  Task<Result<CustomerCreditMovementListResponse>> ExecuteAsync(
    GetCustomerCreditMovementsQuery query,
    CancellationToken cancellationToken = default);
}

public sealed record GetCustomerCreditMovementsQuery(
  Guid CustomerId,
  int Page,
  int PageSize);
