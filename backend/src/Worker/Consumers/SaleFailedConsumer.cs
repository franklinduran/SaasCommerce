using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class SaleFailedConsumer(
  ILogger<SaleFailedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IFailSaleUseCase useCase)
  : DelegatingIntegrationEventConsumer<SaleFailedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    SaleFailedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
