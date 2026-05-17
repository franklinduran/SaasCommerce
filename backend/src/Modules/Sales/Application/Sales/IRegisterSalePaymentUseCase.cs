using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface IRegisterSalePaymentUseCase
{
  Task<Result> ExecuteAsync(
    PaymentRegistrationRequestedEventV1 paymentRegistrationRequested,
    CancellationToken cancellationToken = default);
}
