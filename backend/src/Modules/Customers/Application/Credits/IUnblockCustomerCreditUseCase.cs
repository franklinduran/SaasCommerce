using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public interface IUnblockCustomerCreditUseCase
{
  Task<Result<CustomerCreditSummaryResponse>> ExecuteAsync(
    UnblockCustomerCreditCommand command,
    CancellationToken cancellationToken = default);
}

public sealed record UnblockCustomerCreditCommand(Guid CustomerId);
