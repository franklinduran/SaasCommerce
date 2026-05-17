using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public interface IRegisterCustomerPaymentUseCase
{
  Task<Result<RegisterCustomerPaymentResponse>> ExecuteAsync(
    RegisterCustomerPaymentCommand command,
    CancellationToken cancellationToken = default);
}

public sealed record RegisterCustomerPaymentCommand(
  Guid CustomerId,
  decimal Amount,
  string? Note);
