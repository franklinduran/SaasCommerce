using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class RegisterPaymentConsumer(
  ILogger<RegisterPaymentConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IRegisterSalePaymentUseCase useCase)
  : DelegatingIntegrationEventConsumer<PaymentRegistrationRequestedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    PaymentRegistrationRequestedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
