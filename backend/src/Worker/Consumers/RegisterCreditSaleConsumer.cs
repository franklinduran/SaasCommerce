using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class RegisterCreditSaleConsumer(
  ILogger<RegisterCreditSaleConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IRegisterCreditSaleUseCase useCase)
  : DelegatingIntegrationEventConsumer<SaleCompletedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    SaleCompletedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
