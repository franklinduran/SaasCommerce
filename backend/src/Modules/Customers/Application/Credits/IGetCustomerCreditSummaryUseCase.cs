using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public interface IGetCustomerCreditSummaryUseCase
{
  Task<Result<CustomerCreditSummaryResponse>> ExecuteAsync(
    GetCustomerCreditSummaryQuery query,
    CancellationToken cancellationToken = default);
}

public sealed record GetCustomerCreditSummaryQuery(Guid CustomerId);
