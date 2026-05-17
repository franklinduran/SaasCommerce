using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public interface IBlockCustomerCreditUseCase
{
  Task<Result<CustomerCreditSummaryResponse>> ExecuteAsync(
    BlockCustomerCreditCommand command,
    CancellationToken cancellationToken = default);
}

public sealed record BlockCustomerCreditCommand(Guid CustomerId);
